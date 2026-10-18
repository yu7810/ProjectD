using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using I2.Loc;
using UnityEditor;
using UnityEngine;

namespace ProjectD.EditorTools
{
    public sealed class ProjectDLocalizationWindow : EditorWindow
    {
        private const string CsvPath = "Assets/Localization.csv";
        private const string SourcePath = "Assets/Resources/I2Languages.asset";
        private TextAsset csv;
        private LanguageSourceAsset targetSource;
        private LocalizationCsvReport report;
        private string csvHash, sourceSnapshot, reportText;
        private Vector2 scroll;

        [MenuItem("Tools/ProjectD/多語系/檢查與匯入 CSV")]
        public static void Open()
        {
            GetWindow<ProjectDLocalizationWindow>("多語系 CSV 匯入");
        }

        private void OnEnable()
        {
            minSize = new Vector2(580f, 400f);
            csv = AssetDatabase.LoadAssetAtPath<TextAsset>(CsvPath);
            targetSource = AssetDatabase.LoadAssetAtPath<LanguageSourceAsset>(SourcePath);
            Inspect();
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("先在線上主表編輯，下載 CSV 並替換本機檔案，再檢查與匯入。遊戲使用的是 I2 資源，替換 CSV 本身不會更新遊戲文字。", MessageType.Info);
            EditorGUI.BeginChangeCheck();
            csv = (TextAsset)EditorGUILayout.ObjectField("翻譯 CSV", csv, typeof(TextAsset), false);
            targetSource = (LanguageSourceAsset)EditorGUILayout.ObjectField("I2 語言資源", targetSource, typeof(LanguageSourceAsset), false);
            if (EditorGUI.EndChangeCheck()) { report = null; reportText = ""; }
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling))
            {
                if (GUILayout.Button("檢查本機 CSV")) Inspect();
                using (new EditorGUI.DisabledScope(report == null || !report.CanImport))
                    if (GUILayout.Button("依上方來源匯入並儲存 I2")) Import();
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                EditorGUILayout.HelpBox("請先退出 Play Mode，再匯入翻譯。", MessageType.Info);
            if (report != null)
                EditorGUILayout.HelpBox(report.CanImport ? "檢查通過，可匯入。缺翻譯已列在下方供你確認。" : "有格式錯誤，已阻止匯入。", report.CanImport ? MessageType.Info : MessageType.Error);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.TextArea(reportText ?? "請選擇來源後執行檢查。", GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private byte[] ReadCsv()
        {
            string project = Directory.GetParent(Application.dataPath).FullName;
            return File.ReadAllBytes(Path.Combine(project, AssetDatabase.GetAssetPath(csv)));
        }

        private void Inspect()
        {
            report = null;
            try
            {
                if (csv == null || targetSource == null) throw new InvalidOperationException("找不到 CSV 或 I2 語言資源，請先選擇來源。");
                byte[] bytes = ReadCsv();
                string content = new UTF8Encoding(false, true).GetString(bytes);
                LanguageSourceData source = targetSource.mSource;
                string[] languages = source.mLanguages.Select(l => l.Name).ToArray();
                if (languages.Length == 0) throw new InvalidOperationException("I2 沒有設定語言，無法安全比對。");
                if (source.CaseInsensitiveTerms) throw new InvalidOperationException("此專案工具要求 I2 Key 區分大小寫，請改用 I2 原生匯入處理不區分大小寫的來源。");
                IEnumerable<string[]> existing = source.mTerms.Select(term => new[] { term.Term, term.TermType.ToString(), term.Description ?? "" }
                    .Concat(languages.Select((language, i) => i < term.Languages.Length ? term.Languages[i] ?? "" : "")).ToArray());
                report = ProjectDLocalizationCsv.Analyze(content, languages, existing);
                sourceSnapshot = JsonUtility.ToJson(source);
                using (SHA256 hash = SHA256.Create()) csvHash = Convert.ToBase64String(hash.ComputeHash(bytes));
                reportText = report.Summary();
            }
            catch (Exception error)
            {
                report = new LocalizationCsvReport();
                report.Errors.Add(error.Message);
                reportText = report.Summary();
            }
        }

        private void Import()
        {
            string approvedCsv = csvHash, approvedSource = sourceSnapshot;
            Inspect(); // Re-read disk, not the potentially stale TextAsset cache.
            if (!report.CanImport) return;
            if (approvedCsv != csvHash || approvedSource != sourceSnapshot)
            {
                EditorUtility.DisplayDialog("資料已變動", "CSV 或 I2 資源在預覽後有變更，已重新產生摘要。請確認後再匯入。", "知道了");
                return;
            }
            if (report.Removed.Count > 0 && !EditorUtility.DisplayDialog("確認刪除翻譯 Key",
                $"這份 CSV 會刪除 {report.Removed.Count} 個現有 Key。請確認下方刪除清單。匯入前會備份原資源。", "確認匯入", "取消")) return;

            LanguageSourceData original = targetSource.mSource;
            string before = JsonUtility.ToJson(original);
            bool modified = false;
            try
            {
                // Validate I2's own import on a detached source before touching the real asset.
                LanguageSourceData candidate = new LanguageSourceData();
                JsonUtility.FromJsonOverwrite(before, candidate);
                string error = candidate.Import_CSV("", report.Rows, eSpreadsheetUpdateMode.Replace);
                if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
                foreach (LanguageData language in candidate.mLanguages)
                {
                    LanguageData previous = original.mLanguages.Find(l => l.Name == language.Name);
                    if (previous != null) { language.Code = previous.Code; language.Flags = previous.Flags; }
                }
                string backup = Backup();
                Undo.RecordObject(targetSource, "Import ProjectD localization CSV");
                modified = true;
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(candidate), original);
                original.owner = targetSource;
                original.UpdateDictionary(true);
                EditorUtility.SetDirty(targetSource);
                AssetDatabase.SaveAssetIfDirty(targetSource);
                LocalizationManager.LocalizeAll(true);
                Debug.Log("多語系匯入完成：" + report.Rows.Count + " 列（含標題）。備份：" + backup, targetSource);
                Inspect();
                ShowNotification(new GUIContent("翻譯已匯入並儲存"));
            }
            catch (Exception error)
            {
                if (modified)
                {
                    JsonUtility.FromJsonOverwrite(before, original);
                    original.UpdateDictionary(true);
                    EditorUtility.SetDirty(targetSource);
                    AssetDatabase.SaveAssetIfDirty(targetSource);
                }
                report.Errors.Add("匯入未完成，已還原原資料：" + error.Message);
                reportText = report.Summary();
                Debug.LogException(error);
            }
        }

        private string Backup()
        {
            string project = Directory.GetParent(Application.dataPath).FullName;
            string folder = Path.Combine(Directory.GetParent(project).FullName, "ProjectD AI", "Localization Backups", DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(folder);
            File.Copy(Path.Combine(project, AssetDatabase.GetAssetPath(targetSource)), Path.Combine(folder, "I2Languages.before.asset"));
            File.WriteAllText(Path.Combine(folder, "I2Source.before.json"), JsonUtility.ToJson(targetSource.mSource, true), new UTF8Encoding(false));
            File.WriteAllBytes(Path.Combine(folder, "Localization.incoming.csv"), ReadCsv());
            File.WriteAllText(Path.Combine(folder, "Import report.txt"), reportText, new UTF8Encoding(false));
            return folder;
        }
    }
}
