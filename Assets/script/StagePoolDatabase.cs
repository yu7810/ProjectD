using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class StageRoomWeight
{
    public StageRoomDefinition room;
    [Min(0)] public float weight = 1;
}

[Serializable]
public sealed class StagePool
{
    [Min(1)] public int progressIndex;
    [Tooltip("0 returns to the home scene.")]
    [Min(0)] public int nextProgressIndex;
    public List<StageRoomWeight> rooms = new List<StageRoomWeight>();

    public List<StageRoomDefinition> Draw(int count, Func<double> random)
    {
        var entries = WeightedStageSelection.Draw(rooms, count,
            entry => entry != null && entry.room != null ? entry.weight : 0,
            entry => entry.room != null ? entry.room.GetInstanceID().ToString() : null, random);
        return entries.ConvertAll(entry => entry.room);
    }

    public bool Contains(StageRoomDefinition room) => rooms.Exists(entry =>
        entry != null && entry.room == room && entry.weight > 0);
}

[CreateAssetMenu(menuName = "ProjectD/Stages/Pool Database")]
public sealed class StagePoolDatabase : ScriptableObject
{
    public StageRoomDefinition home;
    [Min(1)] public int firstProgressIndex = 1;
    public List<StagePool> pools = new List<StagePool>();
    public List<ChapterMoneyReward> chapterMoneyRewards = new List<ChapterMoneyReward>();

    public StagePool Find(int index) => pools.Find(pool => pool != null && pool.progressIndex == index);

    public static int ChapterForProgress(int progress) => progress > 0 ? (progress - 1) / 10 + 1 : 0;

    public bool TryGetChapterMoneyReward(int chapter, out int amount)
    {
        amount = 0;
        if (chapter <= 0 || chapterMoneyRewards == null) return false;
        ChapterMoneyReward match = null;
        foreach (var entry in chapterMoneyRewards)
        {
            if (entry == null || entry.chapter != chapter) continue;
            if (match != null || entry.money < 0) return false;
            match = entry;
        }
        if (match == null) return false;
        amount = match.money;
        return true;
    }

    public List<string> ChapterMoneyRewardErrors()
    {
        var errors = new List<string>();
        var chapters = new HashSet<int>();
        if (chapterMoneyRewards == null) errors.Add("缺少章節金幣獎勵清單。");
        else foreach (var entry in chapterMoneyRewards)
        {
            if (entry == null) { errors.Add("章節金幣獎勵有空白項目。"); continue; }
            if (entry.chapter < 1) errors.Add("章節編號必須大於 0。");
            if (!chapters.Add(entry.chapter)) errors.Add($"第 {entry.chapter} 章的金幣獎勵重複設定。");
            if (entry.money < 0) errors.Add($"第 {entry.chapter} 章的金幣數量不可小於 0。");
        }
        var checkedChapters = new HashSet<int>();
        if (pools != null) foreach (var pool in pools)
        {
            if (pool == null || pool.progressIndex <= 0) continue;
            int chapter = ChapterForProgress(pool.progressIndex);
            if (checkedChapters.Add(chapter) && !chapters.Contains(chapter))
                errors.Add($"缺少第 {chapter} 章的金幣獎勵設定。");
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
