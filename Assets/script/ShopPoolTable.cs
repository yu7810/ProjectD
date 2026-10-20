using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

public enum ShopProductType { Weapon, Skill }

public sealed class ShopProduct
{
    public ShopProductType Type;
    public int ItemID, Level;
    public double Weight;
}

// No Unity dependencies: sampling and validation are also used by the editor.
public sealed class ShopPoolTable
{
    public static readonly string[] Headers = { "Type", "ItemID", "Level", "Weight" };
    public readonly List<ShopProduct> Products = new List<ShopProduct>();

    public static ShopPoolTable Parse(string csv)
    {
        var lines = (csv ?? "").TrimStart('\uFEFF').Replace("\r\n", "\n").Replace('\r', '\n')
            .Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        if (lines.Length == 0) throw new FormatException("商店共用池缺少表頭。");
        var header = lines[0].Split(',');
        if (header.Length != Headers.Length || !new HashSet<string>(header).SetEquals(Headers))
            throw new FormatException("商店共用池表頭必須為 Type、ItemID、Level、Weight。");
        var columns = Headers.Select(h => Array.IndexOf(header, h)).ToArray();
        var result = new ShopPoolTable();
        var keys = new HashSet<string>();
        for (int i = 1; i < lines.Length; i++)
        {
            var cells = lines[i].Split(',');
            if (cells.Length != Headers.Length) throw new FormatException($"第 {i + 1} 列欄位數不符。");
            string type = cells[columns[0]].Trim();
            if (type != "Weapon" && type != "Skill") throw new FormatException($"第 {i + 1} 列 Type 必須是 Weapon 或 Skill。");
            var product = new ShopProduct { Type = (ShopProductType)Enum.Parse(typeof(ShopProductType), type) };
            if (!int.TryParse(cells[columns[1]], NumberStyles.Integer, CultureInfo.InvariantCulture, out product.ItemID) ||
                !int.TryParse(cells[columns[2]], NumberStyles.Integer, CultureInfo.InvariantCulture, out product.Level) ||
                !double.TryParse(cells[columns[3]], NumberStyles.Float, CultureInfo.InvariantCulture, out product.Weight) ||
                !IsWeight(product.Weight)) throw new FormatException($"第 {i + 1} 列 ID、等級須為整數；權重須為非負有限數值。");
            bool valid = product.Type == ShopProductType.Weapon
                ? product.ItemID > 0 && product.ItemID < WeaponDataTable.WeaponCount && product.Level == 0
                : SkillLevelTable.ActiveIds.Contains(product.ItemID) && product.Level >= 1 && product.Level <= SkillLevelTable.MaxLevel;
            if (!valid) throw new FormatException($"第 {i + 1} 列商品無效。裝備不可為空手，Level 填 0；技能須為已實作的 ID 與等級。");
            if (!keys.Add(type + "/" + product.ItemID + "/" + product.Level))
                throw new FormatException($"第 {i + 1} 列商品重複。");
            result.Products.Add(product);
        }
        return result;
    }

    public string ToCsv()
    {
        var text = new StringBuilder(string.Join(",", Headers) + "\n");
        foreach (var p in Products)
            text.Append(p.Type).Append(',').Append(p.ItemID).Append(',').Append(p.Level).Append(',')
                .Append(p.Weight.ToString("R", CultureInfo.InvariantCulture)).Append('\n');
        string csv = text.ToString();
        Parse(csv);
        return csv;
    }

    public static bool IsWeight(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;

    public List<ShopProduct> Sample(ShopProductType type, IList<double> groupWeights,
        Func<int, int> weaponRarity, Func<double> random, int count = 3)
    {
        int groups = type == ShopProductType.Weapon ? 4 : SkillLevelTable.MaxLevel;
        if (groupWeights == null || groupWeights.Count != groups || groupWeights.Any(v => !IsWeight(v)))
            throw new FormatException("商人各稀有度／等級機率必須為非負有限數值。");
        var remaining = Products.Where(p => p.Type == type && p.Weight > 0).ToList();
        var selected = new List<ShopProduct>();
        while (selected.Count < count && remaining.Count > 0)
        {
            var buckets = new List<ShopProduct>[groups];
            for (int g = 0; g < groups; g++) buckets[g] = new List<ShopProduct>();
            foreach (var product in remaining)
            {
                int group = type == ShopProductType.Weapon ? weaponRarity(product.ItemID) : product.Level - 1;
                if (group >= 0 && group < groups) buckets[group].Add(product);
            }
            var available = Enumerable.Range(0, groups).Where(g => buckets[g].Count > 0 && groupWeights[g] > 0).ToList();
            if (available.Count == 0) break;
            int chosenGroup = Pick(available, g => groupWeights[g], random);
            var chosen = Pick(buckets[chosenGroup], p => p.Weight, random);
            selected.Add(chosen);
            remaining.Remove(chosen); // Product identity includes skill level.
        }
        return selected;
    }

    private static T Pick<T>(IList<T> values, Func<T, double> weight, Func<double> random)
    {
        // Scale before summing to avoid overflow with large spreadsheet weights.
        double max = values.Max(weight), total = values.Sum(v => weight(v) / max);
        double roll = random();
        if (double.IsNaN(roll) || roll < 0 || roll >= 1) throw new ArgumentOutOfRangeException(nameof(random));
        double threshold = roll * total;
        foreach (T value in values)
        {
            threshold -= weight(value) / max;
            if (threshold < 0) return value;
        }
        return values[values.Count - 1];
    }
}
