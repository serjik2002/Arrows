using UnityEngine;
using UnityEngine.EventSystems;


public class InputHandler : MonoBehaviour
{
    public event System.Action<GridCoordinate> OnGridClick;

    [SerializeField] private Camera _camera; // Можна призначити в інспекторі

    private bool _isInputActive = true; // Прапорець для блокування введення

    private void Awake()
    {
        // Якщо камеру не призначили в інспекторі, беремо головну
        if (_camera == null) _camera = Camera.main;
    }

    private void Update()
    {
        if (!_isInputActive) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            TryGetClickedCell(out var coord);
        }
    }

    public void SetInputActive(bool active)
    {
        _isInputActive = active;
    }

    private bool TryGetClickedCell(out GridCoordinate coord)
    {
        coord = default;
        Vector2 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);

        // ВАЖЛИВО: Переконайся, що на GridCell є BoxCollider2D
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            var cell = hit.collider.GetComponent<GridCell>();
            if (cell != null)
            {
                // Створюємо координату (X = Column, Y = Row)
                coord = new GridCoordinate(cell.X, cell.Y);

                // Викликаємо подію!
                OnGridClick?.Invoke(coord);
                return true;
            }
        }
        return false;
    }
}

public struct GridCoordinate
{
    public int Column { get; private set; } // X
    public int Row { get; private set; }    // Y

    public GridCoordinate(int col, int row)
    {
        Column = col;
        Row = row;
    }

    public Vector2Int ToVector2Int() => new Vector2Int(Column, Row);

    public static GridCoordinate FromVector2Int(Vector2Int vector)
        => new GridCoordinate(vector.x, vector.y);
}