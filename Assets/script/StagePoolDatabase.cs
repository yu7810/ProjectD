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

    public StagePool Find(int index) => pools.Find(pool => pool != null && pool.progressIndex == index);
}
