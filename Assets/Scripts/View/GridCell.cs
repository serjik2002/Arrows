using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GridCell : MonoBehaviour
{
    public int Column { get; private set; }
    public int Row { get; private set; }
    public GridCoordinate Position => new GridCoordinate(Column, Row);

    public void Init(int col, int row, float size)
    {
        Column = col;
        Row = row;

        var collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = new Vector2(size, size);
        }
    }
}