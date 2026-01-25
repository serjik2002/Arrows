using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _startPosition;

    [SerializeField] private float _padding = 1.5f;
    
    public void FitCameraToGrid(int rows, int cols, float cellSize, Vector3 center)
    {
        if (_camera == null) return;

        float totalWidth = cols * cellSize;
        float totalHeight = rows * cellSize;

        float centerX = _startPosition. position.x + (cols - 1) * cellSize / 2.0f;
        float centerY = _startPosition.position.y - (rows - 1) * cellSize / 2.0f;

        _camera.transform.position = new Vector3(centerX, centerY, _camera.transform.position.z);

        float screenAspect = (float)Screen.width / (float)Screen.height;
        float targetAspect = totalWidth / totalHeight;

        if (screenAspect >= targetAspect)
        {
            _camera.orthographicSize = (totalHeight / 2.0f) + _padding;
        }
        else
        {
            float differenceInSize = targetAspect / screenAspect;
            _camera.orthographicSize = ((totalHeight / 2.0f) * differenceInSize) + _padding;
        }
    }

}
