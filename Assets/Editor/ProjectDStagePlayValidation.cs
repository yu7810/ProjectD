using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Explicit batch-only integration checks. Never starts automatically in a normal editor session.
[InitializeOnLoad]
public static class ProjectDStagePlayValidation
{
    private const string Running = "ProjectD.StagePlayValidation.Running";
    private const string Phase = "ProjectD.StagePlayValidation.Phase";
    private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
    private static double deadline;

    static ProjectDStagePlayValidation()
    {
        if (Application.isBatchMode && SessionState.GetBool(Running, false))
        {
            deadline = EditorApplication.timeSinceStartup + 240;
            EditorApplication.update += Tick;
        }
    }

    public static void RunBatch()
    {
        try
        {
            ProjectDStageValidation.Validate();
            EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
            SessionState.SetInt(Phase, 0);
            SessionState.SetBool(Running, true);
            deadline = EditorApplication.timeSinceStartup + 240;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.EnterPlaymode();
        }
        catch (Exception error) { Fail(error); }
    }

    private static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Stage play validation timed out.");
            if (!EditorApplication.isPlaying || LevelCtrl.Instance == null) return;
            LevelCtrl controller = LevelCtrl.Instance;
            if ((bool)typeof(LevelCtrl).GetField("isLoadingLevel", PrivateInstance).GetValue(controller) || controller.CurrentRoom == null) return;
            int phase = SessionState.GetInt(Phase, 0);
            var doors = (Dictionary<Npc, StagePortalChoice>)typeof(LevelCtrl).GetField("offeredDoors", PrivateInstance).GetValue(controller);
            switch (phase)
            {
                case 0:
                    Require(controller.CurrentLevelIndex == 0 && doors.Count == 1, "Home must offer exactly one door.");
                    Enter(doors, 1); break;
                case 1:
                    CheckArrival(controller);
                    Require(controller.leftEnemy > 0 && doors.Count == 0, "Combat exits must be locked before clear.");
                    Clear(controller);
                    Require(doors.Count == 2 && doors.Values.Select(c => c.Room).Distinct().Count() == 2, "Stage 1 must offer two distinct bound rooms.");
                    Require(doors.Values.All(c => c.ProgressIndex == 2), "Wrong next progress.");
                    Npc oldDoor = doors.Keys.First();
                    var captured = doors.Values.Select(c => c.Room).ToArray();
                    foreach (Npc door in doors.Keys.ToArray()) { door.showName(); door.SetNameVisible(false); }
                    Require(captured.SequenceEqual(doors.Values.Select(c => c.Room)), "Door interaction rerolled choices.");
                    Enter(doors, 2);
                    Require(!controller.TryEnterPortal(oldDoor), "Duplicate transition must be rejected.");
                    break;
                case 2:
                    CheckArrival(controller);
                    SessionState.SetInt(Phase, 3);
                    controller.NextLevel(controller.CurrentLevelIndex); break;
                case 3:
                    CheckArrival(controller);
                    Clear(controller);
                    Require(doors.Count == 1 && doors.Values.First().Room.kind == StageRoomKind.Merchant, "Stage 2 must lead to merchant.");
                    Enter(doors, 4); break;
                case 4:
                    CheckArrival(controller);
                    CheckMerchant(controller, doors, 2);
                    SessionState.SetInt(Phase, 5); controller.NextLevel(6); break;
                case 5:
                    Require(controller.CurrentLevelIndex == 6, "Stage 6 failed to load.");
                    CheckMerchant(controller, doors, 2);
                    SessionState.SetInt(Phase, 6); controller.NextLevel(9); break;
                case 6:
                    Require(controller.CurrentLevelIndex == 9, "Stage 9 failed to load.");
                    CheckMerchant(controller, doors, 1);
                    Enter(doors, 7); break;
                case 7:
                    CheckArrival(controller);
                    Require(controller.CurrentRoom.kind == StageRoomKind.Boss && doors.Count == 0, "Boss must start locked.");
                    Clear(controller);
                    Require(UnityEngine.Object.FindObjectsByType<Npc>(FindObjectsSortMode.None).Any(n => n.npcType == NpcType.Passivetree), "Boss tree reward missing.");
                    Require(UnityEngine.Object.FindObjectsByType<Npc>(FindObjectsSortMode.None).Any(n => n.npcType == NpcType.Passivepoint && n.passivepoint == 2), "Boss point reward missing.");
                    SessionState.SetInt(Phase, 8); controller.NextLevel(13); break;
                case 8:
                    Require(controller.CurrentLevelIndex == 13, "Final stage failed to load.");
                    Clear(controller);
                    Require(doors.Count == 1 && doors.Values.First().ProgressIndex == 0, "Final stage must return home.");
                    Enter(doors, 9); break;
                case 9:
                    CheckArrival(controller);
                    Require(controller.CurrentLevelIndex == 0 && doors.Count == 1, "Home return did not reset choices.");
                    SessionState.SetInt(Phase, 10); controller.ResetRun(); controller.NextLevel(0); break;
                case 10:
                    Require(controller.CurrentLevelIndex == 0 && doors.Count == 1, "New run home failed.");
                    SessionState.SetBool(Running, false);
                    EditorApplication.update -= Tick;
                    Debug.Log("STAGES PLAY VALIDATED: home, bound choices, combat gate, double-entry guard, restart, merchants 3/6/9, boss rewards, final return, new run.");
                    EditorApplication.Exit(0); break;
            }
        }
        catch (Exception error) { Fail(error); }
    }

    private static void Enter(Dictionary<Npc, StagePortalChoice> doors, int nextPhase)
    {
        var entry = doors.First();
        SessionState.SetString("ProjectD.StagePlayValidation.ExpectedRoom", entry.Value.Room.name);
        SessionState.SetInt("ProjectD.StagePlayValidation.ExpectedProgress", entry.Value.ProgressIndex);
        SessionState.SetInt(Phase, nextPhase);
        Require(LevelCtrl.Instance.TryEnterPortal(entry.Key), "Offered door was rejected.");
    }

    private static void CheckArrival(LevelCtrl controller)
    {
        Require(controller.CurrentRoom.name == SessionState.GetString("ProjectD.StagePlayValidation.ExpectedRoom", ""), "Selected room rerolled during load/restart.");
        Require(controller.CurrentLevelIndex == SessionState.GetInt("ProjectD.StagePlayValidation.ExpectedProgress", -9), "Progress mismatch.");
        Require(controller.nowPrize == controller.CurrentRoom.reward, "Scene/reward binding lost.");
    }

    private static void Clear(LevelCtrl controller)
    {
        var enemies = ((HashSet<Enemy>)typeof(LevelCtrl).GetField("requiredEnemies", PrivateInstance).GetValue(controller)).ToArray();
        foreach (Enemy enemy in enemies) controller.enemycheck(enemy);
        Require(controller.leftEnemy == 0, "Clear did not settle stage.");
        if (enemies.Length > 0) controller.enemycheck(enemies[0]);
    }

    private static void CheckMerchant(LevelCtrl controller, Dictionary<Npc, StagePortalChoice> doors, int count)
    {
        Require(controller.CurrentRoom.kind == StageRoomKind.Merchant && controller.leftEnemy == 0 && doors.Count == count, "Merchant exit policy failed.");
        Npc[] merchants = UnityEngine.Object.FindObjectsByType<Npc>(FindObjectsSortMode.None)
            .Where(n => n.npcType == NpcType.Skillstore || n.npcType == NpcType.Weaponstore).ToArray();
        Require(merchants.Length == 2 && merchants.All(n => n.item.Count > 0), "Preplaced merchant inventory missing.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Fail(Exception error)
    {
        SessionState.SetBool(Running, false);
        EditorApplication.update -= Tick;
        Debug.LogException(error);
        EditorApplication.Exit(1);
    }
}
