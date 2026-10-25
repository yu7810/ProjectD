using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

// Shared by the editor importer and the scene's CSVReader.
public sealed class WeaponDataTable
{
    public const int WeaponCount = ValueData.CrimsonGrimoireWeaponId + 1;
    public static readonly string[] Headers = { "ID", "Rarity", "Price", "Name", "Damage", "Cooldown", "Size", "Speed", "Cost", "Crit", "CritDamage", "Special1Purpose", "Special1Value", "Special2Purpose", "Special2Value", "Special3Purpose", "Special3Value" };
    private readonly WeaponBase[] weapons = new WeaponBase[WeaponCount];
    public int Count => weapons.Length;

    public WeaponBase[] CreateWeapons()
    {
        return weapons.Select(w => new WeaponBase(w.ID, w.Rarity, w.Price, w.Name, w.Damage,
            w.Cooldown, w.Size, w.Speed, w.Cost, w.Crit, w.CritDmg, w.HomingRange, w.HomingTurnSpeed, w.RageDamagePerPoint)).ToArray();
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
            if (row.Length > Headers.Length) throw new FormatException($"第 {index + 1} 列欄位數量不符，數值欄不可漏填。");
            Array.Resize(ref row, Headers.Length);
            row = row.Select(v => v ?? string.Empty).ToArray();
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
            var values = new float[7];
            for (int j = 0; j < values.Length; j++)
            {
                string number = row[columns[j + 4]].Trim();
                // Additive stat ratios can be negative and accept percentage notation.
                bool percent = j < 7 && number.EndsWith("%", StringComparison.Ordinal);
                if (percent) number = number.Substring(0, number.Length - 1).Trim();
                if (!float.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out values[j]) ||
                    float.IsNaN(values[j]) || float.IsInfinity(values[j]))
                    throw new FormatException($"第 {index + 1} 列 {Headers[j + 4]} 必須填寫有限數值。");
                if (percent) values[j] /= 100f;
            }
            float[] special = ParseSpecialValues(row, columns, id, index + 1);
            if (id == 0 && (rarity != RarityType.Normal || price != 0 || values.Any(v => v != 0f)))
                throw new FormatException("ID 0 是空裝備，必須是 Normal、價格 0、全部數值 0。");
            table.weapons[id] = new WeaponBase(id, rarity, price, name, values[0], values[1], values[2],
                values[3], values[4], values[5], values[6], special[0], special[1], special[2]);
        }
        for (int id = 0; id < WeaponCount; id++)
            if (table.weapons[id] == null) throw new FormatException($"缺少裝備 ID {id}，已阻止不完整資料覆蓋本機。");
        return table;
    }

    // Purpose labels identify parameters; their order among the three slots is editable.
    public static string SpecialPurpose(int weaponId, int parameter)
    {
        if (weaponId == ValueData.TrackingWeaponId && parameter == 1) return "追蹤範圍";
        if (weaponId == ValueData.TrackingWeaponId && parameter == 2) return "每秒轉向角度";
        if (weaponId == ValueData.SunbreakerWeaponId && parameter == 3) return "每點盛怒傷害加成";
        return string.Empty;
    }

    private static float[] ParseSpecialValues(string[] row, int[] columns, int id, int line)
    {
        var result = new float[3];
        var expected = Enumerable.Range(1, 3).Select(p => SpecialPurpose(id, p)).ToArray();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (int slot = 0; slot < 3; slot++)
        {
            string purpose = row[columns[11 + slot * 2]].Trim();
            string number = row[columns[12 + slot * 2]].Trim();
            if (purpose.Length == 0)
            {
                if (number.Length != 0) throw new FormatException($"第 {line} 列特殊欄位 {slot + 1} 有數值但未填用途。");
                continue;
            }
            int parameter = Array.IndexOf(expected, purpose);
            if (parameter < 0) throw new FormatException($"第 {line} 列裝備 {id} 不支援特殊用途：{purpose}。");
            if (!seen.Add(purpose)) throw new FormatException($"第 {line} 列特殊用途重複：{purpose}。");
            bool percent = parameter == 2 && number.EndsWith("%", StringComparison.Ordinal);
            if (percent) number = number.Substring(0, number.Length - 1).Trim();
            if (!float.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out float value)
                || float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new FormatException($"第 {line} 列 {purpose} 必須填寫非負有限數值。");
            result[parameter] = percent ? value / 100f : value;
        }
        foreach (string purpose in expected.Where(p => p.Length > 0))
            if (!seen.Contains(purpose)) throw new FormatException($"第 {line} 列缺少必要的特殊用途：{purpose}。");
        return result;
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
