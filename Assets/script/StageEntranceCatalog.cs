using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectD/Stages/Entrance Catalog")]
public sealed class StageEntranceCatalog : ScriptableObject
{
    public GameObject money;
    public GameObject talent;
    public GameObject talentTree;
    public GameObject life;
    public GameObject boss;
    public GameObject home;
    public GameObject store;
    public GameObject skill;
    private static StageEntranceCatalog cached;
    public static StageEntranceCatalog Default
    {
        get
        {
            if (cached == null) cached = Resources.Load<StageEntranceCatalog>("StageEntrances");
            return cached;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCache() => cached = null;

    public GameObject PrefabFor(StageRoomKind kind)
    {
        switch (kind)
        {
            case StageRoomKind.Money: return money;
            case StageRoomKind.Talent: return talent;
            case StageRoomKind.TalentTree: return talentTree;
            case StageRoomKind.Life: return life;
            case StageRoomKind.Boss: return boss;
            case StageRoomKind.Home: return home;
            case StageRoomKind.Store: return store;
            case StageRoomKind.Skill: return skill;
            default: return null;
        }
    }

    public static string NameKeyFor(StageRoomKind kind)
    {
        switch (kind)
        {
            case StageRoomKind.Money: return "NPC/Exitdoor Money";
            case StageRoomKind.Talent: return "NPC/Exitdoor Passivepoint";
            case StageRoomKind.TalentTree: return "NPC/Item Passivetree Name";
            case StageRoomKind.Life: return "NPC/Item Life Name";
            case StageRoomKind.Boss: return "NPC/Exitdoor Boss";
            case StageRoomKind.Home: return "UI/Home";
            case StageRoomKind.Store: return "NPC/Exitdoor Store";
            case StageRoomKind.Skill: return "NPC/Exitdoor Random Skill";
            default: return null;
        }
    }

    public bool Validate(out string error)
    {
        foreach (StageRoomKind kind in Enum.GetValues(typeof(StageRoomKind)))
        {
            var prefab = PrefabFor(kind);
            if (prefab == null || prefab.GetComponent<Npc>()?.npcType != NpcType.Door)
            { error = "Missing or invalid entrance for " + kind; return false; }
        }
        error = null;
        return true;
    }
}
