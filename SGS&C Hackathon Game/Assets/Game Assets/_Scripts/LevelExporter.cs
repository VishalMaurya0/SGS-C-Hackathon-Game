#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public static class LevelExporter
{
    private static string outputDir = "Assets/StreamingAssets/Levels";

    [MenuItem("Tools/Export Levels to JSON")]
    public static void ExportAllLevels()
    {
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        string[] guids = AssetDatabase.FindAssets("t:LevelSaveSO", new[] { "Assets/Game Assets/LevelSaveSO" });

        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LevelSaveSO so = AssetDatabase.LoadAssetAtPath<LevelSaveSO>(path);
            if (so == null) continue;

            string json = JsonUtility.ToJson(so, true);
            string fileName = Path.GetFileNameWithoutExtension(path) + ".json";
            string outPath = Path.Combine(outputDir, fileName);

            File.WriteAllText(outPath, json);
            count++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"Exported {count} LevelSaveSO assets to {outputDir}");
    }
}
#endif
