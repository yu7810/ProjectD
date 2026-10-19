using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using I2.Loc;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectD.EditorTools
{
    public sealed class ProjectDLocalizationWindow : EditorWindow
    {
        private const string CsvPath = "Assets/Localization.csv";
        private const string SourcePath = "Assets/Resources/I2Languages.asset";
        private const string CloudUrl = "https://docs.google.com/spreadsheets/d/1C_ORywkclOb4Rg58aaSsfy7dnbQCERr83kPZnofz8lo/export?format=csv&gid=1066662696";
        private static bool downloading;
        private byte[] pendingCloudCsv;
        private byte[] localCsvAtCloudPreview;
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

        [MenuItem("Tools/ProjectD/多語系/從雲端下載並匯入")]
        public static async void DownloadFromCloud()
        {
            if (!CanDownloadFromCloud()) return;
            var window = GetWindow<ProjectDLocalizationWindow>("多語系 CSV 匯入");
            await window.DownloadAndImport();
        }

        [MenuItem("Tools/ProjectD/多語系/從雲端下載並匯入", true)]
        private static bool CanDownloadFromCloud()
        {
            return !downloading && !EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling;
        }

        private async Task DownloadAndImport()
        {
            downloading = true;
            pendingCloudCsv = null;
            try
            {
                // Cloud sync always targets the project's canonical CSV and runtime I2 source.
                csv = AssetDatabase.LoadAssetAtPath<TextAsset>(CsvPath);
                targetSource = AssetDatabase.LoadAssetAtPath<LanguageSourceAsset>(SourcePath);
                if (csv == null || targetSource == null)
                    throw new InvalidOperationException("找不到本機 Localization.csv 或 I2Languages.asset。");
                byte[] beforeCsv = ReadCsv();
                string beforeSource = JsonUtility.ToJson(targetSource.mSource);
                Repaint();
                using (var request = UnityWebRequest.Get(CloudUrl))
                {
                    request.timeout = 30;
                    var operation = request.SendWebRequest();
                    while (!operation.isDone) await Task.Delay(100);
                    if (this == null) return;
                    if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                        throw new InvalidOperationException("下載期間進入 Play Mode 或開始編譯，已取消匯入，請稍後重試。");
                    if (request.result != UnityWebRequest.Result.Success)
                        throw new IOException($"無法下載多語系主表（HTTP {request.responseCode}）：{request.error}\n主表可能需要 Google 登入；Unity 不會沿用瀏覽器的登入狀態。");
                    byte[] downloaded = request.downloadHandler.data;
                    // Decode before staging, so invalid UTF-8 never reaches disk.
                    new UTF8Encoding(false, true).GetString(downloaded);
                    localCsvAtCloudPreview = ReadCsv();
                    bool changed = !beforeCsv.SequenceEqual(localCsvAtCloudPreview) || beforeSource != JsonUtility.ToJson(targetSource.mSource);
                    pendingCloudCsv = downloaded;
                    Inspect();
                    if (!report.CanImport)
                    {
                        EditorUtility.DisplayDialog("雲端資料未匯入", "下載內容未通過多語系檢查。請查看視窗中的錯誤；若下載到登入頁面，請先在瀏覽器下載 CSV。原 CSV 與 I2 保持不變。", "知道了");
                        return;
                    }
                    if (changed)
                    {
                        EditorUtility.DisplayDialog("本機資料已變動", "下載期間 CSV 或 I2 有變更，已產生新的差異摘要。請確認後再按匯入，尚未修改本機檔案。", "知道了");
                        return;
                    }
                    Import();
                }
            }
            catch (Exception error)
            {
                if (this == null) return;
                pendingCloudCsv = null;
                report = null;
                reportText = "雲端下載未完成：" + error.Message;
                Debug.LogWarning(reportText);
                EditorUtility.DisplayDialog("多語系未匯入", error.Message + "\n可在瀏覽器下載 Localization 分頁的 CSV，再使用原有「檢查與匯入 CSV」工具。", "知道了");
            }
            finally
            {
                downloading = false;
                if (this != null) Repaint();
            }
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
            EditorGUILayout.HelpBox("可從雲端下載並匯入，或手動替換本機 CSV 後檢查。成功匯入會一起更新 CSV 與 I2 資源，並備份原資料。", MessageType.Info);
            using (new EditorGUI.DisabledScope(downloading))
            {
                EditorGUI.BeginChangeCheck();
                csv = (TextAsset)EditorGUILayout.ObjectField("翻譯 CSV", csv, typeof(TextAsset), false);
                targetSource = (LanguageSourceAsset)EditorGUILayout.ObjectField("I2 語言資源", targetSource, typeof(LanguageSourceAsset), false);
                if (EditorGUI.EndChangeCheck()) { pendingCloudCsv = null; report = null; reportText = ""; }
            }
            using (new EditorGUI.DisabledScope(downloading || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling))
            {
                if (GUILayout.Button("從雲端下載並匯入")) DownloadFromCloud();
                if (GUILayout.Button("檢查本機 CSV")) { pendingCloudCsv = null; Inspect(); }
                using (new EditorGUI.DisabledScope(report == null || !report.CanImport))
                    if (GUILayout.Button("依上方來源匯入並儲存 I2")) Import();
            }
            if (downloading) EditorGUILayout.HelpBox("正在下載多語系主表……", MessageType.Info);
            if (pendingCloudCsv != null)
                EditorGUILayout.HelpBox("目前預覽雲端下載內容，尚未寫入本機。確認匯入後才會更新 CSV 與 I2；可按「檢查本機 CSV」捨棄這份預覽。", MessageType.Info);
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
            if (pendingCloudCsv != null) return pendingCloudCsv;
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
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (pendingCloudCsv != null)
            {
                string localPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, AssetDatabase.GetAssetPath(csv));
                if (localCsvAtCloudPreview == null || !localCsvAtCloudPreview.SequenceEqual(File.ReadAllBytes(localPath)))
                {
                    pendingCloudCsv = null;
                    Inspect();
                    EditorUtility.DisplayDialog("本機 CSV 已變動", "雲端預覽後本機 CSV 有變更，已取消這次雲端匯入並重新檢查本機資料。請確認後重新下載。", "知道了");
                    return;
                }
            }
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
            string csvAssetPath = AssetDatabase.GetAssetPath(csv);
            string csvFilePath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, csvAssetPath);
            byte[] beforeCsv = File.ReadAllBytes(csvFilePath);
            byte[] cloudCsv = pendingCloudCsv;
            bool modified = false;
            bool csvModified = false;
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
                if (cloudCsv != null)
                {
                    csvModified = true;
                    File.WriteAllBytes(csvFilePath, cloudCsv);
                    AssetDatabase.ImportAsset(csvAssetPath, ImportAssetOptions.ForceUpdate);
                }
                EditorUtility.SetDirty(targetSource);
                AssetDatabase.SaveAssetIfDirty(targetSource);
                LocalizationManager.LocalizeAll(true);
                Debug.Log("多語系匯入完成：" + report.Rows.Count + " 列（含標題）。備份：" + backup, targetSource);
                pendingCloudCsv = null;
                csv = AssetDatabase.LoadAssetAtPath<TextAsset>(csvAssetPath);
                Inspect();
                ShowNotification(new GUIContent("翻譯已匯入並儲存"));
            }
            catch (Exception error)
            {
                if (csvModified)
                {
                    File.WriteAllBytes(csvFilePath, beforeCsv);
                    AssetDatabase.ImportAsset(csvAssetPath, ImportAssetOptions.ForceUpdate);
                }
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
            string localCsv = Path.Combine(project, AssetDatabase.GetAssetPath(csv));
            File.Copy(localCsv, Path.Combine(folder, "Localization.before.csv"));
            File.WriteAllText(Path.Combine(folder, "Import report.txt"), reportText, new UTF8Encoding(false));
            return folder;
        }
    }
}
