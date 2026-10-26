using System;
using UnityEngine;

public enum StageSceneCategory
{
    [InspectorName("戰鬥")] Combat,
    [InspectorName("BOSS")] Boss,
    [InspectorName("據點")] Home,
    [InspectorName("商店")] Store
}

[CreateAssetMenu(menuName = "ProjectD/Stages/Scene")]
public sealed class StageSceneDefinition : ScriptableObject
{
    public string sceneName;
    public StageSceneCategory category;
    public StageCompletionRule completion;

    public bool Validate(out string error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(sceneName)) error = "場景名稱不可為空。";
        else if (!Enum.IsDefined(typeof(StageSceneCategory), category)) error = "未知場景分類。";
        else if ((category == StageSceneCategory.Home || category == StageSceneCategory.Store)
            ? completion != StageCompletionRule.OpenOnEntry : completion != StageCompletionRule.ClearEnemies)
            error = "據點／商店需進場開門，戰鬥／BOSS需清除敵人。";
        return error == null;
    }

    public static StageSceneCategory CategoryFor(StageRoomKind kind)
    {
        if (kind == StageRoomKind.Boss) return StageSceneCategory.Boss;
        if (kind == StageRoomKind.Home) return StageSceneCategory.Home;
        if (kind == StageRoomKind.Store) return StageSceneCategory.Store;
        return StageSceneCategory.Combat;
    }
}
