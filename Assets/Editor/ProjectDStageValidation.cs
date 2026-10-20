using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ProjectDStageValidation
{
    [MenuItem("Tools/ProjectD/Stages/Validate Pools")]
    public static void Validate()
    {
        StagePoolDatabase database = Resources.Load<StagePoolDatabase>("StagePools");
        Require(database != null && database.home != null, "StagePools and home room are required.");
        var scenes = new HashSet<string>(EditorBuildSettings.scenes.Where(s => s.enabled)
            .Select(s => System.IO.Path.GetFileNameWithoutExtension(s.path)));
        var indices = new HashSet<int>();
        var rooms = new HashSet<StageRoomDefinition> { database.home };
        foreach (StagePool pool in database.pools)
        {
            Require(pool != null && pool.progressIndex > 0 && indices.Add(pool.progressIndex), "Duplicate/invalid progress index.");
            Require(pool.rooms != null && pool.rooms.Count > 0, $"Empty pool {pool.progressIndex}.");
            var unique = new HashSet<StageRoomDefinition>();
            foreach (StageRoomWeight entry in pool.rooms)
            {
                Require(entry != null && entry.room != null, $"Missing room in pool {pool.progressIndex}.");
                Require(!float.IsNaN(entry.weight) && !float.IsInfinity(entry.weight) && entry.weight >= 0, "Invalid weight.");
                Require(unique.Add(entry.room), $"Duplicate room {entry.room.name} in pool {pool.progressIndex}.");
                rooms.Add(entry.room);
            }
            Require(pool.Draw(int.MaxValue, () => 0.5).Count > 0, $"No selectable rooms in pool {pool.progressIndex}.");
        }
        Require(database.Find(database.firstProgressIndex) != null, "Missing first pool.");
        foreach (StagePool pool in database.pools)
            Require(pool.nextProgressIndex == 0 || database.Find(pool.nextProgressIndex) != null, $"Missing next pool for {pool.progressIndex}.");
        foreach (StageRoomDefinition room in rooms)
        {
            Require(scenes.Contains(room.sceneName), $"Scene not enabled: {room.sceneName}");
            Require(room.exitCount >= 0, $"Negative exit count: {room.name}");
            Require(room.entrancePrefab != null && room.entrancePrefab.GetComponent<Npc>()?.npcType == NpcType.Door,
                $"Invalid entrance prefab: {room.name}");
            if (room.kind == StageRoomKind.Home || room.kind == StageRoomKind.Merchant)
                Require(room.completion == StageCompletionRule.OpenOnEntry && room.reward == PrizeBase.None,
                    $"Home/merchant room must open immediately without a combat reward: {room.name}");
        }
        CheckSelection();
        Debug.Log($"STAGES VALIDATED: {database.pools.Count} pools, {rooms.Count} rooms; weighted selection checks passed.");
    }

    private static void CheckSelection()
    {
        var rng = new System.Random(12345);
        Func<double> random = () => rng.NextDouble();
        var source = new[] { "A", "B", "C", "A", "Disabled" };
        Func<string, double> weight = x => x == "Disabled" ? 0 : x == "A" ? 8 : 1;
        int a = 0;
        for (int i = 0; i < 2000; i++)
        {
            var draw = WeightedStageSelection.Draw(source, 2, weight, x => x, random);
            Require(draw.Count == 2 && draw.Distinct().Count() == 2 && !draw.Contains("Disabled"), "Invalid/non-unique draw.");
            if (draw[0] == "A") a++;
        }
        Require(a > 1400 && a < 1800, "Weights are not reflected in selection.");
        Require(WeightedStageSelection.Draw(source, 99, weight, x => x, () => 1).Count == 3, "Insufficient pool must reduce exit count.");
        Require(WeightedStageSelection.Draw(new[] { "A" }, 2, weight, x => x, () => 0).Count == 1, "Singleton pool failed.");
        Require(WeightedStageSelection.Draw(source, 0, weight, x => x, random).Count == 0, "Zero exit count failed.");
        Require(WeightedStageSelection.Draw(source, 2, x => 0, x => x, random).Count == 0, "Disabled pool failed.");
    }

    public static void RunBatch()
    {
        try { Validate(); EditorApplication.Exit(0); }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
