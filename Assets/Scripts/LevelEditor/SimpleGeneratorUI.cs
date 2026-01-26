// ============================================================================
// PlayModeLevelGenerator.cs
// Simple level generator for testing in Play Mode
// Place in: Assets/Scripts/Development/PlayModeLevelGenerator.cs
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
// ============================================================================
// SimpleGeneratorUI.cs (Optional UI Component)
// Simple UI panel for the generator controls
// ============================================================================

public class SimpleGeneratorUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayModeLevelGenerator _generator;

    [Header("UI Elements")]
    [SerializeField] private Slider _widthSlider;
    [SerializeField] private Slider _heightSlider;
    [SerializeField] private Slider _minLengthSlider;
    [SerializeField] private Slider _maxLengthSlider;
    [SerializeField] private Slider _turnChanceSlider;

    [SerializeField] private TextMeshProUGUI _widthText;
    [SerializeField] private TextMeshProUGUI _heightText;
    [SerializeField] private TextMeshProUGUI _minLengthText;
    [SerializeField] private TextMeshProUGUI _maxLengthText;
    [SerializeField] private TextMeshProUGUI _turnChanceText;

    [SerializeField] private Toggle _randomSeedToggle;
    [SerializeField] private TMP_InputField _seedInput;

    private void Start()
    {
        if (_generator == null)
        {
            _generator = FindObjectOfType<PlayModeLevelGenerator>();
        }

        SetupSliders();
    }

    private void SetupSliders()
    {
        if (_widthSlider != null)
        {
            _widthSlider.minValue = 3;
            _widthSlider.maxValue = 12;
            _widthSlider.wholeNumbers = true;
            _widthSlider.onValueChanged.AddListener(OnWidthChanged);
        }

        if (_heightSlider != null)
        {
            _heightSlider.minValue = 3;
            _heightSlider.maxValue = 15;
            _heightSlider.wholeNumbers = true;
            _heightSlider.onValueChanged.AddListener(OnHeightChanged);
        }

        if (_minLengthSlider != null)
        {
            _minLengthSlider.minValue = 2;
            _minLengthSlider.maxValue = 10;
            _minLengthSlider.wholeNumbers = true;
            _minLengthSlider.onValueChanged.AddListener(OnMinLengthChanged);
        }

        if (_maxLengthSlider != null)
        {
            _maxLengthSlider.minValue = 2;
            _maxLengthSlider.maxValue = 15;
            _maxLengthSlider.wholeNumbers = true;
            _maxLengthSlider.onValueChanged.AddListener(OnMaxLengthChanged);
        }

        if (_turnChanceSlider != null)
        {
            _turnChanceSlider.minValue = 0f;
            _turnChanceSlider.maxValue = 1f;
            _turnChanceSlider.onValueChanged.AddListener(OnTurnChanceChanged);
        }
    }

    private void OnWidthChanged(float value)
    {
        if (_generator != null)
            _generator.SetWidth((int)value);

        if (_widthText != null)
            _widthText.text = $"Width: {(int)value}";
    }

    private void OnHeightChanged(float value)
    {
        if (_generator != null)
            _generator.SetHeight((int)value);

        if (_heightText != null)
            _heightText.text = $"Height: {(int)value}";
    }

    private void OnMinLengthChanged(float value)
    {
        if (_generator != null)
            _generator.SetMinLength((int)value);

        if (_minLengthText != null)
            _minLengthText.text = $"Min: {(int)value}";

        // Ensure max is not less than min
        if (_maxLengthSlider != null && _maxLengthSlider.value < value)
        {
            _maxLengthSlider.value = value;
        }
    }

    private void OnMaxLengthChanged(float value)
    {
        if (_generator != null)
            _generator.SetMaxLength((int)value);

        if (_maxLengthText != null)
            _maxLengthText.text = $"Max: {(int)value}";

        // Ensure min is not more than max
        if (_minLengthSlider != null && _minLengthSlider.value > value)
        {
            _minLengthSlider.value = value;
        }
    }

    private void OnTurnChanceChanged(float value)
    {
        if (_generator != null)
            _generator.SetTurnChance(value);

        if (_turnChanceText != null)
            _turnChanceText.text = $"Turn: {(value * 100):F0}%";
    }
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