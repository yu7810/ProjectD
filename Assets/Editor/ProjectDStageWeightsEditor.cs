using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StagePool))]
public sealed class ProjectDStagePoolDrawer : PropertyDrawer
{
    private static readonly string[] Fields = { "progressIndex", "nextProgressIndex", "exitCount", "exits", "combatScenes", "bossScenes", "storeScenes" };
    private static readonly string[] Labels = { "本關進度", "下一關進度", "本關結束的門數", "本關結束：門類型池", "進入本關：戰鬥場景池", "進入本關：BOSS 場景池", "進入本關：商店場景池" };
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight + 4;
        if (property.isExpanded) foreach (string field in Fields) height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(field), true) + 4;
        return height;
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var row = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        string title = "第 " + property.FindPropertyRelative("progressIndex").intValue + " 關 → "
            + property.FindPropertyRelative("nextProgressIndex").intValue + "，通關後 "
            + property.FindPropertyRelative("exitCount").intValue + " 扇門";
        property.isExpanded = EditorGUI.Foldout(row, property.isExpanded, title, true);
        if (!property.isExpanded) return;
        EditorGUI.indentLevel++;
        for (int i = 0; i < Fields.Length; i++)
        {
            row.y += row.height + 4;
            var child = property.FindPropertyRelative(Fields[i]);
            row.height = EditorGUI.GetPropertyHeight(child, true);
            EditorGUI.PropertyField(row, child, new GUIContent(Labels[i]), true);
        }
        EditorGUI.indentLevel--;
    }
}

[CustomPropertyDrawer(typeof(StageDoorWeight))]
public sealed class ProjectDStageDoorWeightDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUI.GetPropertyHeight(property.FindPropertyRelative("door"), true)
            + EditorGUIUtility.singleLineHeight + 6;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var row = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(row, property.FindPropertyRelative("weight"), new GUIContent("抽選權重"));
        row.y += row.height + 6;
        var door = property.FindPropertyRelative("door");
        row.height = EditorGUI.GetPropertyHeight(door, true);
        EditorGUI.PropertyField(row, door, new GUIContent("門類型與獎勵"), true);
        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(StageDoorDefinition))]
public sealed class ProjectDStageDoorDefinitionDrawer : PropertyDrawer
{
    private static float Gap => EditorGUIUtility.singleLineHeight + 4;
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => Gap * 2 + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("rewards"), true) + 4;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var row = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(row, property.FindPropertyRelative("kind"), new GUIContent("關卡／門類型"));
        row.y += Gap;
        var kind = (StageRoomKind)property.FindPropertyRelative("kind").enumValueIndex;
        using (new EditorGUI.DisabledScope(true))
            EditorGUI.ObjectField(row, "門外觀（自動）", StageEntranceCatalog.Default != null
                ? StageEntranceCatalog.Default.PrefabFor(kind) : null, typeof(GameObject), false);
        row.y += Gap;
        var rewards = property.FindPropertyRelative("rewards");
        row.height = EditorGUI.GetPropertyHeight(rewards, true);
        EditorGUI.PropertyField(row, rewards, new GUIContent("通關獎勵清單（全部發放）"), true);
        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(StageSceneWeight))]
public sealed class ProjectDStageSceneWeightDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(new Rect(position.x, position.y, position.width - 75, position.height), property.FindPropertyRelative("scene"), GUIContent.none);
        EditorGUI.PropertyField(new Rect(position.xMax - 70, position.y, 70, position.height), property.FindPropertyRelative("weight"), GUIContent.none);
    }
}
