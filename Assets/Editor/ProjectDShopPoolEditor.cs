using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using I2.Loc;

public sealed class ProjectDShopPoolEditor : EditorWindow
{
    private const string AssetPath = "Assets/Resources/ShopPool.csv";
    private ShopPoolTable table;
    [SerializeField] private int selectedTab;
    [SerializeField] private string[] searches = { "", "" };
    [SerializeField] private int[] groupFilters = { 0, 0 };
    [SerializeField] private int[] stateFilters = { 0, 0 };
    [SerializeField] private Vector2[] scrollPositions = new Vector2[2];
    private readonly Dictionary<string, bool> expandedGroups = new Dictionary<string, bool>();
    private readonly Dictionary<int, RarityType> rarities = new Dictionary<int, RarityType>();
    private string error, loadedCsv;
    private string nameError;
    private string rarityError;
    private bool dirty;
    private readonly Dictionary<string, string> names = new Dictionary<string, string>();
    private static readonly string[] RarityLabels = { "Normal（灰）", "Magic（藍）", "Rare（黃）", "Unique（紫）" };

    [MenuItem("Tools/ProjectD/商店隨機池/編輯共用池")]
    public static void Open() => GetWindow<ProjectDShopPoolEditor>("商店共用池");

    private void OnEnable()
    {
        minSize = new Vector2(640, 380);
        LocalizationManager.OnLocalizeEvent += RefreshLocalizedNames;
        Load();
    }

    private void OnDisable() => LocalizationManager.OnLocalizeEvent -= RefreshLocalizedNames;
    private void OnProjectChange() => RefreshDisplayData();
    private void OnFocus() => RefreshDisplayData();

    private void RefreshDisplayData()
    {
        RefreshLocalizedNames();
        RefreshWeaponRarities();
    }

    private void RefreshWeaponRarities()
    {
        rarities.Clear();
        rarityError = null;
        try
        {
            foreach (var weapon in WeaponDataTable.Parse(File.ReadAllText("Assets/Weapon.csv")).CreateWeapons())
                rarities[weapon.ID] = weapon.Rarity;
        }
        catch (Exception e) { rarityError = "裝備稀有度讀取失敗：" + e.Message; }
        Repaint();
    }

    private void RefreshLocalizedNames()
    {
        // Refresh names without discarding unsaved pool edits.
        nameError = ProjectDShopEditorNames.Read(names);
        Repaint();
    }

    private static string NameKey(ShopProduct product) => ProjectDShopEditorNames.Key(product.Type, product.ItemID);
    private void Load()
    {
        try
        {
            string csv = File.ReadAllText(AssetPath, new UTF8Encoding(false, true));
            var incoming = ShopPoolTable.Parse(csv);
            loadedCsv = csv;
            table = incoming;
            dirty = false;
            error = null;
            RefreshDisplayData();
        }
        catch (Exception e) { error = e.Message; }
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("這份共用池設定商品權重；各商人的稀有度／技能等級機率仍在 NPC 設定。權重 0 代表停用。搜尋與篩選只影響顯示，儲存時會保留全部商品。", MessageType.Info);
        if (!string.IsNullOrEmpty(error)) EditorGUILayout.HelpBox(error, MessageType.Error);
        if (!string.IsNullOrEmpty(nameError)) EditorGUILayout.HelpBox(nameError, MessageType.Warning);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("重新讀取") && (!dirty || EditorUtility.DisplayDialog("重新讀取", "放棄尚未儲存的權重修改？", "重新讀取", "取消"))) Load();
            using (new EditorGUI.DisabledScope(table == null || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling))
                if (GUILayout.Button(dirty ? "儲存共用池 *" : "儲存共用池")) Save();
        }
        if (table == null) return;
        selectedTab = GUILayout.Toolbar(selectedTab, new[] { "裝備", "技能" });
        bool weaponTab = selectedTab == 0;
        if (weaponTab && !string.IsNullOrEmpty(rarityError)) EditorGUILayout.HelpBox(rarityError, MessageType.Warning);
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("搜尋名稱／ID", GUILayout.Width(90));
            searches[selectedTab] = EditorGUILayout.TextField(searches[selectedTab]);
            if (GUILayout.Button("清除篩選", GUILayout.Width(80)))
            {
                searches[selectedTab] = "";
                groupFilters[selectedTab] = stateFilters[selectedTab] = 0;
            }
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            groupFilters[selectedTab] = EditorGUILayout.Popup(groupFilters[selectedTab], weaponTab
                ? new[] { "全部稀有度", RarityLabels[0], RarityLabels[1], RarityLabels[2], RarityLabels[3], "未知稀有度" }
                : new[] { "全部等級", "等級 1", "等級 2", "等級 3" }, GUILayout.Width(145));
            stateFilters[selectedTab] = EditorGUILayout.Popup(stateFilters[selectedTab],
                new[] { "全部狀態", "只看啟用（權重 > 0）", "只看停用（權重 = 0）" }, GUILayout.Width(175));
        }
        var visible = VisibleProducts();
        var groups = visible.GroupBy(GroupID).ToList();
        int total = table.Products.Count(p => p.Type == CurrentType);
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label($"符合篩選：{visible.Count} / {total} 件");
            if (GUILayout.Button("全部展開", GUILayout.Width(75)))
                foreach (var group in groups) expandedGroups[GroupKey(group.Key)] = true;
            if (GUILayout.Button("全部收合", GUILayout.Width(75)))
                foreach (var group in groups) expandedGroups[GroupKey(group.Key)] = false;
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("ID", GUILayout.Width(50));
            GUILayout.Label("商品名稱", GUILayout.MinWidth(150));
            GUILayout.Label(weaponTab ? "稀有度" : "等級", GUILayout.Width(120));
            GUILayout.Label("商品權重", GUILayout.Width(100));
            GUILayout.Space(52);
        }
        ShopProduct remove = null;
        scrollPositions[selectedTab] = EditorGUILayout.BeginScrollView(scrollPositions[selectedTab]);
        foreach (var group in groups)
        {
            string key = GroupKey(group.Key);
            bool expanded = !expandedGroups.TryGetValue(key, out bool saved) || saved;
            string title = weaponTab ? RarityLabel(group.Key) : ProductName(group.First()) + "（ID " + group.Key + "）";
            expanded = EditorGUILayout.Foldout(expanded, title + " — " + group.Count() + " 件", true);
            expandedGroups[key] = expanded;
            if (!expanded) continue;
            foreach (var p in group)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    p.ItemID = EditorGUILayout.IntField(p.ItemID, GUILayout.Width(50));
                    GUILayout.Label(new GUIContent(ProductName(p), NameKey(p)), GUILayout.MinWidth(150));
                    if (weaponTab) GUILayout.Label(RarityLabel(WeaponGroup(p)), GUILayout.Width(120));
                    else p.Level = EditorGUILayout.IntPopup(p.Level, new[] { "等級 1", "等級 2", "等級 3" }, new[] { 1, 2, 3 }, GUILayout.Width(120));
                    p.Weight = EditorGUILayout.DoubleField(p.Weight, GUILayout.Width(100));
                    if (EditorGUI.EndChangeCheck()) dirty = true;
                    if (GUILayout.Button("移除", GUILayout.Width(48))) remove = p;
                }
            }
        }
        if (visible.Count == 0) EditorGUILayout.HelpBox("沒有符合條件的商品，可清除搜尋或調整篩選。", MessageType.Info);
        EditorGUILayout.EndScrollView();
        if (remove != null) { table.Products.Remove(remove); dirty = true; }
        if (GUILayout.Button(weaponTab ? "新增裝備商品" : "新增技能商品")) AddProduct();
    }

    private ShopProductType CurrentType => selectedTab == 0 ? ShopProductType.Weapon : ShopProductType.Skill;
    private string ProductName(ShopProduct p) => names.TryGetValue(NameKey(p), out string name) ? name : NameKey(p);
    private int WeaponGroup(ShopProduct p) => rarities.TryGetValue(p.ItemID, out RarityType rarity) ? (int)rarity : -1;
    private static string RarityLabel(int group) => group >= 0 && group < RarityLabels.Length ? RarityLabels[group] : "未知稀有度";
    private int GroupID(ShopProduct p) => selectedTab == 0 ? WeaponGroup(p) : p.ItemID;
    private string GroupKey(int id) => CurrentType + "/" + id;

    private bool MatchesGroup(ShopProduct p)
    {
        int filter = groupFilters[selectedTab];
        if (filter == 0) return true;
        return selectedTab == 0 ? WeaponGroup(p) == (filter == 5 ? -1 : filter - 1) : p.Level == filter;
    }

    private List<ShopProduct> VisibleProducts()
    {
        string query = (searches[selectedTab] ?? "").Trim();
        int state = stateFilters[selectedTab];
        var matches = table.Products.Where(p => p.Type == CurrentType && MatchesGroup(p) &&
            (state == 0 || (state == 1 ? p.Weight > 0 : p.Weight == 0)) &&
            (query.Length == 0 || p.ItemID.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
             ProductName(p).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0));
        return matches.OrderBy(p => selectedTab == 0 ? (WeaponGroup(p) < 0 ? int.MaxValue : WeaponGroup(p)) : p.ItemID)
            .ThenBy(p => p.ItemID).ThenBy(p => p.Level).ToList();
    }

    private void AddProduct()
    {
        var candidates = new List<ShopProduct>();
        if (selectedTab == 0)
            for (int id = 1; id < WeaponDataTable.WeaponCount; id++)
                candidates.Add(new ShopProduct { Type = ShopProductType.Weapon, ItemID = id, Level = 0 });
        else
            foreach (int id in SkillLevelTable.ActiveIds)
                for (int level = 1; level <= SkillLevelTable.MaxLevel; level++)
                    candidates.Add(new ShopProduct { Type = ShopProductType.Skill, ItemID = id, Level = level });
        var product = candidates.FirstOrDefault(p => MatchesGroup(p) &&
            !table.Products.Any(existing => existing.Type == p.Type && existing.ItemID == p.ItemID && existing.Level == p.Level));
        if (product == null)
        {
            ShowNotification(new GUIContent("此範圍的商品已全部加入，可調整現有商品權重。"));
            return;
        }
        product.Weight = stateFilters[selectedTab] == 2 ? 0 : 100;
        table.Products.Add(product);
        searches[selectedTab] = "";
        expandedGroups[GroupKey(GroupID(product))] = true;
        dirty = true;
    }

    private void Save()
    {
        byte[] previous = null;
        bool wrote = false;
        try
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            string csv = table.ToCsv(); // Validate before backup or disk changes.
            if (File.ReadAllText(AssetPath, new UTF8Encoding(false, true)) != loadedCsv)
                throw new IOException("本機共用池已被其他程式更新，請先重新讀取再修改，避免覆蓋。");
            previous = File.ReadAllBytes(AssetPath);
            string backup = Path.GetFullPath(Path.Combine(Application.dataPath, "../../ProjectD AI/Shop Pool Backups", DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")));
            Directory.CreateDirectory(backup);
            File.WriteAllBytes(Path.Combine(backup, "ShopPool.before.csv"), previous);
            File.WriteAllText(AssetPath, csv, new UTF8Encoding(false));
            wrote = true;
            AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);
            if (File.ReadAllText(AssetPath) != csv) throw new IOException("共用池寫入驗證失敗。");
            loadedCsv = csv;
            dirty = false;
            error = null;
        }
        catch (Exception e)
        {
            if (wrote && previous != null)
            {
                File.WriteAllBytes(AssetPath, previous);
                AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);
            }
            error = e.Message;
        }
    }
}
