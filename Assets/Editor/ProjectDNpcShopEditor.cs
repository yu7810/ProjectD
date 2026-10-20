using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I2.Loc;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(Npc)), CanEditMultipleObjects]
public sealed class ProjectDNpcShopEditor : Editor
{
    private static readonly string[] WeaponFields = { "normalRarityChance", "magicRarityChance", "rareRarityChance", "uniqueRarityChance" };
    private static readonly string[] SkillFields = { "skillLevel1Chance", "skillLevel2Chance", "skillLevel3Chance" };
    private readonly Dictionary<string, string> names = new Dictionary<string, string>();
    private readonly Dictionary<int, RarityType> rarities = new Dictionary<int, RarityType>();
    private readonly AdvancedDropdownState dropdownState = new AdvancedDropdownState();
    private ReorderableList stock;
    private SerializedProperty items, levels;
    private string nameError, rarityError;
    private bool showSpecified;
    private ProjectDShopProductDropdown dropdown;

    private bool SkillShop => serializedObject.FindProperty("npcType").enumValueIndex == (int)NpcType.Skillstore;
    private ShopProductType ProductType => SkillShop ? ShopProductType.Skill : ShopProductType.Weapon;
    private bool StockAligned => !SkillShop || items.arraySize == levels.arraySize;

    private void OnEnable()
    {
        items = serializedObject.FindProperty("item");
        levels = serializedObject.FindProperty("itemlevel");
        stock = new ReorderableList(serializedObject, items, true, true, true, true);
        stock.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "指定商品（" + items.arraySize + " 件）");
        stock.elementHeightCallback = i => EditorGUIUtility.singleLineHeight + 8 +
            (RowError(i) == null ? 0 : WarningHeight(i) + 4);
        stock.drawElementCallback = DrawProduct;
        stock.onCanAddCallback = list => StockAligned;
        stock.onCanRemoveCallback = list => StockAligned && list.index >= 0;
        stock.onAddCallback = list => AddProduct();
        stock.onRemoveCallback = list => RemoveProduct(list.index);
        stock.onReorderCallbackWithDetails = (list, from, to) => ReorderLevels(from, to);
        LocalizationManager.OnLocalizeEvent += RefreshCatalog;
        EditorApplication.projectChanged += RefreshCatalog;
        Undo.undoRedoPerformed += Repaint;
        RefreshCatalog();
    }

    private void OnDisable()
    {
        LocalizationManager.OnLocalizeEvent -= RefreshCatalog;
        EditorApplication.projectChanged -= RefreshCatalog;
        Undo.undoRedoPerformed -= Repaint;
    }

    private void RefreshCatalog()
    {
        nameError = ProjectDShopEditorNames.Read(names);
        rarities.Clear();
        rarityError = null;
        try
        {
            foreach (var w in WeaponDataTable.Parse(File.ReadAllText("Assets/Weapon.csv")).CreateWeapons()) rarities[w.ID] = w.Rarity;
        }
        catch (Exception e) { rarityError = "裝備稀有度讀取失敗：" + e.Message; }
        Repaint();
    }

    internal static string RarityLabel(int rarity) => rarity >= 0 && rarity < 4
        ? new[] { "Normal（灰）", "Magic（藍）", "Rare（黃）", "Unique（紫）" }[rarity] : "未知稀有度";

    private string ProductName(int id) => ProjectDShopEditorNames.Get(names, ProductType, id);
    private int Rarity(int id) => rarities.TryGetValue(id, out RarityType value) ? (int)value : -1;
    private bool ValidID(int id) => SkillShop ? SkillLevelTable.ActiveIds.Contains(id) : id > 0 && id < WeaponDataTable.WeaponCount;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var type = serializedObject.FindProperty("npcType");
        var random = serializedObject.FindProperty("startRandom");
        bool merchant = !type.hasMultipleDifferentValues &&
            (type.enumValueIndex == (int)NpcType.Skillstore || type.enumValueIndex == (int)NpcType.Weaponstore);
        var excluded = WeaponFields.Concat(SkillFields).Concat(merchant ? new[] { "item", "itemlevel" } : Array.Empty<string>()).ToArray();
        DrawPropertiesExcluding(serializedObject, excluded);
        if (!merchant) { serializedObject.ApplyModifiedProperties(); return; }
        if (!type.hasMultipleDifferentValues && !random.hasMultipleDifferentValues && random.boolValue &&
            (type.enumValueIndex == (int)NpcType.Skillstore || type.enumValueIndex == (int)NpcType.Weaponstore))
        {
            bool skill = type.enumValueIndex == (int)NpcType.Skillstore;
            string[] fields = skill ? SkillFields : WeaponFields;
            string[] labels = skill ? new[] { "技能等級 1", "技能等級 2", "技能等級 3" }
                : new[] { "Normal（灰）", "Magic（藍）", "Rare（黃）", "Unique（紫）" };
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(skill ? "技能等級出現機率" : "裝備稀有度出現機率", EditorStyles.boldLabel);
            for (int i = 0; i < fields.Length; i++)
                EditorGUILayout.PropertyField(serializedObject.FindProperty(fields[i]), new GUIContent(labels[i]));
            var properties = fields.Select(serializedObject.FindProperty).ToArray();
            if (!properties.Any(p => p.hasMultipleDifferentValues))
            {
                double total = properties.Sum(p => (double)p.floatValue);
                if (properties.Any(p => !ShopPoolTable.IsWeight(p.floatValue)) || total <= 0)
                    EditorGUILayout.HelpBox("機率需為非負有限數值，且至少有一組大於 0；否則不會產生商品。", MessageType.Warning);
                else
                    EditorGUILayout.HelpBox("正規化機率：" + string.Join(" / ", properties.Select(p => (p.floatValue / total * 100).ToString("0.##") + "%")) +
                        "\n先抽稀有度／等級，再按共用池商品權重抽選。空組別會排除並重新分配機率。", MessageType.Info);
            }
            EditorGUILayout.HelpBox("指定商品清單不參與隨機。每次抽 3 件、不重複相同商品；技能 ID 相同但等級不同可同時出現。", MessageType.None);
            if (GUILayout.Button("編輯共用商品權重池")) ProjectDShopPoolEditor.Open();
        }
        else if (!random.hasMultipleDifferentValues && !random.boolValue && !type.hasMultipleDifferentValues &&
            (type.enumValueIndex == (int)NpcType.Skillstore || type.enumValueIndex == (int)NpcType.Weaponstore))
            EditorGUILayout.HelpBox("固定商店：維持指定商品，不提供刷新。", MessageType.Info);
        if (serializedObject.isEditingMultipleObjects)
            EditorGUILayout.HelpBox("商品清單請單獨選取一位商人編輯；機率設定仍支援多選。", MessageType.Info);
        else
        {
            bool randomStock = !random.hasMultipleDifferentValues && random.boolValue;
            if (randomStock) showSpecified = EditorGUILayout.Foldout(showSpecified, "指定商品（隨機模式不使用）", true);
            if (!randomStock || showSpecified) DrawStock();
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawStock()
    {
        if (!string.IsNullOrEmpty(nameError)) EditorGUILayout.HelpBox(nameError, MessageType.Warning);
        if (!SkillShop && !string.IsNullOrEmpty(rarityError)) EditorGUILayout.HelpBox(rarityError, MessageType.Warning);
        if (!StockAligned)
        {
            EditorGUILayout.HelpBox("商品與等級的數量不符。請先修正對應，才能增刪或排序；已有等級會保留，缺少的補為 1，多餘的移除。", MessageType.Warning);
            if (GUILayout.Button("修正等級對應"))
                RepairLevelAlignment();
        }
        stock.draggable = StockAligned;
        stock.DoLayoutList();
    }

    private string RowError(int index)
    {
        if (index < 0 || index >= items.arraySize) return null;
        int id = items.GetArrayElementAtIndex(index).intValue;
        if (!ValidID(id)) return "無效商品 ID " + id + (SkillShop ? "：請選擇已實作的技能。" : "：不可販售空手或不存在的裝備。");
        if (SkillShop && (index >= levels.arraySize || levels.GetArrayElementAtIndex(index).intValue < 1 ||
            levels.GetArrayElementAtIndex(index).intValue > SkillLevelTable.MaxLevel)) return "技能等級未設定或無效，請選擇 1～3 級。";
        return null;
    }

    private float WarningHeight(int index) => EditorStyles.helpBox.CalcHeight(new GUIContent(RowError(index)),
        Mathf.Max(120, EditorGUIUtility.currentViewWidth - 70));

    private void RepairLevelAlignment()
    {
        if (!SkillShop) return;
        int previous = levels.arraySize;
        levels.arraySize = items.arraySize;
        for (int i = previous; i < levels.arraySize; i++) levels.GetArrayElementAtIndex(i).intValue = 1;
        serializedObject.ApplyModifiedProperties();
    }

    private void ReorderLevels(int from, int to)
    {
        // ReorderableList has already moved the corresponding item element.
        if (SkillShop) levels.MoveArrayElement(from, to);
        serializedObject.ApplyModifiedProperties();
        Repaint();
    }

    private void DrawProduct(Rect rect, int index, bool active, bool focused)
    {
        if (index < 0 || index >= items.arraySize) return;
        rect.y += 2;
        rect.height = EditorGUIUtility.singleLineHeight;
        float detailWidth = SkillShop ? 72 : 90;
        var productRect = new Rect(rect.x, rect.y, Mathf.Max(40, rect.width - detailWidth - 6), rect.height);
        var detailRect = new Rect(productRect.xMax + 6, rect.y, detailWidth, rect.height);
        var property = items.GetArrayElementAtIndex(index);
        int id = property.intValue;
        var label = new GUIContent(ProductName(id) + "（ID " + id + "）", ProjectDShopEditorNames.Key(ProductType, id));
        EditorGUI.BeginProperty(productRect, label, property);
        if (EditorGUI.DropdownButton(productRect, label, FocusType.Keyboard, EditorStyles.popup)) OpenPicker(productRect, index);
        EditorGUI.EndProperty();
        if (SkillShop && index < levels.arraySize)
        {
            var levelProperty = levels.GetArrayElementAtIndex(index);
            var levelLabel = new GUIContent("等級 " + levelProperty.intValue);
            EditorGUI.BeginProperty(detailRect, levelLabel, levelProperty);
            EditorGUI.BeginChangeCheck();
            int level = EditorGUI.IntPopup(detailRect, levelProperty.intValue, new[] { "1 級", "2 級", "3 級" }, new[] { 1, 2, 3 });
            if (EditorGUI.EndChangeCheck()) levelProperty.intValue = level;
            EditorGUI.EndProperty();
        }
        else EditorGUI.LabelField(detailRect, SkillShop ? "未設定等級" : RarityLabel(Rarity(id)));
        string warning = RowError(index);
        if (warning != null)
            EditorGUI.HelpBox(new Rect(rect.x, rect.yMax + 4, rect.width, WarningHeight(index)), warning, MessageType.Warning);
    }

    private void OpenPicker(Rect rect, int index)
    {
        // A popup callback must not apply to a row that changed while it was open.
        ShopProductType expectedType = ProductType;
        int expectedID = items.GetArrayElementAtIndex(index).intValue, expectedCount = items.arraySize;
        dropdown = new ProjectDShopProductDropdown(dropdownState, expectedType, ProductName, Rarity, id =>
        {
            if (this == null || target == null || serializedObject.isEditingMultipleObjects) return;
            serializedObject.Update();
            int expectedNpcType = expectedType == ShopProductType.Skill ? (int)NpcType.Skillstore : (int)NpcType.Weaponstore;
            if (serializedObject.FindProperty("npcType").enumValueIndex != expectedNpcType ||
                ProductType != expectedType || items.arraySize != expectedCount || index >= items.arraySize ||
                items.GetArrayElementAtIndex(index).intValue != expectedID || !ValidID(id)) return;
            items.GetArrayElementAtIndex(index).intValue = id;
            serializedObject.ApplyModifiedProperties();
            Repaint();
        });
        dropdown.Show(rect);
    }

    private void AddProduct()
    {
        if (!StockAligned) return;
        int index = items.arraySize;
        items.InsertArrayElementAtIndex(index);
        items.GetArrayElementAtIndex(index).intValue = SkillShop ? SkillLevelTable.ActiveIds[0] : 1;
        if (SkillShop)
        {
            levels.InsertArrayElementAtIndex(index);
            levels.GetArrayElementAtIndex(index).intValue = 1;
        }
        stock.index = index;
        serializedObject.ApplyModifiedProperties();
    }

    private void RemoveProduct(int index)
    {
        if (!StockAligned || index < 0 || index >= items.arraySize) return;
        items.DeleteArrayElementAtIndex(index);
        if (SkillShop) levels.DeleteArrayElementAtIndex(index);
        stock.index = Mathf.Min(index, items.arraySize - 1);
        serializedObject.ApplyModifiedProperties();
    }
}
