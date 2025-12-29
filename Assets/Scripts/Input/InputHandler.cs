using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public event System.Action<GridCoordinate> OnGriddClick;
    
    private Camera _camera;


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryGetClickedCell(out var coord);
        }
    }

    private bool TryGetClickedCell(out GridCoordinate coord)
    {
        coord = default;
        Vector2 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            var cell = hit.collider.GetComponent<GridCell>();
            if (cell != null)
            {
                coord = new GridCoordinate(cell.X, cell.Y);
                OnGriddClick?.Invoke(coord);
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

    public static GridCoordinate FromVector2Int(Vector2Int v)
        => new GridCoordinate(v.x, v.y);
}