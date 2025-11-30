using UnityEngine;

public class GridView : MonoBehaviour
{
    [SerializeField] private int _rows = 5;
    [SerializeField] private int _columns = 5;

    [SerializeField] private float _cellSize = 1.0f;
    [SerializeField] private Transform _startPosition;

    [SerializeField] private GameObject _cellPrefab;

    private Vector2[,] _pointPositions;
    public Vector2[,] PointPositions => _pointPositions;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        _pointPositions = new Vector2[_rows, _columns];
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                var cell = Instantiate(_cellPrefab, transform);
                cell.GetComponent<GridCell>().Init(i, j, _cellSize);
                cell.transform.position = new Vector3(
                    _startPosition.position.x + j * _cellSize,
                    _startPosition.position.y - i * _cellSize,
                    _startPosition.position.z
                );
                _pointPositions[i, j] = new Vector2(cell.transform.position.x, cell.transform.position.y);
            }
        }
    }
}
