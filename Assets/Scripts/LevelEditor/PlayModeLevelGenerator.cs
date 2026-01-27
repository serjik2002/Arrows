// ============================================================================
// PlayModeLevelGenerator.cs
// Simple level generator for testing in Play Mode
// Place in: Assets/Scripts/Development/PlayModeLevelGenerator.cs
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using ArrowPuzzle;

public class PlayModeLevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelController _levelController;
    [SerializeField] private GridView _gridView;
    [SerializeField] private LevelView _levelView;
    [SerializeField] private CameraController _cameraController;

    [Header("Generation Settings")]
    [SerializeField] private int _width = 6;
    [SerializeField] private int _height = 8;
    [SerializeField] private int _minLength = 2;
    [SerializeField] private int _maxLength = 8;
    [SerializeField] private float _turnChance = 0.6f;
    [SerializeField] private bool _useRandomSeed = true;
    [SerializeField] private int _seed = -1;

    [Header("Save Settings")]
    [SerializeField] private string _levelName = "test_level";
    [SerializeField] private bool _autoIncrement = true;
    [SerializeField] private int _levelNumber = 1;

    [Header("UI (Optional)")]
    [SerializeField] private TextMeshProUGUI _infoText;
    [SerializeField] private Button _generateButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _solveButton;

    [Header("Hotkeys")]
    [SerializeField] private KeyCode _generateKey = KeyCode.G;
    [SerializeField] private KeyCode _saveKey = KeyCode.S;
    [SerializeField] private KeyCode _solveKey = KeyCode.C;
    [SerializeField] private KeyCode _validateKey = KeyCode.V;

    private LevelModel _currentLevel;
    private LevelGenerator _generator;

    private void Start()
    {
        _generator = new LevelGenerator();
        

        // Setup UI buttons if assigned
        if (_generateButton != null)
            _generateButton.onClick.AddListener(GenerateNewLevel);

        if (_saveButton != null)
            _saveButton.onClick.AddListener(SaveCurrentLevel);

        if (_solveButton != null)
            _solveButton.onClick.AddListener(TrySolve);

        UpdateInfoText("Press G to generate level");
    }

    private void Update()
    {
        // Hotkeys
        if (Input.GetKeyDown(_generateKey))
        {
            GenerateNewLevel();
        }

        if (Input.GetKeyDown(_saveKey))
        {
            SaveCurrentLevel();
        }

        if (Input.GetKeyDown(_solveKey))
        {
            TrySolve();
        }

        if (Input.GetKeyDown(_validateKey))
        {
            ValidateCurrentLevel();
        }
    }

    [ContextMenu("Generate New Level")]
    public void GenerateNewLevel()
    {
        try
        {
            Debug.Log($"[PlayMode Generator] Generating {_width}x{_height} level...");

            int actualSeed = _useRandomSeed ? -1 : _seed;
            _generator = new LevelGenerator(actualSeed);

            _currentLevel = _generator.GenerateLevelModel(
                _width,
                _height,
                _minLength,
                _maxLength,
                _turnChance
            );

            if (_currentLevel == null || _currentLevel.Arrows.Count == 0)
            {
                Debug.LogWarning("[PlayMode Generator] Generated empty level!");
                UpdateInfoText("Failed: Empty level generated");
                return;
            }

            // Initialize views
            if (_gridView != null)
            {
                _gridView.Init(_currentLevel.Height, _currentLevel.Width);
            }

            if (_levelView != null)
            {
                _levelView.RenderLevel(_currentLevel);
            }

            // Fit camera
            if (_cameraController != null && _gridView != null)
            {
                _cameraController.FitToGrid(_gridView);
            }

            // Initialize controller
            if (_levelController != null)
            {
                _levelController.Initialize(_currentLevel);
            }

            string info = $"Generated: {_width}x{_height}, Arrows: {_currentLevel.Arrows.Count}";
            Debug.Log($"[PlayMode Generator] {info}");
            UpdateInfoText(info);

            PrintGridToConsole();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayMode Generator] Generation failed: {e.Message}\n{e.StackTrace}");
            UpdateInfoText($"Error: {e.Message}");
        }
    }

    [ContextMenu("Save Current Level")]
    public void SaveCurrentLevel()
    {
        if (_currentLevel == null)
        {
            Debug.LogWarning("[PlayMode Generator] No level to save!");
            UpdateInfoText("No level to save");
            return;
        }

        try
        {
            string json = LevelLoader.SaveToJSON(_currentLevel);

            // Determine filename
            string fileName = _autoIncrement ? $"level_{_levelNumber}" : _levelName;
            if (!fileName.EndsWith(".json"))
                fileName += ".json";

            string path = Path.Combine(Application.dataPath, "Resources/Levels");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fullPath = Path.Combine(path, fileName);

            File.WriteAllText(fullPath, json);

            string info = $"Saved: {fileName}";
            Debug.Log($"[PlayMode Generator] {info}");
            UpdateInfoText(info);

            if (_autoIncrement)
                _levelNumber++;

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayMode Generator] Save failed: {e.Message}");
            UpdateInfoText($"Save failed: {e.Message}");
        }
    }

    [ContextMenu("Try Solve Level")]
    public void TrySolve()
    {
        if (_currentLevel == null)
        {
            Debug.LogWarning("[PlayMode Generator] No level to solve!");
            return;
        }

        // Create a copy for testing
        string json = LevelLoader.SaveToJSON(_currentLevel);
        LevelModel testLevel = LevelLoader.LoadFromJSON(json);

        int moves = 0;
        int maxMoves = 1000;
        bool stuck = false;
        GridCoordinate blockedCell;

        while (testLevel.Arrows.Count > 0 && moves < maxMoves)
        {
            bool madeMoveThisTurn = false;

            // Check all arrows
            foreach (var arrow in testLevel.Arrows.Values)
            {
                if (testLevel.CanArrowFlyAway(arrow.Id, out blockedCell))
                {
                    testLevel.RemoveArrow(arrow.Id);
                    moves++;
                    madeMoveThisTurn = true;
                    break; // Only remove one per iteration for accurate count
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
            string info = $"✓ SOLVABLE in {moves} moves";
            Debug.Log($"[PlayMode Generator] {info}");
            UpdateInfoText(info);
        }
        else if (stuck)
        {
            string info = $"✗ UNSOLVABLE - {testLevel.Arrows.Count} arrows stuck";
            Debug.LogWarning($"[PlayMode Generator] {info}");
            UpdateInfoText(info);
        }
        else
        {
            string info = $"✗ Timeout after {maxMoves} moves";
            Debug.LogWarning($"[PlayMode Generator] {info}");
            UpdateInfoText(info);
        }
    }

    [ContextMenu("Validate Level")]
    public void ValidateCurrentLevel()
    {
        if (_currentLevel == null)
        {
            Debug.LogWarning("[PlayMode Generator] No level to validate!");
            return;
        }

        bool valid = true;
        string errors = "";

        // Check grid bounds
        if (_currentLevel.Width <= 0 || _currentLevel.Height <= 0)
        {
            errors += $"Invalid grid size: {_currentLevel.Width}x{_currentLevel.Height}\n";
            valid = false;
        }

        // Check each arrow
        foreach (var arrow in _currentLevel.Arrows.Values)
        {
            if (arrow.StartPoint == null || arrow.EndPoint == null)
            {
                errors += $"Arrow {arrow.Id} has null points\n";
                valid = false;
                continue;
            }

            // Check if arrow points are within grid bounds
            ArrowPoint current = arrow.StartPoint;
            while (current != null)
            {
                if (!_currentLevel.IsInsideGrid(current.GridPosition.ToVector2Int()))
                {
                    errors += $"Arrow {arrow.Id} point {current.GridPosition} outside grid\n";
                    valid = false;
                }
                current = current.Next;
            }
        }

        if (valid)
        {
            Debug.Log("[PlayMode Generator] ✓ Validation PASSED");
            UpdateInfoText("✓ Validation PASSED");
        }
        else
        {
            Debug.LogError($"[PlayMode Generator] ✗ Validation FAILED:\n{errors}");
            UpdateInfoText("✗ Validation FAILED");
        }
    }

    private void PrintGridToConsole()
    {
        if (_currentLevel == null) return;

        string output = "=== Generated Level Grid ===\n";

        for (int row = 0; row < _currentLevel.Height; row++)
        {
            for (int col = 0; col < _currentLevel.Width; col++)
            {
                int id = _currentLevel.OccupiedGrid[row, col];
                output += (id == 0 ? " . " : $" {id} ");
            }
            output += "\n";
        }

        Debug.Log(output);
    }

    private void UpdateInfoText(string text)
    {
        if (_infoText != null)
        {
            _infoText.text = text;
        }
    }

    // Public methods for Inspector buttons or external calls
    public void SetWidth(int width) => _width = Mathf.Clamp(width, 3, 20);
    public void SetHeight(int height) => _height = Mathf.Clamp(height, 3, 20);
    public void SetMinLength(int length) => _minLength = Mathf.Clamp(length, 2, 15);
    public void SetMaxLength(int length) => _maxLength = Mathf.Clamp(length, _minLength, 20);
    public void SetTurnChance(float chance) => _turnChance = Mathf.Clamp01(chance);
}


// ============================================================================
// USAGE INSTRUCTIONS
// ============================================================================

/*
=== SETUP INSTRUCTIONS ===

1. CREATE A TEST SCENE:
   - Create new scene: "LevelGeneratorTest"
   - Add Camera with CameraController component
   - Add empty GameObject named "LevelGenerator"

2. SETUP LEVEL GENERATOR OBJECT:
   - Add PlayModeLevelGenerator component
   - Assign references:
     * LevelController (create if needed)
     * GridView (create if needed)
     * LevelView (create if needed)
     * CameraController
   
3. CONFIGURE SETTINGS IN INSPECTOR:
   Generation Settings:
   - Width: 6
   - Height: 8
   - Min Length: 2
   - Max Length: 8
   - Turn Chance: 0.6
   - Use Random Seed: ✓
   
   Save Settings:
   - Level Name: "test_level"
   - Auto Increment: ✓
   - Level Number: 1
   
   Hotkeys (default):
   - Generate Key: G
   - Save Key: S
   - Solve Key: C
   - Validate Key: V

4. OPTIONAL: ADD UI PANEL
   - Create Canvas with SimpleGeneratorUI component
   - Add sliders and text fields
   - Assign references to UI elements

=== HOW TO USE ===

Method 1: HOTKEYS (Fastest)
   G - Generate new level
   S - Save current level
   C - Test if level is solvable
   V - Validate level data

Method 2: INSPECTOR
   Right-click on PlayModeLevelGenerator component:
   - Generate New Level
   - Save Current Level
   - Try Solve Level
   - Validate Level

Method 3: UI PANEL (if created)
   - Adjust sliders for parameters
   - Click "Generate" button
   - Click "Save" button

=== WORKFLOW ===

1. Start Play Mode
2. Press G to generate level
3. Look at the generated level in Game view
4. If you like it:
   - Press S to save
5. If not:
   - Press G again for new level
6. Press C to check if level is solvable
7. Console will show grid and info

=== OUTPUT ===

Saved levels go to:
   Assets/Resources/Levels/level_1.json

Console output shows:
   - Grid visualization (dots and numbers)
   - Arrow count
   - Solvability status
   - Save confirmation

=== TIPS ===

- Increase Turn Chance (0.7-0.9) for more complex paths
- Decrease Turn Chance (0.3-0.5) for straighter arrows
- Larger grids (10x12) allow more arrows
- Press G multiple times until you get a good level
- Use C to verify level is actually solvable before saving
*/