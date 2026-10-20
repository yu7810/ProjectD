using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEditor;

// Shared by the NPC inspector and pool editor; game data Name columns are not used.
internal static class ProjectDShopEditorNames
{
    internal static string Key(ShopProductType type, int id) => type == ShopProductType.Skill
        ? "Skill/Skill Name " + id : "Weapon/Weapon Name " + id;

    internal static string Read(Dictionary<string, string> names)
    {
        names.Clear();
        var asset = AssetDatabase.LoadAssetAtPath<LanguageSourceAsset>("Assets/Resources/I2Languages.asset");
        var source = asset != null ? asset.mSource : null;
        int language = source?.mLanguages.FindIndex(l => l.Name == "Chinese (Taiwan)") ?? -1;
        if (language < 0) return "找不到本地 I2 多語系資料的繁中欄位；商品名稱暫以 Key 顯示。";
        // Importing may replace term data before I2 rebuilds its cached dictionary.
        foreach (var term in source.mTerms)
        {
            if (term == null || string.IsNullOrEmpty(term.Term) || term.Languages == null || language >= term.Languages.Length) continue;
            if (!term.Term.StartsWith("Skill/Skill Name ", StringComparison.Ordinal) &&
                !term.Term.StartsWith("Weapon/Weapon Name ", StringComparison.Ordinal)) continue;
            string value = term.Languages[language];
            if (!string.IsNullOrWhiteSpace(value)) names[term.Term] = value;
        }
        return null;
    }

    internal static string Get(Dictionary<string, string> names, ShopProductType type, int id)
    {
        string key = Key(type, id);
        return names.TryGetValue(key, out string name) ? name : key;
    }
}
