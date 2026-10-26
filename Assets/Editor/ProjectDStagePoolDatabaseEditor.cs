using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(StagePoolDatabase))]
public sealed class ProjectDStagePoolDatabaseEditor : Editor
{
    private bool showMoney;
    private ReorderableList rewards;
    private SerializedProperty rewardData;

    [MenuItem("Tools/ProjectD/Stages/章節金幣獎勵")]
    [MenuItem("Tools/ProjectD/Stages/關卡權重設定")]
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
        EditorGUILayout.LabelField("據點（第 0 關）", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("home"), new GUIContent("據點門／獎勵資料"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("firstProgressIndex"), new GUIContent("出發的第一關"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("homeExitCount"), new GUIContent("據點門數"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("homeExits"), new GUIContent("離開據點的門類型池"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("homeScenes"), new GUIContent("返回據點的場景池"), true);
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("每關分開設定：進入本關的場景池，以及本關結束後的門類型池。先抽不重複的門，再從下一關對應的場景池各抽一次；門生成後固定結果，選門與重開不重抽。據點門回到第 0 關。", MessageType.Info);
        EditorGUILayout.HelpBox("門池各項直接設定權重、關卡／門類型與通關獎勵，不需另建門獎勵資產。場景池右側數字是相對權重；0 代表停用。一般獎勵門共用戰鬥場景池，不同門可以抽到相同場景。", MessageType.Info);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pools"), new GUIContent("逐關設定"), true);
        showMoney = EditorGUILayout.Foldout(showMoney, "章節金幣獎勵", true);
        if (showMoney) rewards.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
        foreach (string error in ((StagePoolDatabase)target).ConfigurationErrors())
            EditorGUILayout.HelpBox(error, MessageType.Error);
        foreach (string error in ((StagePoolDatabase)target).ChapterMoneyRewardErrors())
            EditorGUILayout.HelpBox(error, MessageType.Error);
    }
}
