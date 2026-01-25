// ============================================================================
// LevelEditorPreview.cs
// Place in: Assets/Editor/LevelEditorPreview.cs
// Hybrid editor: GUI controls + Scene visualization
// ============================================================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
// ============================================================================
// LevelEditorPreview.cs
// Place in: Assets/Scripts/Editor/LevelEditorPreview.cs
// Scene visualization component (runtime-like, but editor-only)
// ============================================================================


#if UNITY_EDITOR
#endif

[ExecuteInEditMode]
public class LevelEditorPreview : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _cellPrefab;
    [SerializeField] private GameObject _arrowPrefab;
    [SerializeField] private GameObject _headPrefab;

    [Header("Settings")]
    [SerializeField] private float _cellSize = 1.0f;
    [SerializeField] private Color _gridColor = new Color(1, 1, 1, 0.3f);

    private GridView _gridView;
    private LevelView _levelView;
    private LevelModel _currentLevel;

    public bool ShowGrid { get; set; } = true;

    public void Initialize()
    {
        // Load prefabs if not assigned
        if (_cellPrefab == null)
        {
            _cellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GridCell.prefab");
        }

        if (_arrowPrefab == null)
        {
            _arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Arrow.prefab");
        }

        if (_headPrefab == null)
        {
            _headPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ArrowHead.prefab");
        }

        // Create child objects for grid and level
        CreateGridView();
        CreateLevelView();
    }

    private void CreateGridView()
    {
        var gridObj = transform.Find("Grid");
        if (gridObj == null)
        {
            gridObj = new GameObject("Grid").transform;
            gridObj.SetParent(transform);
            gridObj.localPosition = Vector3.zero;
        }

        _gridView = gridObj.GetComponent<GridView>();
        if (_gridView == null)
        {
            _gridView = gridObj.gameObject.AddComponent<GridView>();
        }
    }

    private void CreateLevelView()
    {
        var levelObj = transform.Find("Arrows");
        if (levelObj == null)
        {
            levelObj = new GameObject("Arrows").transform;
            levelObj.SetParent(transform);
            levelObj.localPosition = Vector3.zero;
        }

        _levelView = levelObj.GetComponent<LevelView>();
        if (_levelView == null)
        {
            _levelView = levelObj.gameObject.AddComponent<LevelView>();
        }
    }

    public void DisplayLevel(LevelModel level)
    {
        if (level == null) return;

        _currentLevel = level;

        // Initialize grid
        if (_gridView != null)
        {
            _gridView.Init(level.Height, level.Width);
        }

        // Render arrows
        if (_levelView != null)
        {
            _levelView.RenderLevel(level);
        }

        // Position camera
        PositionSceneCamera(level);
    }

    public void Clear()
    {
        _currentLevel = null;

        if (_levelView != null)
        {
            _levelView.ClearView();
        }

        // Clear grid children
        if (_gridView != null)
        {
            while (_gridView.transform.childCount > 0)
            {
                DestroyImmediate(_gridView.transform.GetChild(0).gameObject);
            }
        }
    }

    private void PositionSceneCamera(LevelModel level)
    {
#if UNITY_EDITOR
        var sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null && _gridView != null)
        {
            float gridWidth = level.Width * _cellSize;
            float gridHeight = level.Height * _cellSize;

            float centerX = (level.Width - 1) * _cellSize / 2f;
            float centerY = -(level.Height - 1) * _cellSize / 2f;

            Vector3 center = new Vector3(centerX, centerY, 0);

            // Set scene view to look at grid
            sceneView.LookAt(center, Quaternion.identity, Mathf.Max(gridWidth, gridHeight) * 1.2f);
        }
#endif
    }

    private void OnDrawGizmos()
    {
        if (!ShowGrid || _currentLevel == null) return;

        Gizmos.color = _gridColor;

        int width = _currentLevel.Width;
        int height = _currentLevel.Height;

        // Draw grid lines
        for (int i = 0; i <= height; i++)
        {
            Vector3 start = new Vector3(0, -i * _cellSize, 0);
            Vector3 end = new Vector3(width * _cellSize, -i * _cellSize, 0);
            Gizmos.DrawLine(start, end);
        }

        for (int j = 0; j <= width; j++)
        {
            Vector3 start = new Vector3(j * _cellSize, 0, 0);
            Vector3 end = new Vector3(j * _cellSize, -height * _cellSize, 0);
            Gizmos.DrawLine(start, end);
        }

        // Draw border
        Gizmos.color = Color.white;
        Vector3[] corners = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(width * _cellSize, 0, 0),
            new Vector3(width * _cellSize, -height * _cellSize, 0),
            new Vector3(0, -height * _cellSize, 0)
        };

        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
    }
}
#endif