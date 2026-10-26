using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public partial class LevelCtrl
{
    private GameObject RewardPrefab(StageRewardEntry reward)
    {
        switch (reward.type)
        {
            case StageRewardType.Money: return itemMoney;
            case StageRewardType.PassivePoints: return itemPassivepoint;
            case StageRewardType.PassiveTree: return itemPassiveTree;
            case StageRewardType.Life: return itemLife;
            case StageRewardType.RandomSkill: return itemSkill;
            case StageRewardType.SkillStore: return skillstorePrefab;
            case StageRewardType.WeaponStore: return weaponstorePrefab;
            default: return null;
        }
    }

    private bool ValidateCompletionRewards(StageRoomSelection room, out string error, int progressIndex = -1)
    {
        error = null;
        if (room == null || !room.GrantsCompletionRewards || !room.HasRewards) return true;
        if (!room.ValidateRewards(out error)) return false;
        foreach (var reward in room.rewards)
        {
            if (reward.IsEmpty) continue;
            int chapter = progressIndex >= 0 ? StagePoolDatabase.ChapterForProgress(progressIndex) : nowLevel;
            if (reward.type == StageRewardType.Money && reward.useChapterMoney &&
                !StagePools.TryGetChapterMoneyReward(chapter, out _))
            { error = "Missing chapter money reward for chapter " + chapter; return false; }
            var prefab = RewardPrefab(reward);
            if (prefab == null || prefab.GetComponent<Npc>() == null)
            { error = "Missing NPC reward prefab for " + reward.type; return false; }
        }
        return true;
    }

    private void DropConfiguredRewards(Vector3 origin)
    {
        if (!ValidateCompletionRewards(CurrentRoom, out string error))
        { Debug.LogError($"Invalid completion rewards for {CurrentRoom.name}: {error}", CurrentRoom.Context); return; }

        // Resolve every draw before spawning, so invalid live edits cannot give a partial payout.
        var entries = new List<StageRewardEntry>();
        var skills = new List<int>();
        foreach (var reward in CurrentRoom.rewards)
        {
            if (reward.IsEmpty) continue;
            if (reward.type == StageRewardType.Money && reward.useChapterMoney &&
                StagePools.TryGetChapterMoneyReward(nowLevel, out int chapterMoney) && chapterMoney == 0)
                continue;
            int skillId = 0;
            if (reward.type == StageRewardType.RandomSkill &&
                !reward.skillPool.TryDraw(reward.skillLevel, () => Random.value, out skillId))
            { Debug.LogError("Skill reward pool became invalid before settlement.", CurrentRoom.Context); return; }
            entries.Add(reward);
            skills.Add(skillId);
        }
        var positions = new List<Vector3>();
        for (int i = 0; i < entries.Count; i++)
        {
            var reward = entries[i];
            GameObject prefab = RewardPrefab(reward);
            Vector3 position = RewardPosition(origin, i, entries.Count, positions);
            positions.Add(position);
            if (reward.type == StageRewardType.SkillStore || reward.type == StageRewardType.WeaponStore)
                position.y = prefab.transform.position.y;
            Npc npc = Instantiate(prefab, position, prefab.transform.rotation).GetComponent<Npc>();
            switch (reward.type)
            {
                case StageRewardType.Money:
                    npc.npcType = NpcType.Money;
                    if (reward.useChapterMoney) StagePools.TryGetChapterMoneyReward(nowLevel, out npc.money);
                    else npc.money = reward.amount;
                    break;
                case StageRewardType.PassivePoints:
                    npc.npcType = NpcType.Passivepoint; npc.passivepoint = reward.amount; break;
                case StageRewardType.PassiveTree:
                    npc.npcType = NpcType.Passivetree;
                    npc.allowPassiveTreeReroll = reward.allowPassiveTreeReroll; break;
                case StageRewardType.Life:
                    npc.npcType = NpcType.Life;
                    npc.lifeMaxIncrease = reward.lifeMaxIncrease; npc.lifeRecovery = reward.lifeRecovery; break;
                case StageRewardType.RandomSkill:
                    npc.npcType = NpcType.Skill;
                    npc.item = new List<int> { skills[i] };
                    npc.itemlevel = new List<int> { reward.skillLevel }; break;
                case StageRewardType.SkillStore:
                case StageRewardType.WeaponStore:
                    npc.npcType = reward.type == StageRewardType.SkillStore ? NpcType.Skillstore : NpcType.Weaponstore;
                    npc.startRandom = true;
                    npc.RandomItem();
                    break;
            }
            npc.showName();
        }
    }

    private Vector3 RewardPosition(Vector3 origin, int index, int count, List<Vector3> previous)
    {
        Vector3 offset = count == 1 ? Vector3.zero : count == 2 ? Vector3.right * (index == 0 ? -1.25f : 1.25f)
            : Quaternion.Euler(0, index * 360f / count, 0) * Vector3.forward * Mathf.Max(2f, count * 0.4f);
        Vector3 candidate = origin + offset;
        for (int attempt = 0; attempt < 32; attempt++)
        {
            if (attempt > 0)
                candidate = origin + Quaternion.Euler(0, index * 137.5f + attempt * 67f, 0)
                    * Vector3.forward * (1.5f + attempt / 8 * 1.25f);
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 0.6f, NavMesh.AllAreas)) continue;
            candidate = new Vector3(hit.position.x, origin.y, hit.position.z);
            bool overlaps = previous.Exists(p => (p - candidate).sqrMagnitude < 1.5625f);
            if (!overlaps && !Physics.CheckSphere(candidate + Vector3.up * 0.5f, 0.4f, 1 << 8, QueryTriggerInteraction.Ignore))
                return candidate;
        }
        return origin + offset;
    }
}
