using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjectD.EditorTools
{
    public sealed class LocalizationCsvReport
    {
        public readonly List<string[]> Rows = new List<string[]>();
        public readonly List<string> Errors = new List<string>();
        public readonly List<string> Warnings = new List<string>();
        public readonly List<string> Added = new List<string>();
        public readonly List<string> Changed = new List<string>();
        public readonly List<string> Removed = new List<string>();
        public readonly Dictionary<string, List<string>> Missing = new Dictionary<string, List<string>>();
        public bool CanImport { get { return Errors.Count == 0; } }
        public string Summary()
        {
            var text = new StringBuilder();
            text.AppendLine($"有效詞條：{Math.Max(0, Rows.Count - 1)}　新增：{Added.Count}　修改：{Changed.Count}　刪除：{Removed.Count}");
            foreach (var language in Missing)
                text.AppendLine($"缺少翻譯 — {language.Key}：{language.Value.Count}");
            Append(text, "錯誤（必須修正）", Errors);
            Append(text, "提醒", Warnings);
            Append(text, "新增 Key", Added);
            Append(text, "修改 Key", Changed);
            Append(text, "刪除 Key", Removed);
            foreach (var language in Missing)
                Append(text, "缺翻譯 Key — " + language.Key, language.Value);
            return text.ToString();
        }
        private static void Append(StringBuilder text, string title, List<string> items)
        {
            if (items.Count == 0) return;
            text.AppendLine("\n" + title);
            foreach (string item in items) text.AppendLine("• " + item);
        }
    }

    // Pure validation: never mutates I2 assets or writes files.
    public static class ProjectDLocalizationCsv
    {
        public static LocalizationCsvReport Analyze(string csv, string[] languages, IEnumerable<string[]> existing)
        {
            var report = new LocalizationCsvReport();
            List<string[]> rows;
            try { rows = Parse(csv); }
            catch (FormatException error) { report.Errors.Add(error.Message); return report; }
            if (rows.Count < 2) { report.Errors.Add("CSV 沒有詞條，已阻止清空 I2 資料。"); return report; }
            string[] header = rows[0];
            if (header.Length != languages.Length + 3 || header[0] != "Key" || header[1] != "Type" || header[2] != "Desc" ||
                !new HashSet<string>(header.Skip(3), StringComparer.Ordinal).SetEquals(languages) ||
                header.Skip(3).Distinct(StringComparer.Ordinal).Count() != languages.Length)
            {
                report.Errors.Add("欄位必須為 Key、Type、Desc，以及 I2 目前的語言：" + string.Join("、", languages) + "。進度等額外欄位請放到其他工作表。");
                return report;
            }
            report.Rows.Add(header);
            foreach (string language in languages) report.Missing.Add(language, new List<string>());
            var old = existing.ToDictionary(r => r[0], r => r, StringComparer.Ordinal);
            var keys = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 1; index < rows.Count; index++)
            {
                string[] row = rows[index];
                if (row.Length > header.Length) { report.Errors.Add($"第 {index + 1} 筆資料的欄位過多。"); continue; }
                Array.Resize(ref row, header.Length);
                for (int i = 0; i < row.Length; i++) row[i] = row[i] ?? "";
                string key = row[0];
                string normalized = key.Replace('\\', '/');
                int firstSlash = normalized.IndexOf('/');
                if (firstSlash >= 0) normalized = normalized.Substring(0, firstSlash + 1) + normalized.Substring(firstSlash + 1).Replace("/", "");
                if (string.IsNullOrWhiteSpace(key) || key != key.Trim() || key != normalized || key.EndsWith("]", StringComparison.Ordinal))
                { report.Errors.Add($"第 {index + 1} 筆 Key 無效或含不支援的特化格式：{key}"); continue; }
                if (!keys.Add(key)) { report.Errors.Add("重複 Key：" + key); continue; }
                if (!string.Equals(row[1], "Text", StringComparison.OrdinalIgnoreCase))
                { report.Errors.Add("此工具只匯入文字詞條，請以 I2 原工具處理其他類型：" + key); continue; }
                row[1] = "Text";
                var canonical = new string[languages.Length + 3];
                canonical[0] = key; canonical[1] = row[1]; canonical[2] = row[2];
                for (int i = 0; i < languages.Length; i++)
                {
                    string value = row[Array.IndexOf(header, languages[i])];
                    canonical[i + 3] = value == "-" ? "" : value;
                    if (string.IsNullOrWhiteSpace(value) || value == "-") report.Missing[languages[i]].Add(key);
                    if (Regex.IsMatch(value, @"^#(REF!|VALUE!|N/A|ERROR!|LOADING|NAME\?|DIV/0!)", RegexOptions.IgnoreCase))
                        report.Errors.Add($"試算表公式尚未成功計算：{key} / {languages[i]}");
                }
                string[] previous;
                if (!old.TryGetValue(key, out previous)) report.Added.Add(key);
                else if (!canonical.SequenceEqual(previous)) report.Changed.Add(key);
                CheckPlaceholders(key, canonical.Skip(3).ToArray(), languages, report);
                report.Rows.Add(row);
            }
            report.Removed.AddRange(old.Keys.Where(key => !keys.Contains(key)).OrderBy(key => key, StringComparer.Ordinal));
            if (!keys.Contains("NPC/Item Passivetree Name"))
                report.Errors.Add("缺少遊戲使用的 Key：NPC/Item Passivetree Name，請下載最新的線上主表。");
            if (report.Missing.Values.Any(list => list.Count > 0)) report.Warnings.Add("缺翻譯只提醒，不會阻止匯入；可保留開發中或暫不使用的內容。");
            return report;
        }

        private static void CheckPlaceholders(string key, string[] values, string[] languages, LocalizationCsvReport report)
        {
            string baseline = null;
            for (int i = 0; i < values.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(values[i])) continue;
                string tokens = string.Join(",", Regex.Matches(values[i], @"(?<!\{)\{(\d+)(?:[^{}]*)\}(?!\})")
                    .Cast<Match>().Select(m => m.Groups[1].Value).Distinct().OrderBy(x => x, StringComparer.Ordinal));
                if (baseline == null) baseline = tokens;
                else if (baseline != tokens) report.Errors.Add($"替換參數不一致：{key} / {languages[i]}，請核對 {{0}} 等參數。");
            }
        }

        public static List<string[]> Parse(string csv)
        {
            csv = (csv ?? "").TrimStart('\uFEFF').Replace("\r\n", "\n").Replace('\r', '\n');
            var rows = new List<string[]>(); var row = new List<string>(); var cell = new StringBuilder();
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
                    if (c == '\n')
                    {
                        if (row.Any(v => !string.IsNullOrWhiteSpace(v))) rows.Add(row.ToArray());
                        row.Clear();
                    }
                }
                else if (c == '"' && cell.Length == 0 && !closed) quoted = true;
                else if (c == '"' || closed) throw new FormatException("CSV 的雙引號或分隔格式不正確。");
                else cell.Append(c);
            }
            return rows;
        }
    }
}
