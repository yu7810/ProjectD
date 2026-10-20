using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

internal sealed class ProjectDShopProductDropdown : AdvancedDropdown
{
    private readonly ShopProductType type;
    private readonly Func<int, string> name;
    private readonly Func<int, int> rarity;
    private readonly Action<int> selected;
    private readonly Dictionary<int, int> productIDs = new Dictionary<int, int>();

    internal ProjectDShopProductDropdown(AdvancedDropdownState state, ShopProductType type,
        Func<int, string> name, Func<int, int> rarity, Action<int> selected) : base(state)
    {
        this.type = type;
        this.name = name;
        this.rarity = rarity;
        this.selected = selected;
        minimumSize = new Vector2(300, 320);
    }

    protected override AdvancedDropdownItem BuildRoot()
    {
        productIDs.Clear();
        int nextID = 1;
        var root = new AdvancedDropdownItem(type == ShopProductType.Skill ? "選擇技能（搜尋名稱或 ID）" : "選擇裝備（搜尋名稱或 ID）") { id = nextID++ };
        IEnumerable<int> ids = type == ShopProductType.Skill ? SkillLevelTable.ActiveIds.OrderBy(id => id)
            : Enumerable.Range(1, WeaponDataTable.WeaponCount - 1);
        var groups = new Dictionary<int, AdvancedDropdownItem>();
        if (type == ShopProductType.Weapon)
        {
            foreach (int group in ids.Select(rarity).Distinct().OrderBy(g => g < 0 ? int.MaxValue : g))
            {
                var item = new AdvancedDropdownItem(ProjectDNpcShopEditor.RarityLabel(group)) { id = nextID++ };
                groups.Add(group, item);
                root.AddChild(item);
            }
        }
        foreach (int id in ids)
        {
            var item = new AdvancedDropdownItem(name(id) + "（ID " + id + "）") { id = nextID++ };
            productIDs.Add(item.id, id);
            (type == ShopProductType.Skill ? root : groups[rarity(id)]).AddChild(item);
        }
        return root;
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        if (productIDs.TryGetValue(item.id, out int id)) selected(id);
    }
}
