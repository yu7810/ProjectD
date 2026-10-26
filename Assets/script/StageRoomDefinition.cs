using System;
using System.Collections.Generic;
using UnityEngine;

public enum StageRoomKind
{
    [InspectorName("金幣")] Money = 0,
    [InspectorName("天賦")] Talent = 1,
    [InspectorName("天賦樹")] TalentTree = 2,
    [InspectorName("生命")] Life = 3,
    [InspectorName("BOSS")] Boss = 4,
    [InspectorName("據點")] Home = 5,
    [InspectorName("商店")] Store = 6,
    [InspectorName("技能")] Skill = 7
}
public enum StageCompletionRule { ClearEnemies, OpenOnEntry }
public enum StageRewardType { Money, PassivePoints, PassiveTree, Life, RandomSkill, SkillStore, WeaponStore }

[Serializable]
public sealed class StageRewardEntry
{
    public StageRewardType type = StageRewardType.PassivePoints;
    [Min(0)] public int amount = 2;
    public bool useChapterMoney;
    public bool allowPassiveTreeReroll = true;
    [Min(0f)] public float lifeMaxIncrease = 3f;
    [Min(0f)] public float lifeRecovery = 3f;
    public SkillRewardPool skillPool;
    [Range(1, 4)] public int skillLevel = 1;

    public bool IsEmpty => (type == StageRewardType.Money && !useChapterMoney || type == StageRewardType.PassivePoints) && amount == 0
        || type == StageRewardType.Life && lifeMaxIncrease == 0f && lifeRecovery == 0f;

    public bool Validate(out string error)
    {
        error = null;
        if (!Enum.IsDefined(typeof(StageRewardType), type)) error = "未知的獎勵類型。";
        else if ((type == StageRewardType.Money || type == StageRewardType.PassivePoints) && amount < 0)
            error = "獎勵數量不可小於 0。";
        else if (type == StageRewardType.Life && (float.IsNaN(lifeMaxIncrease) || float.IsInfinity(lifeMaxIncrease)
            || float.IsNaN(lifeRecovery) || float.IsInfinity(lifeRecovery) || lifeMaxIncrease < 0 || lifeRecovery < 0))
            error = "生命獎勵數值必須是有效的非負數。";
        else if (type == StageRewardType.RandomSkill)
        {
            if (skillPool == null) error = "隨機技能獎勵需要指定獎勵池。";
            else skillPool.ValidateLevel(skillLevel, out error);
        }
        return error == null;
    }
}

// Legacy standalone data retained for reference. Active doors are embedded in StagePools.
public sealed class StageRoomDefinition : ScriptableObject
{
    public StageRoomKind kind;
    public List<StageRewardEntry> rewards = new List<StageRewardEntry>();
    public GameObject EntrancePrefab => StageEntranceCatalog.Default != null
        ? StageEntranceCatalog.Default.PrefabFor(kind) : null;

    public bool HasRewards => rewards != null && rewards.Count > 0;

    public bool ValidateRewards(out string error)
    {
        error = null;
        if (!HasRewards) return true;
        for (int i = 0; i < rewards.Count; i++)
        {
            if (rewards[i] == null) { error = $"第 {i + 1} 項獎勵為空。"; return false; }
            if (!rewards[i].Validate(out error)) { error = $"第 {i + 1} 項：{error}"; return false; }
        }
        return true;
    }
}
