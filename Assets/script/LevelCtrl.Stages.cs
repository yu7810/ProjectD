using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class LevelCtrl
{
    [SerializeField] private StagePoolDatabase stagePools;
    public StagePoolDatabase StagePools
    {
        get { EnsureStagePools(); return stagePools; }
    }
    public StageRoomDefinition CurrentRoom { get; private set; }
    private readonly Dictionary<Npc, StagePortalChoice> offeredDoors = new Dictionary<Npc, StagePortalChoice>();
    private readonly List<DoorAnchor> doorAnchors = new List<DoorAnchor>();
    private Transform generatedDoors;

    private struct DoorAnchor
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    private IEnumerator Start()
    {
        // Let the home scene's player, UI and data finish their Start methods.
        yield return null;
        if (isLoadingLevel) yield break;
        EnsureStagePools();
        if (stagePools == null) yield break;
        string scene = SceneManager.GetActiveScene().name;
        if (stagePools.home != null && stagePools.home.sceneName == scene)
        {
            SetProgress(0);
            CurrentRoom = stagePools.home;
        }
        else
        {
            foreach (StagePool pool in stagePools.pools)
            {
                foreach (StageRoomWeight entry in pool.rooms)
                {
                    if (entry?.room == null || entry.room.sceneName != scene) continue;
                    SetProgress(pool.progressIndex);
                    CurrentRoom = entry.room;
                    break;
                }
                if (CurrentRoom != null) break;
            }
        }
        if (CurrentRoom != null)
        {
            nowPrize = CurrentRoom.reward;
            PrepareRoom();
        }
    }

    private void EnsureStagePools()
    {
        if (stagePools == null) stagePools = Resources.Load<StagePoolDatabase>("StagePools");
        if (stagePools == null) Debug.LogError("Missing Resources/StagePools database.", this);
    }

    private void SetProgress(int level)
    {
        CurrentLevelIndex = level;
        nowclass = level > 0 ? level % 10 : 0;
        nowLevel = level > 0 ? (level - 1) / 10 + 1 : 0;
    }

    private bool TryLoadStage(int level, StageRoomDefinition selectedRoom, bool portalRecovery)
    {
        if (isLoadingLevel) return false;
        EnsureStagePools();
        if (stagePools == null) return false;
        StageRoomDefinition room = selectedRoom;
        if (level == 0) room = stagePools.home;
        else if (level > 0)
        {
            StagePool pool = stagePools.Find(level);
            if (pool == null) { Debug.LogError($"Missing stage pool {level}.", this); return false; }
            // Restart keeps the selected scene/reward pair; debug stage selection rolls once.
            if (room == null && CurrentLevelIndex == level && pool.Contains(CurrentRoom)) room = CurrentRoom;
            if (room == null)
            {
                List<StageRoomDefinition> selection = pool.Draw(1, () => Random.value);
                if (selection.Count > 0) room = selection[0];
            }
            if (room == null || !pool.Contains(room))
            { Debug.LogError($"No valid room in stage pool {level}.", this); return false; }
        }
        else if (level != -1) return false;

        string scene = level == -1 ? "Title" : room != null ? room.sceneName : null;
        if (string.IsNullOrEmpty(scene) || !Application.CanStreamedLevelBeLoaded(scene))
        { Debug.LogError($"Stage scene is not enabled in Build Settings: {scene}", this); return false; }
        isLoadingLevel = true;
        if (portalRecovery) ValueData.Instance.ApplyPortalRecovery();
        if (Skill.Instance != null) Skill.Instance.StopAllSunfire();
        StartCoroutine(LoadSceneWithProgress(level, room));
        return true;
    }

    public bool TryEnterPortal(Npc portal)
    {
        if (portal == null || !portal.gameObject.activeInHierarchy ||
            !offeredDoors.TryGetValue(portal, out StagePortalChoice choice)) return false;
        return TryLoadStage(choice.ProgressIndex, choice.Room, true);
    }

    private void PrepareRoom()
    {
        offeredDoors.Clear();
        doorAnchors.Clear();
        generatedDoors = null;
        // Use the existing authored door positions, including inactive combat exits.
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Npc door in root.GetComponentsInChildren<Npc>(true))
            {
                if (door.npcType != NpcType.Door) continue;
                doorAnchors.Add(new DoorAnchor { position = door.transform.position,
                    rotation = door.transform.rotation, scale = door.transform.lossyScale });
                door.gameObject.SetActive(false);
            }
        }
        Enemys = GameObject.Find("Enemys");
        InitializeStageEnemies();
        if (CurrentRoom.completion == StageCompletionRule.OpenOnEntry)
        {
            // Merchant/home/event rooms have their own contents. No combat reward settlement.
            stageCompleted = true;
            requiredEnemies.Clear();
            leftEnemy = 0;
            CreateStageExits();
        }
        else if (leftEnemy == 0) CompleteStage();
    }

    private void CompleteStage()
    {
        if (stageCompleted) return;
        stageCompleted = true;
        getPrize();
        CreateStageExits();
    }

    private void CreateStageExits()
    {
        if (CurrentRoom == null || generatedDoors != null || CurrentRoom.exitCount <= 0) return;
        int next = CurrentLevelIndex == 0 ? stagePools.firstProgressIndex :
            stagePools.Find(CurrentLevelIndex)?.nextProgressIndex ?? 0;
        var choices = new List<StageRoomDefinition>();
        if (next == 0)
        {
            if (stagePools.home != null) choices.Add(stagePools.home);
        }
        else
        {
            StagePool pool = stagePools.Find(next);
            if (pool != null) choices = pool.Draw(CurrentRoom.exitCount, () => Random.value);
        }
        if (choices.Count == 0)
        { Debug.LogError($"No exit candidates for stage {CurrentLevelIndex}, next pool {next}.", this); return; }
        generatedDoors = new GameObject("Stage Exits (Runtime)").transform;
        for (int i = 0; i < choices.Count; i++)
        {
            StageRoomDefinition target = choices[i];
            if (target.entrancePrefab == null)
            { Debug.LogError($"Room {target.name} has no entrance prefab.", target); continue; }
            DoorAnchor anchor;
            if (i < doorAnchors.Count) anchor = doorAnchors[i];
            else
            {
                anchor = doorAnchors.Count > 0 ? doorAnchors[0] :
                    new DoorAnchor { position = new Vector3(0, 0.3f, 8), rotation = Quaternion.identity, scale = Vector3.one };
                anchor.position += anchor.rotation * Vector3.right * (4f * (i - doorAnchors.Count + 1));
            }
            GameObject instance = Instantiate(target.entrancePrefab, anchor.position, anchor.rotation, generatedDoors);
            instance.transform.localScale = anchor.scale;
            Npc door = instance.GetComponent<Npc>();
            if (door == null) { Destroy(instance); Debug.LogError($"Room {target.name} entrance has no Npc.", target); continue; }
            door.Level = next;
            door.Prize = target.reward;
            if (!string.IsNullOrEmpty(target.entranceNameKey)) door.Name = target.entranceNameKey;
            door.SetNameVisible(false);
            offeredDoors.Add(door, new StagePortalChoice(next, target));
            instance.SetActive(true);
        }
    }
}
