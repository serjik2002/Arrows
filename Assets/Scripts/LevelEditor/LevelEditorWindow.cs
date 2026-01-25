// ============================================================================
// LevelEditorWindow.cs
// Place in: Assets/Editor/LevelEditorWindow.cs
// Hybrid editor: GUI controls + Scene visualization
// ============================================================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using ArrowPuzzle;

public class LevelEditorWindow : EditorWindow
{
    [MenuItem("Tools/Arrow Puzzle/Level Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<LevelEditorWindow>("Level Editor");
        window.minSize = new Vector2(350, 600);
    }

    // Generation settings
    private int width = 6;
    private int height = 8;
    private int minLength = 2;
    private int maxLength = 8;
    private float turnChance = 0.6f;
    private int seed = -1;
    private bool useRandomSeed = true;

    // Save settings
    private string levelName = "level_1";
    private int levelNumber = 1;
    private bool autoIncrement = true;

    // Preview settings
    private bool autoFocusOnGenerate = true;
    private bool showGrid = true;

    // State
    private LevelModel generatedLevel;
    private LevelEditorPreview scenePreview;
    private string lastMessage = "";
    private MessageType lastMessageType = MessageType.Info;

    private void OnEnable()
    {
        // Ensure preview exists
        if (scenePreview == null)
        {
            scenePreview = FindOrCreatePreview();
        }

        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Level Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        DrawGenerationSettings();
        EditorGUILayout.Space();

        DrawPreviewSettings();
        EditorGUILayout.Space();

        DrawGenerationButtons();
        EditorGUILayout.Space();

        DrawSaveSection();
        EditorGUILayout.Space();

        DrawLevelInfo();
        EditorGUILayout.Space();

        DrawBatchGeneration();

        // Status message
        if (!string.IsNullOrEmpty(lastMessage))
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(lastMessage, lastMessageType);
        }
    }

    private void DrawGenerationSettings()
    {
        EditorGUILayout.LabelField("Generation Settings", EditorStyles.boldLabel);

        width = EditorGUILayout.IntSlider("Width (Columns)", width, 3, 12);
        height = EditorGUILayout.IntSlider("Height (Rows)", height, 3, 15);

        EditorGUILayout.Space(5);

        minLength = EditorGUILayout.IntSlider("Min Arrow Length", minLength, 2, 10);
        maxLength = EditorGUILayout.IntSlider("Max Arrow Length", maxLength, minLength, 15);
        turnChance = EditorGUILayout.Slider("Turn Chance", turnChance, 0f, 1f);

        EditorGUILayout.Space(5);

        useRandomSeed = EditorGUILayout.Toggle("Random Seed", useRandomSeed);

        EditorGUI.BeginDisabledGroup(useRandomSeed);
        seed = EditorGUILayout.IntField("Seed", seed);
        EditorGUI.EndDisabledGroup();
    }

    private void DrawPreviewSettings()
    {
        EditorGUILayout.LabelField("Preview Settings", EditorStyles.boldLabel);

        autoFocusOnGenerate = EditorGUILayout.Toggle("Auto Focus Camera", autoFocusOnGenerate);
        showGrid = EditorGUILayout.Toggle("Show Grid", showGrid);

        if (scenePreview != null)
        {
            scenePreview.ShowGrid = showGrid;
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Focus Scene View", GUILayout.Height(25)))
        {
            FocusSceneView();
        }
        if (GUILayout.Button("Clear Preview", GUILayout.Height(25)))
        {
            ClearPreview();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawGenerationButtons()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Generate Level", GUILayout.Height(40)))
        {
            GenerateLevel();
        }

        if (GUILayout.Button("Re-Generate", GUILayout.Height(40)))
        {
            GenerateLevel();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginDisabledGroup(generatedLevel == null);

        if (GUILayout.Button("Test Solve", GUILayout.Height(30)))
        {
            TestSolve();
        }

        if (GUILayout.Button("Validate", GUILayout.Height(30)))
        {
            ValidateLevel();
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawSaveSection()
    {
        EditorGUILayout.LabelField("Save Settings", EditorStyles.boldLabel);

        autoIncrement = EditorGUILayout.Toggle("Auto-increment", autoIncrement);

        if (autoIncrement)
        {
            levelNumber = EditorGUILayout.IntField("Level Number", levelNumber);
            levelName = $"level_{levelNumber}";
            EditorGUILayout.LabelField("File:", $"{levelName}.json");
        }
        else
        {
            levelName = EditorGUILayout.TextField("Level Name", levelName);
        }

        EditorGUI.BeginDisabledGroup(generatedLevel == null);

        if (GUILayout.Button("💾 Save to Resources/Levels", GUILayout.Height(35)))
        {
            SaveLevel();
        }

        EditorGUI.EndDisabledGroup();
    }

    private void DrawLevelInfo()
    {
        if (generatedLevel == null)
        {
            EditorGUILayout.HelpBox("No level generated. Click 'Generate Level' to start.", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField("Current Level Info", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Grid Size:", $"{generatedLevel.Width} × {generatedLevel.Height}");
        EditorGUILayout.LabelField("Total Arrows:", generatedLevel.Arrows.Count.ToString());

        // Arrow details
        if (generatedLevel.Arrows.Count > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Arrows:", EditorStyles.miniBoldLabel);

            foreach (var arrow in generatedLevel.Arrows.Values)
            {
                int length = GetArrowLength(arrow);
                EditorGUILayout.LabelField($"  Arrow {arrow.Id}:", $"{length} cells");
            }
        }
    }

    private void DrawBatchGeneration()
    {
        EditorGUILayout.LabelField("Batch Generation", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Generate 5 Levels", GUILayout.Height(30)))
        {
            BatchGenerate(5);
        }

        if (GUILayout.Button("Generate 10 Levels", GUILayout.Height(30)))
        {
            BatchGenerate(10);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox(
            "Batch generation creates multiple solvable levels with auto-incrementing numbers.",
            MessageType.Info
        );
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        // Draw instructions in scene view
        if (generatedLevel != null && scenePreview != null)
        {
            Handles.BeginGUI();

            GUILayout.BeginArea(new Rect(10, 10, 250, 100));
            GUILayout.BeginVertical("box");

            GUILayout.Label("Level Editor - Scene View", EditorStyles.boldLabel);
            GUILayout.Label($"Size: {generatedLevel.Width}×{generatedLevel.Height}");
            GUILayout.Label($"Arrows: {generatedLevel.Arrows.Count}");

            if (GUILayout.Button("Back to Editor Window"))
            {
                Focus();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();

            Handles.EndGUI();
        }
    }

    private void GenerateLevel()
    {
        try
        {
            if (scenePreview == null)
            {
                scenePreview = FindOrCreatePreview();
            }

            var generator = new LevelGenerator(useRandomSeed ? -1 : seed);

            generatedLevel = generator.GenerateLevelModel(
                width,
                height,
                minLength,
                maxLength,
                turnChance
            );

            // Update scene preview
            scenePreview.DisplayLevel(generatedLevel);

            if (autoFocusOnGenerate)
            {
                FocusSceneView();
            }

            lastMessage = $"✓ Generated {width}×{height} level with {generatedLevel.Arrows.Count} arrows";
            lastMessageType = MessageType.Info;

            Debug.Log($"[Level Editor] {lastMessage}");
        }
        catch (System.Exception e)
        {
            lastMessage = $"✗ Generation failed: {e.Message}";
            lastMessageType = MessageType.Error;
            Debug.LogError($"[Level Editor] {e}");
        }
    }

    private void SaveLevel()
    {
        if (generatedLevel == null) return;

        try
        {
            string json = LevelLoader.SaveToJSON(generatedLevel);

            string folderPath = Path.Combine(Application.dataPath, "Resources/Levels");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = levelName.EndsWith(".json") ? levelName : $"{levelName}.json";
            string fullPath = Path.Combine(folderPath, fileName);

            if (File.Exists(fullPath))
            {
                if (!EditorUtility.DisplayDialog(
                    "Overwrite?",
                    $"'{fileName}' already exists. Overwrite?",
                    "Yes",
                    "No"))
                {
                    return;
                }
            }

            File.WriteAllText(fullPath, json);
            AssetDatabase.Refresh();

            if (autoIncrement)
            {
                levelNumber++;
            }

            lastMessage = $"✓ Saved: Resources/Levels/{fileName}";
            lastMessageType = MessageType.Info;

            Debug.Log($"[Level Editor] {lastMessage}");
        }
        catch (System.Exception e)
        {
            lastMessage = $"✗ Save failed: {e.Message}";
            lastMessageType = MessageType.Error;
            Debug.LogError($"[Level Editor] {e}");
        }
    }

    private void TestSolve()
    {
        if (generatedLevel == null) return;

        var testLevel = CloneLevel(generatedLevel);
        int moves = 0;
        int maxMoves = 1000;

        while (testLevel.Arrows.Count > 0 && moves < maxMoves)
        {
            bool madeMove = false;

            foreach (var arrow in testLevel.Arrows.Values)
            {
                if (testLevel.CanArrowFlyAway(arrow.Id))
                {
                    testLevel.RemoveArrow(arrow.Id);
                    moves++;
                    madeMove = true;
                }
            }

            if (!madeMove) break;
        }

        if (testLevel.Arrows.Count == 0)
        {
            lastMessage = $"✓ Level is SOLVABLE in {moves} moves";
            lastMessageType = MessageType.Info;
        }
        else
        {
            lastMessage = $"✗ Level is UNSOLVABLE - {testLevel.Arrows.Count} arrows stuck";
            lastMessageType = MessageType.Warning;
        }
    }

    private void ValidateLevel()
    {
        if (generatedLevel == null) return;

        bool valid = true;
        string errors = "";

        foreach (var arrow in generatedLevel.Arrows.Values)
        {
            ArrowPoint current = arrow.StartPoint;
            while (current != null)
            {
                if (!generatedLevel.IsInsideGrid(current.GridPosition.ToVector2Int()))
                {
                    errors += $"Arrow {arrow.Id} outside grid\n";
                    valid = false;
                }
                current = current.Next;
            }
        }

        if (valid)
        {
            lastMessage = "✓ Validation PASSED";
            lastMessageType = MessageType.Info;
        }
        else
        {
            lastMessage = $"✗ Validation FAILED:\n{errors}";
            lastMessageType = MessageType.Error;
        }
    }

    private void BatchGenerate(int count)
    {
        if (!EditorUtility.DisplayDialog(
            "Batch Generation",
            $"Generate {count} solvable levels starting from level_{levelNumber}?",
            "Generate",
            "Cancel"))
        {
            return;
        }

        int generated = 0;
        int attempts = 0;
        int maxAttempts = count * 20;

        while (generated < count && attempts < maxAttempts)
        {
            attempts++;

            EditorUtility.DisplayProgressBar(
                "Batch Generation",
                $"Generated {generated}/{count} levels (attempt {attempts})",
                (float)generated / count
            );

            GenerateLevel();

            if (generatedLevel != null)
            {
                SaveLevel();
                generated++;
            }
        }

        EditorUtility.ClearProgressBar();

        lastMessage = $"✓ Generated {generated} levels in {attempts} attempts";
        lastMessageType = MessageType.Info;
    }

    private void FocusSceneView()
    {
        if (scenePreview != null)
        {
            Selection.activeGameObject = scenePreview.gameObject;
            SceneView.lastActiveSceneView?.FrameSelected();
        }
    }

    private void ClearPreview()
    {
        if (scenePreview != null)
        {
            scenePreview.Clear();
        }

        generatedLevel = null;
        lastMessage = "Preview cleared";
        lastMessageType = MessageType.Info;
    }

    private LevelEditorPreview FindOrCreatePreview()
    {
        var existing = FindObjectOfType<LevelEditorPreview>();

        if (existing != null)
        {
            return existing;
        }

        // Create new preview object
        var previewObj = new GameObject("[Level Editor Preview]");
        previewObj.hideFlags = HideFlags.DontSave;

        var preview = previewObj.AddComponent<LevelEditorPreview>();
        preview.Initialize();

        return preview;
    }

    private int GetArrowLength(ArrowModel arrow)
    {
        int count = 0;
        ArrowPoint current = arrow.StartPoint;
        while (current != null)
        {
            count++;
            current = current.Next;
        }
        return count;
    }

    private LevelModel CloneLevel(LevelModel source)
    {
        string json = LevelLoader.SaveToJSON(source);
        return LevelLoader.LoadFromJSON(json);
    }
}


// Extension for ToArray() support in Editor
public static class EditorDictionaryExtensions
{
    public static TValue[] ToArray<TKey, TValue>(
        this System.Collections.Generic.Dictionary<TKey, TValue>.ValueCollection values)
    {
        var result = new TValue[values.Count];
        values.CopyTo(result, 0);
        return result;
    }
}
#endif