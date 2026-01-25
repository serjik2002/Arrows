// ============================================================================
// LevelMenuItems.cs
// Place in: Assets/Editor/LevelMenuItems.cs
// Unity Editor Window for level generation
// ============================================================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
// ============================================================================
// LevelMenuItem.cs
// Place in: Assets/Editor/LevelMenuItem.cs  
// Quick menu shortcuts
// ============================================================================

#if UNITY_EDITOR

public static class LevelMenuItems
{
    [MenuItem("Tools/Arrow Puzzle/Open Levels Folder")]
    public static void OpenLevelsFolder()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "Resources/Levels");

        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }

        EditorUtility.RevealInFinder(path);
    }

    [MenuItem("Tools/Arrow Puzzle/Delete All Levels")]
    public static void DeleteAllLevels()
    {
        if (!EditorUtility.DisplayDialog(
            "Delete All Levels",
            "Are you sure you want to delete ALL level files?\n\nThis cannot be undone!",
            "Delete",
            "Cancel"))
        {
            return;
        }

        string path = System.IO.Path.Combine(Application.dataPath, "Resources/Levels");

        if (System.IO.Directory.Exists(path))
        {
            var files = System.IO.Directory.GetFiles(path, "*.json");

            foreach (var file in files)
            {
                System.IO.File.Delete(file);
            }

            AssetDatabase.Refresh();

            Debug.Log($"[Level Tools] Deleted {files.Length} level files");
            EditorUtility.DisplayDialog("Success", $"Deleted {files.Length} level files", "OK");
        }
    }

    [MenuItem("Tools/Arrow Puzzle/Generate 1 Test Level")]
    public static void QuickGenerate()
    {
        var generator = new ArrowPuzzle.LevelGenerator();
        var level = generator.GenerateLevelModel(6, 8, 2, 8, 0.6f);

        string json = LevelLoader.SaveToJSON(level);
        string path = System.IO.Path.Combine(Application.dataPath, "Resources/Levels");

        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }

        string fileName = $"test_level_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
        System.IO.File.WriteAllText(System.IO.Path.Combine(path, fileName), json);

        AssetDatabase.Refresh();

        Debug.Log($"[Level Tools] Generated: {fileName}");
        EditorUtility.DisplayDialog("Success", $"Generated test level:\n{fileName}", "OK");
    }
}
#endif
#endif

