// ============================================================================
// CameraController.cs - FIXED VERSION
// Responsibility: Camera positioning and zoom management
// ============================================================================

using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera _camera;

    [Header("Settings")]
    [SerializeField] private float _padding = 1.5f;
    [SerializeField] private bool _autoAssignMainCamera = true;

    private void Awake()
    {
        // Auto-assign main camera if not set
        if (_camera == null && _autoAssignMainCamera)
        {
            _camera = Camera.main;

            if (_camera == null)
            {
                Debug.LogError("[CameraController] No camera found! Assign manually or add Main Camera tag.");
            }
        }
    }

    /// <summary>
    /// Fits camera to show the entire grid with padding
    /// </summary>
    /// <param name="gridView">The grid view to fit to</param>
    public void FitToGrid(GridView gridView)
    {
        if (_camera == null)
        {
            Debug.LogError("[CameraController] Camera is not assigned!");
            return;
        }

        if (gridView == null)
        {
            Debug.LogError("[CameraController] GridView is null!");
            return;
        }

        // Get grid parameters
        int rows = gridView.Rows;
        int cols = gridView.Columns;
        float cellSize = gridView.CellSize;
        Vector3 startPos = gridView.StartPosition;

        // Calculate grid dimensions in world units
        float gridWorldWidth = cols * cellSize;
        float gridWorldHeight = rows * cellSize;

        // Calculate grid center position
        // X: start + (columns - 1) * cellSize / 2
        // Y: start - (rows - 1) * cellSize / 2 (Y goes down)
        float centerX = startPos.x + (cols - 1) * cellSize / 2.0f;
        float centerY = startPos.y - (rows - 1) * cellSize / 2.0f;

        // Position camera at grid center (preserve Z)
        Vector3 newPosition = new Vector3(centerX, centerY, _camera.transform.position.z);
        _camera.transform.position = newPosition;

        // Calculate orthographic size to fit grid with padding
        // Orthographic size = half of viewport height in world units

        // Height needed (with padding)
        float targetHeight = (gridWorldHeight / 2.0f) + _padding;

        // Width needed (converted to height via aspect ratio)
        float aspect = _camera.aspect; // width / height
        float targetWidth = (gridWorldWidth / 2.0f + _padding) / aspect;

        // Use the larger value to ensure grid fits both horizontally and vertically
        _camera.orthographicSize = Mathf.Max(targetHeight, targetWidth);

        Debug.Log($"[CameraController] Fitted camera to {cols}×{rows} grid at ({centerX:F2}, {centerY:F2}) with size {_camera.orthographicSize:F2}");
    }

    /// <summary>
    /// Fits camera to grid with explicit parameters
    /// </summary>
    public void FitToGrid(int rows, int cols, float cellSize, Vector3 startPosition)
    {
        if (_camera == null)
        {
            Debug.LogError("[CameraController] Camera is not assigned!");
            return;
        }

        // Calculate grid dimensions
        float gridWorldWidth = cols * cellSize;
        float gridWorldHeight = rows * cellSize;

        // Calculate center
        float centerX = startPosition.x + (cols - 1) * cellSize / 2.0f;
        float centerY = startPosition.y - (rows - 1) * cellSize / 2.0f;

        // Position camera
        _camera.transform.position = new Vector3(centerX, centerY, _camera.transform.position.z);

        // Calculate orthographic size
        float targetHeight = (gridWorldHeight / 2.0f) + _padding;
        float targetWidth = (gridWorldWidth / 2.0f + _padding) / _camera.aspect;

        _camera.orthographicSize = Mathf.Max(targetHeight, targetWidth);
    }

    /// <summary>
    /// Sets the padding around the grid
    /// </summary>
    public void SetPadding(float padding)
    {
        _padding = Mathf.Max(0, padding);
    }

    /// <summary>
    /// Gets current padding value
    /// </summary>
    public float GetPadding() => _padding;

    /// <summary>
    /// Smoothly transitions camera to fit grid
    /// </summary>
    public void FitToGridSmooth(GridView gridView, float duration = 0.5f)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(FitToGridCoroutine(gridView, duration));
        }
        else
        {
            FitToGrid(gridView);
        }
    }

    private System.Collections.IEnumerator FitToGridCoroutine(GridView gridView, float duration)
    {
        if (_camera == null || gridView == null) yield break;

        // Calculate target values
        int rows = gridView.Rows;
        int cols = gridView.Columns;
        float cellSize = gridView.CellSize;
        Vector3 startPos = gridView.StartPosition;

        float centerX = startPos.x + (cols - 1) * cellSize / 2.0f;
        float centerY = startPos.y - (rows - 1) * cellSize / 2.0f;
        Vector3 targetPosition = new Vector3(centerX, centerY, _camera.transform.position.z);

        float gridWorldWidth = cols * cellSize;
        float gridWorldHeight = rows * cellSize;
        float targetHeight = (gridWorldHeight / 2.0f) + _padding;
        float targetWidth = (gridWorldWidth / 2.0f + _padding) / _camera.aspect;
        float targetSize = Mathf.Max(targetHeight, targetWidth);

        // Store start values
        Vector3 startPosition = _camera.transform.position;
        float startSize = _camera.orthographicSize;
        float elapsed = 0f;

        // Animate
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Smooth interpolation
            t = t * t * (3f - 2f * t); // Smoothstep

            _camera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            _camera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);

            yield return null;
        }

        // Ensure exact final values
        _camera.transform.position = targetPosition;
        _camera.orthographicSize = targetSize;
    }

#if UNITY_EDITOR
    [Header("Debug Visualization")]
    [SerializeField] private bool _showDebugGizmos = true;
    [SerializeField] private Color _gizmoColor = Color.cyan;

    private void OnDrawGizmos()
    {
        if (!_showDebugGizmos || _camera == null) return;

        // Draw camera frustum visualization
        Gizmos.color = _gizmoColor;

        float height = _camera.orthographicSize * 2f;
        float width = height * _camera.aspect;

        Vector3 center = _camera.transform.position;
        center.z = 0; // Draw on plane

        // Draw rectangle showing camera view
        Vector3 topLeft = center + new Vector3(-width / 2, height / 2, 0);
        Vector3 topRight = center + new Vector3(width / 2, height / 2, 0);
        Vector3 bottomRight = center + new Vector3(width / 2, -height / 2, 0);
        Vector3 bottomLeft = center + new Vector3(-width / 2, -height / 2, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);

        // Draw center cross
        float crossSize = 0.5f;
        Gizmos.DrawLine(center + Vector3.left * crossSize, center + Vector3.right * crossSize);
        Gizmos.DrawLine(center + Vector3.up * crossSize, center + Vector3.down * crossSize);
    }
#endif
}


// ============================================================================
// GridView.cs - UPDATED WITH HELPER METHODS
// Add these methods to your existing GridView.cs
// ============================================================================

