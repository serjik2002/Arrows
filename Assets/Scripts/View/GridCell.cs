using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GridCell : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }

    // Убрали manager, оставили только координаты и размер
    public void Init(int x, int y, float size)
    {
        X = x;
        Y = y;

        // Настраиваем размер колайдера
        var collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = new Vector2(size, size);
        }
    }
}