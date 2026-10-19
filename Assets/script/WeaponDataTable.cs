using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

// Shared by the editor importer and the scene's CSVReader.
public sealed class WeaponDataTable
{
    public const int WeaponCount = 30;
    public static readonly string[] Headers = { "ID", "Rarity", "Price", "Name", "Damage", "Cooldown", "Size", "Speed", "Cost", "Crit", "CritDamage", "HomingRange", "HomingTurnSpeed" };
    private readonly WeaponBase[] weapons = new WeaponBase[WeaponCount];
    public int Count => weapons.Length;

    public WeaponBase[] CreateWeapons()
    {
        return weapons.Select(w => new WeaponBase(w.ID, w.Rarity, w.Price, w.Name, w.Damage,
            w.Cooldown, w.Size, w.Speed, w.Cost, w.Crit, w.CritDmg, w.HomingRange, w.HomingTurnSpeed)).ToArray();
    }

    public static WeaponDataTable Parse(string csv)
    {
        var records = ParseCsv(csv);
        if (records.Count < 2 || records[0].Length != Headers.Length ||
            !new HashSet<string>(records[0], StringComparer.Ordinal).SetEquals(Headers))
            throw new FormatException("裝備表欄位不符，請下載 Weapon 分頁的 CSV，保留原有英文表頭。");
        int[] columns = Headers.Select(h => Array.IndexOf(records[0], h)).ToArray();
        var table = new WeaponDataTable();
        for (int index = 1; index < records.Count; index++)
        {
            string[] row = records[index];
            if (row.Length != Headers.Length) throw new FormatException($"第 {index + 1} 列欄位數量不符，數值欄不可漏填。");
            int id = Integer(row[columns[0]], index + 1, "ID");
            if (id < 0 || id >= WeaponCount)
                throw new FormatException($"第 {index + 1} 列 ID 超出目前支援範圍 0～{WeaponCount - 1}；新裝備需先實作對應圖示與效果。");
            if (table.weapons[id] != null) throw new FormatException($"重複裝備 ID：{id}。");
            string rarityText = row[columns[1]].Trim();
            if (!Enum.GetNames(typeof(RarityType)).Contains(rarityText))
                throw new FormatException($"第 {index + 1} 列 Rarity 必須是 Normal、Magic、Rare 或 Unique。");
            var rarity = (RarityType)Enum.Parse(typeof(RarityType), rarityText);
            int price = Integer(row[columns[2]], index + 1, "Price");
            if (price < 0) throw new FormatException($"第 {index + 1} 列 Price 不可為負數。");
            string name = row[columns[3]].Trim();
            if (string.IsNullOrWhiteSpace(name)) throw new FormatException($"第 {index + 1} 列 Name 不可留空。");
            var values = new float[9];
            for (int j = 0; j < values.Length; j++)
            {
                string number = row[columns[j + 4]].Trim();
                // Additive stat ratios can be negative; homing fields are absolute values.
                bool percent = j < 7 && number.EndsWith("%", StringComparison.Ordinal);
                if (percent) number = number.Substring(0, number.Length - 1).Trim();
                if (!float.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out values[j]) ||
                    float.IsNaN(values[j]) || float.IsInfinity(values[j]))
                    throw new FormatException($"第 {index + 1} 列 {Headers[j + 4]} 必須填寫有限數值。");
                if (percent) values[j] /= 100f;
            }
            if (values[7] < 0 || values[8] < 0)
                throw new FormatException($"第 {index + 1} 列追蹤範圍與轉向速度不可為負數。");
            if (id == 0 && (rarity != RarityType.Normal || price != 0 || values.Any(v => v != 0f)))
                throw new FormatException("ID 0 是空裝備，必須是 Normal、價格 0、全部數值 0。");
            table.weapons[id] = new WeaponBase(id, rarity, price, name, values[0], values[1], values[2],
                values[3], values[4], values[5], values[6], values[7], values[8]);
        }
        for (int id = 0; id < WeaponCount; id++)
            if (table.weapons[id] == null) throw new FormatException($"缺少裝備 ID {id}，已阻止不完整資料覆蓋本機。");
        return table;
    }

    private static int Integer(string value, int row, string field)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            throw new FormatException($"第 {row} 列 {field} 必須是整數。");
        return result;
    }

    private static List<string[]> ParseCsv(string csv)
    {
        csv = (csv ?? "").TrimStart('\uFEFF').Replace("\r\n", "\n").Replace('\r', '\n');
        var records = new List<string[]>(); var row = new List<string>(); var cell = new StringBuilder();
        bool quoted = false, closed = false;
        for (int i = 0; i <= csv.Length; i++)
        {
            char c = i == csv.Length ? '\n' : csv[i];
            if (quoted)
            {
                if (i == csv.Length) throw new FormatException("CSV 有未關閉的雙引號。");
                if (c == '"')
                {
                    if (i + 1 < csv.Length && csv[i + 1] == '"') { cell.Append('"'); i++; }
                    else { quoted = false; closed = true; }
                }
                else cell.Append(c);
            }
            else if (c == ',' || c == '\n')
            {
                row.Add(cell.ToString()); cell.Clear(); closed = false;
                if (c == '\n') { if (row.Any(v => !string.IsNullOrWhiteSpace(v))) records.Add(row.ToArray()); row.Clear(); }
            }
            else if (c == '"' && cell.Length == 0 && !closed) quoted = true;
            else if (c == '"' || closed) throw new FormatException("CSV 引號格式錯誤。");
            else cell.Append(c);
        }
        return records;
    }
}
