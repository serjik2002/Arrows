using System.Collections.Generic;
using UnityEngine;

public class LevelView : MonoBehaviour
{
    [Header("Налаштування")]
    [SerializeField] private GridView _gridView; // Щоб знати координати точок
    [SerializeField] private GameObject _arrowPrefab; // Префаб лінії
    [SerializeField] private GameObject _headPrefab;  // Префаб голови


    private Dictionary<int, GameObject> _visualArrows = new Dictionary<int, GameObject>();

    //TODO: object pooling для оптимізації
    // Метод очистки сцени перед новим рівнем
    public void ClearView()
    {
        foreach (var obj in _visualArrows.Values)
        {
            Destroy(obj);
        }
        _visualArrows.Clear();
    }


    public void RemoveVisualArrow(int arrowId)
    {
        if (_visualArrows.TryGetValue(arrowId, out GameObject arrowObj))
        {
            Destroy(arrowObj); // Видаляємо зі сцени
            _visualArrows.Remove(arrowId); // Видаляємо зі словника
        }
    }

    // Головний метод: Малює весь рівень
    public void RenderLevel(LevelModel level)
    {
        ClearView(); // Про всяк випадок

        foreach (var arrowModel in level.Arrows.Values)
        {
            RenderArrow(arrowModel);
        }
    }


    private void RenderArrow(ArrowModel arrow)
    {
        // 1. Створюємо об'єкт лінії
        GameObject lineObj = Instantiate(_arrowPrefab, transform);
        lineObj.name = $"Visual_Arrow_{arrow.Id}";
        _visualArrows.Add(arrow.Id, lineObj);

        LineRenderer lr = lineObj.GetComponent<LineRenderer>();

        // 2. Будуємо шлях для LineRenderer
        List<Vector3> worldPositions = new List<Vector3>();
        ArrowPoint current = arrow.StartPoint;

        // Додаємо першу точку
        worldPositions.Add(GridToWorld(current.GridPosition));

        while (current.Next != null)
        {
            // Додаємо проміжні точки (згладжування кутів)
            AddIntermediatePoints(worldPositions, current.GridPosition, current.Next.GridPosition);
            current = current.Next;
        }

        // Застосовуємо точки до лінії
        lr.positionCount = worldPositions.Count;
        lr.SetPositions(worldPositions.ToArray());

        // 3. Малюємо голову стрілки (в кінці шляху)
        RenderHead(arrow, lineObj.transform);
    }

    private void RenderHead(ArrowModel arrow, Transform parent)
    {
        if (arrow.EndPoint == null || arrow.EndPoint.Prev == null) return;

        Vector3 endPos = GridToWorld(arrow.EndPoint.GridPosition);
        Vector3 prevPos = GridToWorld(arrow.EndPoint.Prev.GridPosition);

        Vector3 dir = endPos - prevPos;
        float zAngle = 0f;

        // Перевіряємо, куди більше зміщення: по горизонталі чи вертикалі
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            // Рух по горизонталі
            zAngle = (dir.x > 0) ? -90f : -270f; // Вправо : Вліво
        }
        else
        {
            // Рух по вертикалі
            zAngle = (dir.y > 0) ? 0f : 180f; // Вгору : Вниз
        }

        Quaternion rotation = Quaternion.Euler(0, 0, zAngle);
        Instantiate(_headPrefab, endPos, rotation, parent);
    }


    private void AddIntermediatePoints(List<Vector3> positions, Vector2Int from, Vector2Int to)
    {
        Vector2 dir = to - from;
        int steps = (int)(Mathf.Abs(dir.x) + Mathf.Abs(dir.y));
        if (steps > 0)
        {
            for (int i = 1; i <= steps; i++)
            {
                Vector2 stepPos = (Vector2)from + dir * i / steps;
                positions.Add(GridToWorld(stepPos));
            }
        }
    }


    private Vector3 GridToWorld(Vector2 gridPos)
    {
        int x = (int)gridPos.x;
        int y = (int)gridPos.y;

        // Беремо реальні координати з твого GridView
        if (_gridView != null && _gridView.PointPositions != null)
        {
            if (x >= 0 && x < _gridView.PointPositions.GetLength(0) &&
                y >= 0 && y < _gridView.PointPositions.GetLength(1))
            {
                Vector2 worldPos = _gridView.PointPositions[x, y];
                return new Vector3(worldPos.x, worldPos.y, 0);
            }
        }
        return Vector3.zero;
    }

}
