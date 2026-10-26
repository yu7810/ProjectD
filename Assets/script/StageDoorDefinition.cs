using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class StageDoorDefinition
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
