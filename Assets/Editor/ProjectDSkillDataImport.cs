using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public static class ProjectDSkillDataImport
{
    private const string AssetPath = "Assets/Resources/SkillLevels.csv";
    private const string CloudUrl = "https://docs.google.com/spreadsheets/d/1e7nJ_tqyU6XNznNDEyZnv8UNE3BMQmh_NYpselu4AZc/export?format=csv&gid=0";
    private static bool importing;

    [MenuItem("Tools/ProjectD/技能數值/從雲端下載並匯入")]
    public static async void Download()
    {
        if (importing || EditorApplication.isPlayingOrWillChangePlaymode) return;
        importing = true;
        try
        {
            using (var request = UnityWebRequest.Get(CloudUrl))
            {
                request.timeout = 30;
                var operation = request.SendWebRequest();
                while (!operation.isDone) await Task.Delay(100);
                if (request.result != UnityWebRequest.Result.Success)
                    throw new IOException("下載失敗：" + request.error);
                Import(new UTF8Encoding(false, true).GetString(request.downloadHandler.data));
            }
        }
        catch (Exception error)
        {
            Debug.LogError(error);
            EditorUtility.DisplayDialog("技能表未匯入", error.Message + "\n若表格需要登入，請在瀏覽器下載 Skill 分頁的 CSV，再使用「從 CSV 匯入」。原資料保持不變。", "知道了");
        }
        finally { importing = false; }
    }

    [MenuItem("Tools/ProjectD/技能數值/從 CSV 匯入")]
    public static void ImportFile()
    {
        if (importing || EditorApplication.isPlayingOrWillChangePlaymode) return;
        string path = EditorUtility.OpenFilePanel("選擇 Skill 分頁下載的 CSV", "", "csv");
        if (string.IsNullOrEmpty(path)) return;
        try { Import(File.ReadAllText(path, new UTF8Encoding(false, true))); }
        catch (Exception error) { EditorUtility.DisplayDialog("技能表未匯入", error.Message, "知道了"); }
    }

    // Validate everything before replacing the build's local snapshot. No network at runtime.
    private static void Import(string csv)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("請先停止遊戲。");
        SkillLevelTable table = SkillLevelTable.Parse(csv);
        string destination = Path.GetFullPath(AssetPath);
        string project = Directory.GetParent(Application.dataPath).FullName;
        string backup = Path.Combine(Directory.GetParent(project).FullName, "ProjectD AI", "Skill Data Backups", DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));
        Directory.CreateDirectory(backup);
        if (File.Exists(destination)) File.Copy(destination, Path.Combine(backup, "SkillLevels.before.csv"));
        File.WriteAllText(Path.Combine(backup, "SkillLevels.incoming.csv"), csv, new UTF8Encoding(false));
        byte[] previous = File.Exists(destination) ? File.ReadAllBytes(destination) : null;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.WriteAllText(destination, csv, new UTF8Encoding(false));
            AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);
        }
        catch
        {
            if (previous != null) File.WriteAllBytes(destination, previous);
            else if (File.Exists(destination)) File.Delete(destination);
            AssetDatabase.Refresh();
            throw;
        }
        Debug.Log($"技能表已匯入：{table.Count} 列。下次開始遊戲生效。備份：{backup}");
        EditorUtility.DisplayDialog("技能表已匯入", $"已更新 {table.Count} 列。下次開始遊戲生效。\n升級開放裝備欄位維持原規則。", "完成");
    }
}
