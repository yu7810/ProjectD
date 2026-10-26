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
        Require(StageEntranceCatalog.Default != null, "StageEntrances catalog is required.");
        Require(StageEntranceCatalog.Default.Validate(out string entranceError), entranceError);
        var errors = database.ConfigurationErrors();
        errors.AddRange(database.ChapterMoneyRewardErrors());
        Require(errors.Count == 0, string.Join("\n", errors));
        var scenes = new HashSet<string>(EditorBuildSettings.scenes.Where(s => s.enabled)
            .Select(s => System.IO.Path.GetFileNameWithoutExtension(s.path)));
        var all = new List<StageSceneWeight>(database.homeScenes);
        foreach (var pool in database.pools)
        {
            all.AddRange(pool.combatScenes); all.AddRange(pool.bossScenes); all.AddRange(pool.storeScenes);
            Require(database.TryDrawChoices(pool.progressIndex, () => .5, out var choices, out _), "Invalid exit draw");
            Require(choices.Select(c => c.Room.kind).Distinct().Count() == choices.Count, "Door kinds duplicated");
        }
        foreach (var entry in all)
            if (entry?.scene != null && entry.weight > 0)
                Require(scenes.Contains(entry.scene.sceneName), "Scene not enabled: " + entry.scene.sceneName);
        CheckSelection();
        Debug.Log($"STAGES VALIDATED: {database.pools.Count} independent progress pools; weighted selection checks passed.");
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
