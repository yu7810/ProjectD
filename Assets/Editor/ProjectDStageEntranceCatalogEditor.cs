using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StageEntranceCatalog))]
public sealed class ProjectDStageEntranceCatalogEditor : Editor
{
    [MenuItem("Tools/ProjectD/Stages/傳送門外觀")]
    public static void Open()
    {
        Selection.activeObject = StageEntranceCatalog.Default;
        if (Selection.activeObject != null) EditorGUIUtility.PingObject(Selection.activeObject);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        string[] fields = { "money", "talent", "talentTree", "life", "boss", "home", "store", "skill" };
        string[] labels = { "金幣", "天賦", "天賦樹", "生命", "BOSS", "據點", "商店", "技能" };
        for (int i = 0; i < fields.Length; i++)
            EditorGUILayout.PropertyField(serializedObject.FindProperty(fields[i]), new GUIContent(labels[i]));
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.HelpBox("關卡會依 Kind 自動使用此處對應的門外觀與多語系名稱；實際獎勵由關卡的通關獎勵清單決定。", MessageType.Info);
        if (!((StageEntranceCatalog)target).Validate(out string error)) EditorGUILayout.HelpBox(error, MessageType.Error);
    }
}
