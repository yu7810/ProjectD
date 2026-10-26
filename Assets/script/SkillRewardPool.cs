using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class SkillRewardEntry
{
    public int skillId = 1;
    [Min(0f)] public float weight = 1f;
}

[CreateAssetMenu(menuName = "ProjectD/Rewards/Skill Reward Pool")]
public sealed class SkillRewardPool : ScriptableObject
{
    public List<SkillRewardEntry> level1 = new List<SkillRewardEntry>();
    public List<SkillRewardEntry> level2 = new List<SkillRewardEntry>();
    public List<SkillRewardEntry> level3 = new List<SkillRewardEntry>();
    public List<SkillRewardEntry> level4 = new List<SkillRewardEntry>();

    public List<SkillRewardEntry> Entries(int level)
    {
        switch (level)
        {
            case 1: return level1;
            case 2: return level2;
            case 3: return level3;
            case 4: return level4;
            default: return null;
        }
    }

    public bool ValidateLevel(int level, out string error)
    {
        error = null;
        if (level < 1 || level > 4 || level > SkillLevelTable.MaxLevel)
            error = "技能獎勵等級尚未支援：" + level;
        else
        {
            var entries = Entries(level);
            var seen = new HashSet<int>();
            double total = 0;
            if (entries == null || entries.Count == 0)
                error = "技能獎勵池沒有項目。";
            else foreach (var entry in entries)
            {
                if (entry == null || Array.IndexOf(SkillLevelTable.ActiveIds, entry.skillId) < 0)
                { error = "技能獎勵池包含未使用或無效的技能 ID。"; break; }
                if (!seen.Add(entry.skillId))
                { error = "同等級的技能獎勵池包含重複 ID：" + entry.skillId; break; }
                if (float.IsNaN(entry.weight) || float.IsInfinity(entry.weight) || entry.weight < 0f)
                { error = "技能獎勵權重必須是有效的非負數。"; break; }
                total += entry.weight;
            }
            if (error == null && total <= 0)
                error = "技能獎勵池至少需要一個大於 0 的權重。";
        }
        return error == null;
    }

    public bool TryDraw(int level, Func<double> random, out int skillId)
    {
        skillId = 0;
        if (!ValidateLevel(level, out _) || random == null)
            return false;
        var selected = WeightedStageSelection.Draw(Entries(level), 1,
            entry => entry.weight, entry => entry.skillId.ToString(), random);
        if (selected.Count != 1) return false;
        skillId = selected[0].skillId;
        return true;
    }
}
