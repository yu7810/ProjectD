using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class LevelCtrl
{
    [SerializeField] private StagePoolDatabase stagePools;
    public StagePoolDatabase StagePools
    {
        get { EnsureStagePools(); return stagePools; }
    }
    public StageRoomSelection CurrentRoom { get; private set; }
    private readonly Dictionary<Npc, StagePortalChoice> offeredDoors = new Dictionary<Npc, StagePortalChoice>();
    private readonly List<DoorAnchor> doorAnchors = new List<DoorAnchor>();
    private Transform generatedDoors;

    private struct DoorAnchor
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    private IEnumerator Start()
    {
        // Let the home scene's player, UI and data finish their Start methods.
        yield return null;
        if (isLoadingLevel) yield break;
        EnsureStagePools();
        if (stagePools == null) yield break;
        if (stagePools.TryResolveLoadedScene(SceneManager.GetActiveScene().name, out var initial))
        {
            CurrentRoom = initial;
            SetProgress(initial.ProgressIndex);
        }
        if (CurrentRoom != null)
        {
            if (!ValidateCompletionRewards(CurrentRoom, out string error))
            { Debug.LogError($"Invalid completion rewards for {CurrentRoom.name}: {error}", CurrentRoom.Context); yield break; }
            PrepareRoom();
        }
    }

    private void EnsureStagePools()
    {
        if (stagePools == null) stagePools = Resources.Load<StagePoolDatabase>("StagePools");
        if (stagePools == null) Debug.LogError("Missing Resources/StagePools database.", this);
    }

    private void SetProgress(int level)
    {
        CurrentLevelIndex = level;
        nowclass = level > 0 ? level % 10 : 0;
        nowLevel = StagePoolDatabase.ChapterForProgress(level);
    }

    private bool TryLoadStage(int level, StageRoomSelection selectedRoom, bool portalRecovery)
    {
        if (isLoadingLevel) return false;
        EnsureStagePools();
        if (stagePools == null) return false;
        StageRoomSelection room = selectedRoom;
        if (level >= 0)
        {
            if (room == null && CurrentRoom != null && CurrentLevelIndex == level) room = CurrentRoom;
            if (room == null && !stagePools.TryDrawArrival(level, () => Random.value, out room, out string drawError))
            { Debug.LogError(drawError, this); return false; }
            if (room == null || room.ProgressIndex != level || room.Scene == null || room.Door == null)
            { Debug.LogError("Invalid captured stage selection for progress " + level, this); return false; }
        }
        else if (level != -1) return false;
        string scene = level == -1 ? "Title" : room != null ? room.sceneName : null;
        if (string.IsNullOrEmpty(scene) || !Application.CanStreamedLevelBeLoaded(scene))
        { Debug.LogError($"Stage scene is not enabled in Build Settings: {scene}", this); return false; }
        if (!ValidateCompletionRewards(room, out string rewardError, level))
        { Debug.LogError($"Invalid completion rewards for {room.name}: {rewardError}", room.Context); return false; }
        isLoadingLevel = true;
        if (portalRecovery) ValueData.Instance.ApplyPortalRecovery();
        if (Skill.Instance != null) Skill.Instance.StopAllSunfire();
        StartCoroutine(LoadSceneWithProgress(level, room));
        return true;
    }

    public bool TryEnterPortal(Npc portal)
    {
        if (portal == null || !portal.gameObject.activeInHierarchy ||
            !offeredDoors.TryGetValue(portal, out StagePortalChoice choice)) return false;
        return TryLoadStage(choice.ProgressIndex, choice.Room, true);
    }

    private void PrepareRoom()
    {
        offeredDoors.Clear();
        doorAnchors.Clear();
        generatedDoors = null;
        // Use the existing authored door positions, including inactive combat exits.
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Npc door in root.GetComponentsInChildren<Npc>(true))
            {
                if (door.npcType != NpcType.Door) continue;
                doorAnchors.Add(new DoorAnchor { position = door.transform.position,
                    rotation = door.transform.rotation, scale = door.transform.lossyScale });
                door.gameObject.SetActive(false);
            }
        }
        Enemys = GameObject.Find("Enemys");
        InitializeStageEnemies();
        if (CurrentRoom.completion == StageCompletionRule.OpenOnEntry)
        {
            // Merchant/home/event rooms have their own contents. No combat reward settlement.
            stageCompleted = true;
            requiredEnemies.Clear();
            leftEnemy = 0;
            CreateStageExits();
        }
        else if (leftEnemy == 0) CompleteStage();
    }

    private void CompleteStage()
    {
        if (stageCompleted) return;
        stageCompleted = true;
        getPrize();
        CreateStageExits();
    }

    private void CreateStageExits()
    {
        if (CurrentRoom == null || generatedDoors != null || CurrentRoom.exitCount <= 0) return;
        if (!stagePools.TryDrawChoices(CurrentLevelIndex, () => Random.value, out var choices, out string error,
            CurrentRoom.exitCount))
        { Debug.LogError(error, this); return; }
        if (choices.Count == 0) return;
        generatedDoors = new GameObject("Stage Exits (Runtime)").transform;
        for (int i = 0; i < choices.Count; i++)
        {
            StageRoomSelection target = choices[i].Room;
            GameObject entrance = target.EntrancePrefab;
            if (entrance == null)
            { Debug.LogError($"Room {target.name} has no entrance prefab.", target.Context); continue; }
            DoorAnchor anchor;
            if (i < doorAnchors.Count) anchor = doorAnchors[i];
            else
            {
                anchor = doorAnchors.Count > 0 ? doorAnchors[0] :
                    new DoorAnchor { position = new Vector3(0, 0.3f, 8), rotation = Quaternion.identity, scale = Vector3.one };
                anchor.position += anchor.rotation * Vector3.right * (4f * (i - doorAnchors.Count + 1));
            }
            GameObject instance = Instantiate(entrance, anchor.position, anchor.rotation, generatedDoors);
            instance.transform.localScale = anchor.scale;
            Npc door = instance.GetComponent<Npc>();
            if (door == null) { Destroy(instance); Debug.LogError($"Room {target.name} entrance has no Npc.", target.Context); continue; }
            door.Level = choices[i].ProgressIndex;
            var skillReward = target.rewards?.Find(reward => reward != null && reward.type == StageRewardType.RandomSkill);
            door.Prize = target.kind == StageRoomKind.Skill ? PrizeBase.RandomSkill : PrizeBase.None;
            if (skillReward != null) door.randomSkillRewardLevel = skillReward.skillLevel;
            door.Name = StageEntranceCatalog.NameKeyFor(target.kind);
            door.SetNameVisible(false);
            offeredDoors.Add(door, choices[i]);
            instance.SetActive(true);
        }
    }
}
