using System.Collections.Generic;
using UnityEngine;

// A fixed result of two independent rolls. Never serialized as an authoring asset.
public sealed class StageRoomSelection
{
    public readonly int ProgressIndex;
    public readonly StageSceneDefinition Scene;
    public readonly StageDoorDefinition Door;
    public readonly string sceneName;
    public readonly StageRoomKind kind;
    public readonly StageCompletionRule completion;
    public readonly int exitCount;
    public string name => kind + " / " + Scene.name;
    public Object Context => Scene;
    public List<StageRewardEntry> rewards => Door.rewards;
    public bool HasRewards => Door.HasRewards;
    public bool GrantsCompletionRewards => completion == StageCompletionRule.ClearEnemies && kind != StageRoomKind.Home;
    public GameObject EntrancePrefab => Door.EntrancePrefab;

    public StageRoomSelection(int progress, StageSceneDefinition scene, StageDoorDefinition door, int exits)
    {
        ProgressIndex = progress; Scene = scene; Door = door;
        sceneName = scene.sceneName; kind = door.kind; completion = scene.completion; exitCount = exits;
    }

    public bool ValidateRewards(out string error) => Door.ValidateRewards(out error);
}
