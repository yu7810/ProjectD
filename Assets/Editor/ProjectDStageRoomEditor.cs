using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(StageRewardEntry))]
public sealed class ProjectDStageRewardDrawer : PropertyDrawer
{
    private static readonly string[] Labels = { "金幣", "天賦點", "天賦樹三選一", "生命", "隨機技能", "技能商店", "裝備商店" };
    private static float Line => EditorGUIUtility.singleLineHeight + 4f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var type = (StageRewardType)property.FindPropertyRelative("type").enumValueIndex;
        int lines = type == StageRewardType.SkillStore || type == StageRewardType.WeaponStore ? 1
            : type == StageRewardType.Life || type == StageRewardType.RandomSkill ? 3
            : type == StageRewardType.Money && !property.FindPropertyRelative("useChapterMoney").boolValue ? 3 : 2;
        return Line * lines;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var row = new Rect(position.x, position.y + 2, position.width, EditorGUIUtility.singleLineHeight);
        var type = property.FindPropertyRelative("type");
        int next = EditorGUI.Popup(row, "獎勵類型", type.enumValueIndex, Labels);
        if (next != type.enumValueIndex)
        {
            type.enumValueIndex = next;
            if (next == (int)StageRewardType.RandomSkill)
                property.FindPropertyRelative("skillPool").objectReferenceValue = Resources.Load<SkillRewardPool>("SkillRewardPool");
        }
        row.y += Line;
        switch ((StageRewardType)type.enumValueIndex)
        {
            case StageRewardType.Money:
                var chapter = property.FindPropertyRelative("useChapterMoney");
                EditorGUI.PropertyField(row, chapter, new GUIContent("依章節金幣表"));
                if (!chapter.boolValue)
                {
                    row.y += Line;
                    EditorGUI.PropertyField(row, property.FindPropertyRelative("amount"), new GUIContent("金幣數量"));
                }
                break;
            case StageRewardType.PassivePoints:
                EditorGUI.PropertyField(row, property.FindPropertyRelative("amount"), new GUIContent("數量")); break;
            case StageRewardType.PassiveTree:
                EditorGUI.PropertyField(row, property.FindPropertyRelative("allowPassiveTreeReroll"), new GUIContent("允許重骰")); break;
            case StageRewardType.Life:
                EditorGUI.PropertyField(row, property.FindPropertyRelative("lifeMaxIncrease"), new GUIContent("最大生命增加量"));
                row.y += Line;
                EditorGUI.PropertyField(row, property.FindPropertyRelative("lifeRecovery"), new GUIContent("恢復生命量")); break;
            case StageRewardType.RandomSkill:
                EditorGUI.PropertyField(row, property.FindPropertyRelative("skillPool"), new GUIContent("技能獎勵池"));
                row.y += Line;
                EditorGUI.PropertyField(row, property.FindPropertyRelative("skillLevel"), new GUIContent("技能等級")); break;
        }
        EditorGUI.EndProperty();
    }
}

[CustomEditor(typeof(StageRoomDefinition))]
public sealed class ProjectDStageRoomEditor : Editor
{
    private ReorderableList rewards;

    private void OnEnable()
    {
        var data = serializedObject.FindProperty("rewards");
        rewards = new ReorderableList(serializedObject, data, true, true, true, true);
        rewards.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "通關獎勵清單（全部發放，每列獨立掉落）");
        rewards.elementHeightCallback = index => EditorGUI.GetPropertyHeight(data.GetArrayElementAtIndex(index)) + 8;
        rewards.drawElementCallback = (rect, index, active, focused) =>
            EditorGUI.PropertyField(rect, data.GetArrayElementAtIndex(index), GUIContent.none, true);
        rewards.onAddCallback = _ =>
        {
            data.InsertArrayElementAtIndex(data.arraySize);
            var entry = data.GetArrayElementAtIndex(data.arraySize - 1);
            entry.FindPropertyRelative("type").enumValueIndex = (int)StageRewardType.PassivePoints;
            entry.FindPropertyRelative("amount").intValue = 2;
            entry.FindPropertyRelative("useChapterMoney").boolValue = false;
            entry.FindPropertyRelative("allowPassiveTreeReroll").boolValue = true;
            entry.FindPropertyRelative("lifeMaxIncrease").floatValue = 3;
            entry.FindPropertyRelative("lifeRecovery").floatValue = 3;
            entry.FindPropertyRelative("skillPool").objectReferenceValue = null;
            entry.FindPropertyRelative("skillLevel").intValue = 1;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "rewards");
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.ObjectField("門外觀（Kind 自動）", ((StageRoomDefinition)target).EntrancePrefab, typeof(GameObject), false);
        EditorGUILayout.HelpBox("這是保留供參考的舊版獨立資料。遊戲已改用 StagePools 內的門類型與獎勵，請從 Tools → ProjectD → Stages → 關卡權重設定 編輯。", MessageType.Info);
        EditorGUILayout.Space();
        rewards.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
        var room = (StageRoomDefinition)target;
        EditorGUILayout.HelpBox(room.HasRewards
            ? "清單內的獎勵全部發放，每列各掉落／抽取一次。道具外觀自動依獎勵類型使用對應 Prefab。"
            : "清單為空：沒有通關獎勵。", MessageType.Info);
        if (!room.ValidateRewards(out string error)) EditorGUILayout.HelpBox(error, MessageType.Error);
    }
}
