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
    public float CellSize => _cellSize;
    public Vector3 StartPosition => _startPosition != null ? _startPosition.position : transform.position;
    public int Rows => _rows;
    public int Columns => _columns;



    public void Init(int rows, int cols)
    {
        _rows = rows;
        _columns = cols;

        for (int k = transform.childCount - 1; k >= 0; k--)
        {
            GameObject child = transform.GetChild(k).gameObject;

#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(child);
            else Destroy(child);
#else
            Destroy(child);
#endif
        }

        // 2. Створення нового масиву
        _pointPositions = new Vector2[_rows, _columns];

        // 3. Генерація нових клітинок
        // i = Рядок (Row / Y), j = Стовпець (Col / X)
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                var cell = Instantiate(_cellPrefab, transform);

                // Передаємо логічні (X=j, Y=i)
                cell.GetComponent<GridCell>().Init(j, i, _cellSize);

                // Розрахунок позиції у світі
                cell.transform.position = new Vector3(
                    _startPosition.position.x + j * _cellSize,
                    _startPosition.position.y - i * _cellSize,
                    _startPosition.position.z
                );

                // Зберігаємо в [Row, Col] -> [i, j]
                _pointPositions[i, j] = new Vector2(cell.transform.position.x, cell.transform.position.y);
            }
        }
    }
}