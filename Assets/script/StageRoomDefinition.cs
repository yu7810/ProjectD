using UnityEngine;

public enum StageRoomKind { Combat, Merchant, Boss, Event, Home }
public enum StageCompletionRule { ClearEnemies, OpenOnEntry }

// A room is an indivisible scene + reward + exit policy. Pools select this asset,
// never a scene and a reward independently.
[CreateAssetMenu(menuName = "ProjectD/Stages/Room")]
public sealed class StageRoomDefinition : ScriptableObject
{
    public string sceneName;
    public StageRoomKind kind;
    public StageCompletionRule completion;
    public PrizeBase reward;
    [Min(0)] public int exitCount = 2;
    public GameObject entrancePrefab;
    [Tooltip("Existing I2 term. Empty uses the entrance prefab's localized name.")]
    public string entranceNameKey;
}
