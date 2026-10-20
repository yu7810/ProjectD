using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(StagePoolDatabase))]
public sealed class ProjectDStagePoolDatabaseEditor : Editor
{
    private ReorderableList rewards;
    private SerializedProperty rewardData;

    [MenuItem("Tools/ProjectD/Stages/章節金幣獎勵")]
    public static void Open()
    {
        var database = Resources.Load<StagePoolDatabase>("StagePools");
        if (database == null) { Debug.LogError("Missing Resources/StagePools database."); return; }
        Selection.activeObject = database;
        EditorGUIUtility.PingObject(database);
    }

    private void OnEnable()
    {
        rewardData = serializedObject.FindProperty("chapterMoneyRewards");
        rewards = new ReorderableList(serializedObject, rewardData, true, true, true, true);
        rewards.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "章節金幣關獎勵");
        rewards.elementHeight = EditorGUIUtility.singleLineHeight + 8;
        rewards.drawElementCallback = (rect, index, active, focused) =>
        {
            var row = rewardData.GetArrayElementAtIndex(index);
            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;
            float half = (rect.width - 10) / 2;
            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 42;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, half, rect.height), row.FindPropertyRelative("chapter"), new GUIContent("章節"));
            EditorGUI.PropertyField(new Rect(rect.x + half + 10, rect.y, half, rect.height), row.FindPropertyRelative("money"), new GUIContent("金幣"));
            EditorGUIUtility.labelWidth = previousLabelWidth;
        };
        rewards.onAddCallback = list =>
        {
            var used = new HashSet<int>();
            for (int i = 0; i < rewardData.arraySize; i++) used.Add(rewardData.GetArrayElementAtIndex(i).FindPropertyRelative("chapter").intValue);
            int chapter = 1;
            while (used.Contains(chapter)) chapter++;
            int index = rewardData.arraySize;
            rewardData.InsertArrayElementAtIndex(index);
            var row = rewardData.GetArrayElementAtIndex(index);
            row.FindPropertyRelative("chapter").intValue = chapter;
            row.FindPropertyRelative("money").intValue = 0;
            list.index = index;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "chapterMoneyRewards", "pools");
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("金幣關通關獎勵依大章節查表。同章每個金幣關給相同金額，不再隨小關變動或隨機。0 表示不給金幣；不影響敵人掉落與 BOSS 獎勵。", MessageType.Info);
        rewards.DoLayoutList();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pools"), true);
        serializedObject.ApplyModifiedProperties();
        foreach (string error in ((StagePoolDatabase)target).ChapterMoneyRewardErrors())
            EditorGUILayout.HelpBox(error, MessageType.Error);
    }
}
