using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public static class ProjectDWeaponDataImport
{
    private const string AssetPath = "Assets/Weapon.csv";
    private const string CloudUrl = "https://docs.google.com/spreadsheets/d/1SAL2CVocqeLEqbBnQbQdgZf2z6yVQDmjuiCZGDXreuY/export?format=csv&gid=0";
    private static bool importing;
    private static string LocalPath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, AssetPath);
    private static bool CanImport => !importing && !EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling;

    [MenuItem("Tools/ProjectD/裝備數值/從雲端下載並匯入", true)]
    private static bool CanDownload() => CanImport;

    [MenuItem("Tools/ProjectD/裝備數值/從雲端下載並匯入")]
    public static async void Download()
    {
        if (!CanImport) return;
        importing = true;
        try
        {
            byte[] beforeDownload = ReadLocal();
            using (var request = UnityWebRequest.Get(CloudUrl))
            {
                request.timeout = 30;
                var operation = request.SendWebRequest();
                while (!operation.isDone) await Task.Delay(100);
                if (request.result != UnityWebRequest.Result.Success)
                    throw new IOException($"下載失敗（HTTP {request.responseCode}）：{request.error}");
                Import(new UTF8Encoding(false, true).GetString(request.downloadHandler.data), beforeDownload, true);
            }
        }
        catch (Exception error)
        {
            Debug.LogWarning("裝備表未匯入：" + error.Message);
            EditorUtility.DisplayDialog("裝備表未匯入", error.Message + "\n若表格需要登入，請在瀏覽器下載 Weapon 分頁的 CSV，再使用「從 CSV 匯入」。", "知道了");
        }
        finally { importing = false; }
    }

    [MenuItem("Tools/ProjectD/裝備數值/從 CSV 匯入", true)]
    private static bool CanImportFile() => CanImport;

    [MenuItem("Tools/ProjectD/裝備數值/從 CSV 匯入")]
    public static void ImportFile()
    {
        if (!CanImport) return;
        importing = true;
        try
        {
            string path = EditorUtility.OpenFilePanel("選擇 Weapon 分頁下載的 CSV", "", "csv");
            if (string.IsNullOrEmpty(path)) return;
            Import(File.ReadAllText(path, new UTF8Encoding(false, true)), null, false);
        }
        catch (Exception error) { EditorUtility.DisplayDialog("裝備表未匯入", error.Message, "知道了"); }
        finally { importing = false; }
    }

    private static byte[] ReadLocal() => File.Exists(LocalPath) ? File.ReadAllBytes(LocalPath) : null;

    // Runtime uses the same validator, and never downloads balance data during play.
    private static void Import(string csv, byte[] expectedLocal, bool checkLocal)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            throw new InvalidOperationException("請先停止遊戲並等待編譯完成，再匯入裝備表。");
        WeaponDataTable table = WeaponDataTable.Parse(csv);
        byte[] previous = ReadLocal();
        if (checkLocal && ((expectedLocal == null) != (previous == null) ||
            (expectedLocal != null && !expectedLocal.SequenceEqual(previous))))
            throw new InvalidOperationException("下載期間本機 Weapon.csv 有變更，已取消覆蓋，請確認後重新下載。");
        string project = Directory.GetParent(Application.dataPath).FullName;
        string backup = Path.Combine(Directory.GetParent(project).FullName, "ProjectD AI", "Weapon Data Backups", DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));
        Directory.CreateDirectory(backup);
        if (previous != null) File.WriteAllBytes(Path.Combine(backup, "Weapon.before.csv"), previous);
        File.WriteAllText(Path.Combine(backup, "Weapon.incoming.csv"), csv, new UTF8Encoding(false));
        try
        {
            File.WriteAllText(LocalPath, csv, new UTF8Encoding(false));
            AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);
            TextAsset imported = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetPath);
            if (imported == null) throw new IOException("Unity 未能載入更新後的 Weapon.csv。");
            WeaponDataTable.Parse(imported.text);
        }
        catch
        {
            if (previous != null) File.WriteAllBytes(LocalPath, previous);
            else if (File.Exists(LocalPath)) File.Delete(LocalPath);
            AssetDatabase.Refresh();
            throw;
        }
        Debug.Log($"裝備表已匯入：{table.Count} 筆。下次開始遊戲生效。備份：{backup}");
        EditorUtility.DisplayDialog("裝備表已匯入", $"已更新 {table.Count} 筆裝備資料，下次開始遊戲生效。\n裝備名稱翻譯仍由多語系主表管理。", "完成");
    }
}
