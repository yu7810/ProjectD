using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

// Independent of Unity so the same validator is used by the editor and builds.
public sealed class SkillLevelStats
{
    public int ID, Level, Price;
    public string Name;
    public float Damage, Cooldown, Size, Speed, Cost, Crit, CritDmg;
    public float WaterballManaRatio, WaterballDamagePerMana, ChargeDamagePerMeter;
    public float SunfireLifeRatio, SunfireInterval;
}

public sealed class SkillLevelTable
{
    public const int MaxLevel = 3;
    public static readonly int[] ActiveIds = { 1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
    public static readonly string[] Headers = { "SkillID", "Name", "Level", "Price", "Damage", "Cooldown", "Size", "Speed", "Cost", "Crit", "CritDmg", "Special1Purpose", "Special1Value", "Special2Purpose", "Special2Value", "Special3Purpose", "Special3Value" };

    // Slot meanings are defined by the implemented skill, never by editable spreadsheet text.
    public static string SpecialPurpose(int skillId, int slot)
    {
        if (skillId == 10 && slot == 1) return "當前魔力消耗比例";
        if (skillId == 10 && slot == 2) return "每點魔力傷害";
        if (skillId == 11 && slot == 1) return "每公尺追加傷害";
        if (skillId == 12 && slot == 1) return "最大生命傷害比例";
        if (skillId == 12 && slot == 2) return "傷害觸發間隔秒數";
        return string.Empty;
    }
    private readonly Dictionary<int, SkillLevelStats> rows = new Dictionary<int, SkillLevelStats>();
    public int Count => rows.Count;
    public SkillLevelStats Get(int id, int level)
    {
        rows.TryGetValue(id * 10 + Math.Max(1, Math.Min(MaxLevel, level)), out var result);
        return result;
    }

    public static SkillLevelTable Parse(string csv)
    {
        var records = ParseCsv(csv);
        if (records.Count == 0 || records[0].Length != Headers.Length ||
            !new HashSet<string>(records[0]).SetEquals(Headers))
            throw new FormatException("技能表欄位不符，請下載 Skill 分頁的 CSV，保留原有英文表頭。");
        var columns = Headers.Select(h => Array.IndexOf(records[0], h)).ToArray();
        var table = new SkillLevelTable();
        for (int i = 1; i < records.Count; i++)
        {
            var row = records[i];
            if (row.Length > Headers.Length) throw new FormatException($"第 {i + 1} 列欄位數量不符。");
            // CSV exporters may omit trailing empty special slots.
            Array.Resize(ref row, Headers.Length);
            int id = Integer(row[columns[0]]), level = Integer(row[columns[2]]);
            if (!ActiveIds.Contains(id) || level < 1 || level > MaxLevel)
                throw new FormatException($"第 {i + 1} 列技能 ID 或等級無效。");
            if (string.IsNullOrWhiteSpace(row[columns[1]])) throw new FormatException($"第 {i + 1} 列缺少名稱。");
            if (!int.TryParse(row[columns[3]], NumberStyles.Integer, CultureInfo.InvariantCulture, out int price) || price < 0)
                throw new FormatException($"第 {i + 1} 列 Price 必須填寫非負整數，不可留空。");
            var v = new float[7];
            for (int j = 0; j < v.Length; j++)
            {
                v[j] = Number(row[columns[j + 4]], j == 5, i + 1, Headers[j + 4]);
            }
            if (v[1] <= 0 || v[2] <= 0 || v[3] <= 0 || v[5] > 1 || v[6] < 1)
                throw new FormatException($"第 {i + 1} 列冷卻、大小、速度須大於零；暴擊率為 0～1，暴傷倍率至少為 1。");
            var special = new float[3];
            for (int slot = 1; slot <= 3; slot++)
            {
                string purpose = (row[columns[11 + (slot - 1) * 2]] ?? "").Trim();
                string value = row[columns[12 + (slot - 1) * 2]];
                string expected = SpecialPurpose(id, slot);
                if (expected.Length == 0)
                {
                    if (purpose.Length != 0 || !string.IsNullOrWhiteSpace(value))
                        throw new FormatException($"第 {i + 1} 列技能 {id} 未使用特殊欄位 {slot}，用途與數值都必須留空。");
                    continue;
                }
                if (purpose != expected)
                    throw new FormatException($"第 {i + 1} 列特殊欄位 {slot} 的用途必須是「{expected}」，不可修改用途或交換欄位。");
                bool ratio = (id == 10 || id == 12) && slot == 1;
                special[slot - 1] = Number(value, ratio, i + 1, $"特殊欄位 {slot}（{expected}）");
                if (id == 10 && ratio && special[slot - 1] > 1)
                    throw new FormatException($"第 {i + 1} 列當前魔力消耗比例必須為 0～1（例如 0.5 或 50%）。");
                if (id == 12 && slot == 2 && special[1] <= 0f)
                    throw new FormatException($"第 {i + 1} 列日炎傷害觸發間隔必須大於零。");
            }
            int key = id * 10 + level;
            if (table.rows.ContainsKey(key)) throw new FormatException($"重複的技能/等級：{id}/{level}。");
            table.rows.Add(key, new SkillLevelStats { ID = id, Level = level, Price = price, Name = row[columns[1]],
                Damage = v[0], Cooldown = v[1], Size = v[2], Speed = v[3], Cost = v[4], Crit = v[5], CritDmg = v[6],
                WaterballManaRatio = id == 10 ? special[0] : 0,
                WaterballDamagePerMana = id == 10 ? special[1] : 0,
                ChargeDamagePerMeter = id == 11 ? special[0] : 0,
                SunfireLifeRatio = id == 12 ? special[0] : 0,
                SunfireInterval = id == 12 ? special[1] : 0 });
        }
        foreach (int id in ActiveIds)
            for (int level = 1; level <= MaxLevel; level++)
                if (table.Get(id, level) == null) throw new FormatException($"缺少技能 {id} 的 {level} 級資料。");
        return table;
    }

    private static float Number(string text, bool allowPercent, int row, string field)
    {
        string number = (text ?? "").Trim();
        bool percent = allowPercent && number.EndsWith("%", StringComparison.Ordinal);
        if (percent) number = number.Substring(0, number.Length - 1).Trim();
        if (!float.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ||
            float.IsNaN(value) || float.IsInfinity(value) || value < 0)
            throw new FormatException($"第 {row} 列 {field} 必須填寫非負有限數值，不可留空。");
        return percent ? value / 100f : value;
    }

    private static int Integer(string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number))
            throw new FormatException("技能 ID 與等級必須是整數。");
        return number;
    }

    private static List<string[]> ParseCsv(string csv)
    {
        csv = (csv ?? "").TrimStart('\uFEFF').Replace("\r\n", "\n").Replace('\r', '\n');
        var result = new List<string[]>(); var row = new List<string>(); var cell = new StringBuilder();
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
                if (c == '\n') { if (row.Any(s => !string.IsNullOrWhiteSpace(s))) result.Add(row.ToArray()); row.Clear(); }
            }
            else if (c == '"' && cell.Length == 0 && !closed) quoted = true;
            else if (c == '"' || closed) throw new FormatException("CSV 引號格式錯誤。");
            else cell.Append(c);
        }
        return result;
    }
}
