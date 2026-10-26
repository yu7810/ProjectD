using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class StageDoorWeight
{
    public StageDoorDefinition door = new StageDoorDefinition();
    [Min(0)] public float weight = 1;
}

[Serializable]
public sealed class StageSceneWeight
{
    public StageSceneDefinition scene;
    [Min(0)] public float weight = 1;
}

[Serializable]
public sealed class StagePool
{
    [Min(1)] public int progressIndex;
    [Min(0)] public int nextProgressIndex;
    [Min(0)] public int exitCount = 2;
    public List<StageDoorWeight> exits = new List<StageDoorWeight>();
    public List<StageSceneWeight> combatScenes = new List<StageSceneWeight>();
    public List<StageSceneWeight> bossScenes = new List<StageSceneWeight>();
    public List<StageSceneWeight> storeScenes = new List<StageSceneWeight>();

    public List<StageSceneWeight> Scenes(StageSceneCategory category)
    {
        if (category == StageSceneCategory.Combat) return combatScenes;
        if (category == StageSceneCategory.Boss) return bossScenes;
        if (category == StageSceneCategory.Store) return storeScenes;
        return null;
    }
}

[CreateAssetMenu(menuName = "ProjectD/Stages/Pool Database")]
public sealed class StagePoolDatabase : ScriptableObject
{
    public StageDoorDefinition home = new StageDoorDefinition { kind = StageRoomKind.Home };
    [Min(1)] public int firstProgressIndex = 1;
    [Min(0)] public int homeExitCount = 1;
    public List<StageDoorWeight> homeExits = new List<StageDoorWeight>();
    public List<StageSceneWeight> homeScenes = new List<StageSceneWeight>();
    public List<StagePool> pools = new List<StagePool>();
    public List<ChapterMoneyReward> chapterMoneyRewards = new List<ChapterMoneyReward>();
    public StagePool Find(int index) => pools.Find(pool => pool != null && pool.progressIndex == index);
    public static int ChapterForProgress(int progress) => progress > 0 ? (progress - 1) / 10 + 1 : 0;
    public List<StageDoorWeight> ExitsAfter(int progress) => progress == 0 ? homeExits : Find(progress)?.exits;
    public int ExitCountAfter(int progress) => progress == 0 ? homeExitCount : Find(progress)?.exitCount ?? 0;
    public int NextAfter(int progress) => progress == 0 ? firstProgressIndex : Find(progress)?.nextProgressIndex ?? 0;
    public List<StageSceneWeight> ScenesFor(int progress, StageRoomKind kind) => kind == StageRoomKind.Home
        ? homeScenes : Find(progress)?.Scenes(StageSceneDefinition.CategoryFor(kind));
    private static bool Weight(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0;

    private static StageSceneDefinition DrawScene(List<StageSceneWeight> scenes, Func<double> random)
    {
        var draw = WeightedStageSelection.Draw(scenes, 1,
            e => e != null && e.scene != null ? e.weight : 0,
            e => e.scene != null ? e.scene.sceneName : null, random);
        return draw.Count > 0 ? draw[0].scene : null;
    }

    public bool TryDrawChoices(int sourceProgress, Func<double> random, out List<StagePortalChoice> choices,
        out string error, int countOverride = -1)
    {
        choices = new List<StagePortalChoice>(); error = null;
        int count = countOverride >= 0 ? countOverride : ExitCountAfter(sourceProgress);
        if (count <= 0) return true;
        if (!ValidateExits(sourceProgress, out error)) return false;
        var draw = WeightedStageSelection.Draw(ExitsAfter(sourceProgress), count,
            e => e != null && e.door != null ? e.weight : 0,
            e => e.door != null ? ((int)e.door.kind).ToString() : null, random);
        foreach (var entry in draw)
        {
            int next = entry.door.kind == StageRoomKind.Home ? 0 : NextAfter(sourceProgress);
            var scene = DrawScene(ScenesFor(next, entry.door.kind), random);
            if (scene == null) { error = "No scene for " + entry.door.kind; choices.Clear(); return false; }
            choices.Add(new StagePortalChoice(next,
                new StageRoomSelection(next, scene, entry.door, ExitCountAfter(next))));
        }
        return true;
    }

    public bool TryDrawArrival(int progress, Func<double> random, out StageRoomSelection selection, out string error)
    {
        selection = null; error = null;
        if (progress == 0)
        {
            var scene = DrawScene(homeScenes, random);
            if (scene == null || home == null) { error = "Missing home scene/reward."; return false; }
            selection = new StageRoomSelection(0, scene, home, homeExitCount); return true;
        }
        int source = firstProgressIndex == progress ? 0 : -1;
        if (source < 0) foreach (var pool in pools)
            if (pool != null && pool.nextProgressIndex == progress) { source = pool.progressIndex; break; }
        if (source < 0) { error = "No incoming door pool for progress " + progress; return false; }
        if (!ValidateExits(source, out error)) return false;
        var incoming = ExitsAfter(source)?.FindAll(e => e?.door != null && e.door.kind != StageRoomKind.Home);
        var draw = WeightedStageSelection.Draw(incoming, 1, e => e.weight, e => ((int)e.door.kind).ToString(), random);
        if (draw.Count == 0) { error = "No incoming non-home door for progress " + progress; return false; }
        var chosen = draw[0].door;
        var target = DrawScene(ScenesFor(progress, chosen.kind), random);
        if (target == null) { error = "No scene for progress " + progress; return false; }
        selection = new StageRoomSelection(progress, target, chosen, ExitCountAfter(progress));
        return true;
    }

    public bool TryResolveLoadedScene(string sceneName, out StageRoomSelection selection)
    {
        selection = null;
        foreach (var entry in homeScenes)
            if (entry?.scene != null && entry.weight > 0 && entry.scene.sceneName == sceneName)
            { selection = new StageRoomSelection(0, entry.scene, home, homeExitCount); return true; }
        foreach (var pool in pools)
        {
            if (pool == null) continue;
            foreach (var category in new[] { StageSceneCategory.Combat, StageSceneCategory.Boss, StageSceneCategory.Store })
            {
                var scenes = pool.Scenes(category);
                if (scenes == null) continue;
                foreach (var entry in scenes)
                {
                    if (entry?.scene == null || entry.weight <= 0 || entry.scene.sceneName != sceneName) continue;
                    var incoming = pool.progressIndex == firstProgressIndex ? homeExits
                        : pools.Find(p => p != null && p.nextProgressIndex == pool.progressIndex)?.exits;
                    var door = incoming?.Find(e => e?.door != null && e.weight > 0
                        && StageSceneDefinition.CategoryFor(e.door.kind) == category)?.door;
                    if (door == null) continue;
                    selection = new StageRoomSelection(pool.progressIndex, entry.scene, door, pool.exitCount); return true;
                }
            }
        }
        return false;
    }

    private static bool ValidateScenes(List<StageSceneWeight> entries, StageSceneCategory category, bool requireAny, out string error)
    {
        error = null; bool any = false; var seen = new HashSet<string>();
        if (entries != null) foreach (var entry in entries)
        {
            if (entry == null || !Weight(entry.weight)) { error = "場景權重必須是有效的非負數。"; return false; }
            if (entry.weight == 0) continue;
            if (entry.scene == null) { error = "場景池有未指定場景的項目。"; return false; }
            if (!entry.scene.Validate(out error)) return false;
            if (entry.scene.category != category) { error = "場景分類與池不符：" + entry.scene.name; return false; }
            if (!seen.Add(entry.scene.sceneName)) { error = "場景池重複：" + entry.scene.sceneName; return false; }
            any = true;
        }
        if (requireAny && !any) error = "沒有可抽選的「" + category + "」場景。";
        return error == null;
    }

    public bool ValidateExits(int sourceProgress, out string error)
    {
        error = null; var entries = ExitsAfter(sourceProgress); var seen = new HashSet<StageRoomKind>(); bool any = false;
        if (entries != null) foreach (var entry in entries)
        {
            if (entry == null || !Weight(entry.weight)) { error = "門權重必須是有效的非負數。"; return false; }
            if (entry.weight == 0) continue;
            if (entry.door == null) { error = "門類型池有未指定的獎勵資料。"; return false; }
            if (!Enum.IsDefined(typeof(StageRoomKind), entry.door.kind)) { error = "未知門分類。"; return false; }
            if (!seen.Add(entry.door.kind)) { error = "同一池的門類型重複：" + entry.door.kind; return false; }
            if (!entry.door.ValidateRewards(out error)) return false;
            int next = entry.door.kind == StageRoomKind.Home ? 0 : NextAfter(sourceProgress);
            if (entry.door.kind != StageRoomKind.Home && (next <= 0 || Find(next) == null))
            { error = "找不到下一關進度：" + next; return false; }
            if (!ValidateScenes(ScenesFor(next, entry.door.kind), StageSceneDefinition.CategoryFor(entry.door.kind), true, out error))
            { error = "第 " + sourceProgress + " 關結束 → " + entry.door.kind + "：" + error; return false; }
            any = true;
        }
        if (!any && ExitCountAfter(sourceProgress) > 0) error = "已設定門數，但沒有可抽選的門類型。";
        return error == null;
    }

    public List<string> ConfigurationErrors()
    {
        var errors = new List<string>();
        if (home == null || home.kind != StageRoomKind.Home) errors.Add("據點獎勵資料缺少或分類錯誤。");
        if (!ValidateScenes(homeScenes, StageSceneCategory.Home, true, out string error)) errors.Add(error);
        if (Find(firstProgressIndex) == null) errors.Add("找不到第一關進度。");
        if (homeExitCount < 0) errors.Add("據點門數不可為負。");
        if (!ValidateExits(0, out error)) errors.Add(error);
        var seen = new HashSet<int>();
        foreach (var pool in pools)
        {
            if (pool == null || pool.progressIndex < 1 || !seen.Add(pool.progressIndex)) { errors.Add("進度編號無效或重複。"); continue; }
            if (pool.exitCount < 0 || pool.nextProgressIndex < 0) errors.Add("門數與下一關編號不可為負。");
            foreach (var category in new[] { StageSceneCategory.Combat, StageSceneCategory.Boss, StageSceneCategory.Store })
                if (!ValidateScenes(pool.Scenes(category), category, false, out error)) errors.Add("第 " + pool.progressIndex + " 關：" + error);
            if (!ValidateExits(pool.progressIndex, out error)) errors.Add(error);
        }
        return errors;
    }

    public bool TryGetChapterMoneyReward(int chapter, out int amount)
    {
        amount = 0; ChapterMoneyReward match = null;
        if (chapter <= 0 || chapterMoneyRewards == null) return false;
        foreach (var entry in chapterMoneyRewards)
        {
            if (entry == null || entry.chapter != chapter) continue;
            if (match != null || entry.money < 0) return false;
            match = entry;
        }
        if (match == null) return false;
        amount = match.money; return true;
    }

    public List<string> ChapterMoneyRewardErrors()
    {
        var errors = new List<string>(); var chapters = new HashSet<int>();
        if (chapterMoneyRewards == null) errors.Add("缺少章節金幣獎勵清單。");
        else foreach (var entry in chapterMoneyRewards)
        {
            if (entry == null || entry.chapter < 1 || entry.money < 0 || !chapters.Add(entry.chapter))
                errors.Add("章節金幣資料無效、重複或為負值。");
        }
        var checkedChapters = new HashSet<int>();
        var sources = new List<int> { 0 }; foreach (var pool in pools) if (pool != null) sources.Add(pool.progressIndex);
        foreach (int source in sources)
        {
            var doors = ExitsAfter(source); if (doors == null) continue;
            foreach (var entry in doors)
            {
                if (entry?.door == null || entry.weight <= 0 || entry.door.kind == StageRoomKind.Home || entry.door.kind == StageRoomKind.Store) continue;
                bool needs = entry.door.rewards != null && entry.door.rewards.Exists(r => r != null && r.type == StageRewardType.Money && r.useChapterMoney);
                int chapter = ChapterForProgress(NextAfter(source));
                if (needs && checkedChapters.Add(chapter) && !chapters.Contains(chapter)) errors.Add("缺少第 " + chapter + " 章的金幣獎勵設定。");
            }
        }
        return errors;
    }
}

[Serializable]
public sealed class ChapterMoneyReward
{
    [Min(1)] public int chapter = 1;
    [Min(0)] public int money;
}
