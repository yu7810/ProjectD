using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(SkillRewardPool))]
public sealed class ProjectDSkillRewardPoolEditor : Editor
{
    private int tab;
    private readonly Dictionary<string, string> names = new Dictionary<string, string>();
    private readonly ReorderableList[] lists = new ReorderableList[4];

    [MenuItem("Tools/ProjectD/Rewards/技能獎勵池")]
    public static void Open()
    {
        Selection.activeObject = Resources.Load<SkillRewardPool>("SkillRewardPool");
        if (Selection.activeObject != null) EditorGUIUtility.PingObject(Selection.activeObject);
    }

    private void OnEnable()
    {
        for (int i = 0; i < 4; i++)
        {
            var property = serializedObject.FindProperty("level" + (i + 1));
            var list = new ReorderableList(serializedObject, property, true, true, true, true);
            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "技能（多語系名稱）                                      權重");
            list.elementHeight = EditorGUIUtility.singleLineHeight + 6;
            list.drawElementCallback = (rect, index, active, focused) =>
            {
                var row = property.GetArrayElementAtIndex(index);
                var id = row.FindPropertyRelative("skillId");
                string[] labels = SkillLevelTable.ActiveIds.Select(value => value + "  " +
                    ProjectDShopEditorNames.Get(names, ShopProductType.Skill, value)).ToArray();
                rect.y += 2; rect.height = EditorGUIUtility.singleLineHeight;
                int selected = Array.IndexOf(SkillLevelTable.ActiveIds, id.intValue);
                int next = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width - 85, rect.height), selected, labels);
                if (next >= 0) id.intValue = SkillLevelTable.ActiveIds[next];
                EditorGUI.PropertyField(new Rect(rect.xMax - 75, rect.y, 75, rect.height), row.FindPropertyRelative("weight"), GUIContent.none);
            };
            list.onAddCallback = _ =>
            {
                var used = new HashSet<int>();
                for (int n = 0; n < property.arraySize; n++) used.Add(property.GetArrayElementAtIndex(n).FindPropertyRelative("skillId").intValue);
                int id = SkillLevelTable.ActiveIds.FirstOrDefault(value => !used.Contains(value));
                if (id == 0) return;
                property.InsertArrayElementAtIndex(property.arraySize);
                var row = property.GetArrayElementAtIndex(property.arraySize - 1);
                row.FindPropertyRelative("skillId").intValue = id;
                row.FindPropertyRelative("weight").floatValue = 1f;
            };
            lists[i] = list;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        string error = ProjectDShopEditorNames.Read(names);
        if (!string.IsNullOrEmpty(error)) EditorGUILayout.HelpBox(error, MessageType.Warning);
        tab = GUILayout.Toolbar(tab, new[] { "1 等", "2 等", "3 等", "4 等（預留）" });
        EditorGUILayout.HelpBox("只從傳送門指定等級的清單抽一個技能；機率為該技能權重／此池總權重。權重 0 表示不抽取。", MessageType.Info);
        if (tab + 1 > SkillLevelTable.MaxLevel)
            EditorGUILayout.HelpBox("目前數值表只支援 1～3 等。第 4 池可預先編輯，但不能作為傳送門獎勵。", MessageType.Warning);
        lists[tab].DoLayoutList();
        serializedObject.ApplyModifiedProperties();
        if (!((SkillRewardPool)target).ValidateLevel(tab + 1, out error))
            EditorGUILayout.HelpBox(error, MessageType.Warning);
    }
}
