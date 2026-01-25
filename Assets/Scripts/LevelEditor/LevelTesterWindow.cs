// ============================================================================
// LevelTesterWindow.cs
// Place in: Assets/Editor/LevelTesterWindow.cs
// Unity Editor Window for level generation
// ============================================================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
#endif


// ============================================================================
// LevelTesterWindow.cs  
// Place in: Assets/Editor/LevelTesterWindow.cs
// Test levels in Editor without running the game
// ============================================================================

#if UNITY_EDITOR


public class LevelTesterWindow : EditorWindow
{
    [MenuItem("Tools/Arrow Puzzle/Level Tester")]
    public static void ShowWindow()
    {
        GetWindow<LevelTesterWindow>("Level Tester");
    }

    private string levelName = "level_1";
    private LevelModel loadedLevel;
    private Vector2 scrollPosition;
    private string testResult = "";

    private void OnGUI()
    {
        GUILayout.Label("Level Tester", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Load section
        EditorGUILayout.BeginHorizontal();
        levelName = EditorGUILayout.TextField("Level Name:", levelName);

        if (GUILayout.Button("Load", GUILayout.Width(60)))
        {
            LoadLevel();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Test buttons
        EditorGUI.BeginDisabledGroup(loadedLevel == null);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Validate Level", GUILayout.Height(30)))
        {
            ValidateLevel();
        }

        if (GUILayout.Button("Test Solvability", GUILayout.Height(30)))
        {
            TestSolvability();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUI.EndDisabledGroup();

        // Results
        if (!string.IsNullOrEmpty(testResult))
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(testResult, MessageType.Info);
        }

        // Preview
        if (loadedLevel != null)
        {
            EditorGUILayout.Space();
            DrawLevelPreview();
        }
    }

    private void LoadLevel()
    {
        try
        {
            TextAsset levelFile = Resources.Load<TextAsset>($"Levels/{levelName}");

            if (levelFile == null)
            {
                testResult = $"✗ Level '{levelName}' not found in Resources/Levels/";
                EditorUtility.DisplayDialog("Error", testResult, "OK");
                return;
            }

            loadedLevel = LevelLoader.LoadFromJSON(levelFile.text);
            testResult = $"✓ Loaded '{levelName}' - {loadedLevel.Width}x{loadedLevel.Height} with {loadedLevel.Arrows.Count} arrows";

            Debug.Log($"[Level Tester] {testResult}");
        }
        catch (System.Exception e)
        {
            testResult = $"✗ Load failed: {e.Message}";
            EditorUtility.DisplayDialog("Error", testResult, "OK");
            Debug.LogError($"[Level Tester] {e}");
        }
    }

    private void ValidateLevel()
    {
        if (loadedLevel == null) return;

        bool hasErrors = false;
        string errors = "";

        // Check grid size
        if (loadedLevel.Width <= 0 || loadedLevel.Height <= 0)
        {
            errors += $"- Invalid grid size: {loadedLevel.Width}x{loadedLevel.Height}\n";
            hasErrors = true;
        }

        // Check arrows
        foreach (var arrow in loadedLevel.Arrows.Values)
        {
            if (arrow.StartPoint == null || arrow.EndPoint == null)
            {
                errors += $"- Arrow {arrow.Id} has null points\n";
                hasErrors = true;
                continue;
            }

            // Check bounds
            ArrowPoint current = arrow.StartPoint;
            while (current != null)
            {
                if (!loadedLevel.IsInsideGrid(current.GridPosition.ToVector2Int()))
                {
                    errors += $"- Arrow {arrow.Id} point {current.GridPosition} outside grid\n";
                    hasErrors = true;
                }
                current = current.Next;
            }
        }

        if (hasErrors)
        {
            testResult = "✗ Validation FAILED:\n" + errors;
            EditorUtility.DisplayDialog("Validation Failed", errors, "OK");
        }
        else
        {
            testResult = "✓ Validation PASSED - No errors found!";
            EditorUtility.DisplayDialog("Success", testResult, "OK");
        }
    }

    private void TestSolvability()
    {
        if (loadedLevel == null) return;

        // Create a copy for testing
        LevelModel testLevel = CloneLevel(loadedLevel);

        int moves = 0;
        int maxMoves = 1000;
        bool stuck = false;

        while (testLevel.Arrows.Count > 0 && moves < maxMoves)
        {
            bool madeMoveThisTurn = false;

            foreach (var arrow in testLevel.Arrows.Values)
            {
                if (testLevel.CanArrowFlyAway(arrow.Id))
                {
                    testLevel.RemoveArrow(arrow.Id);
                    moves++;
                    madeMoveThisTurn = true;
                }
            }

            if (!madeMoveThisTurn)
            {
                stuck = true;
                break;
            }
        }

        if (testLevel.Arrows.Count == 0)
        {
            testResult = $"✓ Level is SOLVABLE in {moves} moves!";
            EditorUtility.DisplayDialog("Success", testResult, "OK");
        }
        else if (stuck)
        {
            testResult = $"✗ Level is UNSOLVABLE - stuck with {testLevel.Arrows.Count} arrows remaining";
            EditorUtility.DisplayDialog("Failed", testResult, "OK");
        }
        else
        {
            testResult = $"✗ Test timeout after {maxMoves} moves";
            EditorUtility.DisplayDialog("Timeout", testResult, "OK");
        }
    }

    private void DrawLevelPreview()
    {
        GUILayout.Label("Level Preview", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

        GUIStyle cellStyle = new GUIStyle(EditorStyles.miniButton);
        cellStyle.fontSize = 10;

        for (int row = 0; row < loadedLevel.Height; row++)
        {
            EditorGUILayout.BeginHorizontal();

            for (int col = 0; col < loadedLevel.Width; col++)
            {
                int id = loadedLevel.OccupiedGrid[row, col];
                string label = id == 0 ? "·" : id.ToString();

                GUILayout.Button(label, cellStyle, GUILayout.Width(30), GUILayout.Height(30));
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private LevelModel CloneLevel(LevelModel source)
    {
        string json = LevelLoader.SaveToJSON(source);
        return LevelLoader.LoadFromJSON(json);
    }
}

#endif
#if UNITY_EDITOR
#endif