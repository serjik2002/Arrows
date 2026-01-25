using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelView : MonoBehaviour
{
    [Header("Налаштування")]
    [SerializeField] private GridView _gridView;
    [SerializeField] private GameObject _arrowPrefab;
    [SerializeField] private GameObject _headPrefab;

    [Header("Анімація")]
    [SerializeField] private float _snakeSpeed = 5f; // Швидкість уползання
    [SerializeField] private float _offScreenDistance = 10f; // Наскільки далеко повзти за межі

    // Зберігаємо не тільки об'єкт, а й його голову та оригінальний шлях
    private class ArrowVisualData
    {
        public GameObject RootObject;
        public LineRenderer LineRenderer;
        public Transform HeadTransform;
        public List<Vector3> FullPath; // Шлях у світових координатах
    }

    private Dictionary<int, ArrowVisualData> _visualArrows = new Dictionary<int, ArrowVisualData>();

    public void ClearView()
    {
        StopAllCoroutines(); // Зупиняємо анімації, якщо вони йдуть
        foreach (var data in _visualArrows.Values)
        {
            if (data.RootObject != null) Destroy(data.RootObject);
        }
        _visualArrows.Clear();
    }

    // Додаємо параметр animate = true за замовчуванням
    public void RemoveVisualArrow(int arrowId, bool animate = true, float speed = 45f)
    {
        if (_visualArrows.TryGetValue(arrowId, out ArrowVisualData data))
        {
            _visualArrows.Remove(arrowId); // Видаляємо логічно

            if (animate && gameObject.activeInHierarchy)
            {
                // Визначаємо, яку швидкість використовувати
                float effectiveSpeed = (speed > 0) ? speed : _snakeSpeed;

                StartCoroutine(SnakeExitRoutine(data, effectiveSpeed, () =>
                {
                    if (data.RootObject != null) Destroy(data.RootObject);
                }));
            }
            else
            {
                if (data.RootObject != null) Destroy(data.RootObject);
            }
        }
    }

    public void RenderLevel(LevelModel level)
    {
        ClearView();
        foreach (var arrowModel in level.Arrows.Values)
        {
            RenderArrow(arrowModel);
        }
    }



    private void RenderArrow(ArrowModel arrow)
    {
        GameObject lineObj = Instantiate(_arrowPrefab, transform);
        lineObj.name = $"Visual_Arrow_{arrow.Id}";

        LineRenderer lr = lineObj.GetComponent<LineRenderer>();

        // Будуємо шлях
        List<Vector3> worldPositions = new List<Vector3>();
        ArrowPoint current = arrow.StartPoint;
        worldPositions.Add(GridToWorld(current.GridPosition));

        while (current.Next != null)
        {
            AddIntermediatePoints(worldPositions, current.GridPosition, current.Next.GridPosition);
            current = current.Next;
        }

        // Застосовуємо точки
        lr.positionCount = worldPositions.Count;
        lr.SetPositions(worldPositions.ToArray());

        // Створюємо голову
        GameObject headObj = RenderHead(arrow, lineObj.transform);

        // Зберігаємо дані для майбутньої анімації
        ArrowVisualData visualData = new ArrowVisualData
        {
            RootObject = lineObj,
            LineRenderer = lr,
            HeadTransform = headObj.transform,
            FullPath = new List<Vector3>(worldPositions) // Копіюємо список
        };

        _visualArrows.Add(arrow.Id, visualData);
    }

    private GameObject RenderHead(ArrowModel arrow, Transform parent)
    {
        if (arrow.EndPoint == null || arrow.EndPoint.Prev == null) return null;

        Vector3 endPos = GridToWorld(arrow.EndPoint.GridPosition);
        Vector3 prevPos = GridToWorld(arrow.EndPoint.Prev.GridPosition);
        Vector3 dir = (endPos - prevPos).normalized;

        // Визначаємо кут (твоя логіка)
        float zAngle = 0f;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) zAngle = (dir.x > 0) ? -90f : -270f;
        else zAngle = (dir.y > 0) ? 0f : 180f;

        Quaternion rotation = Quaternion.Euler(0, 0, zAngle);
        return Instantiate(_headPrefab, endPos, rotation, parent);
    }

    // -----------------------------------------------------------------------
    // ЛОГІКА АНІМАЦІЇ (ЗМІЙКА)
    // -----------------------------------------------------------------------
    private IEnumerator SnakeExitRoutine(ArrowVisualData data, float speed, System.Action onComplete)
    {
        // ... (підготовка шляху, так само як було) ...
        List<Vector3> extendedPath = new List<Vector3>(data.FullPath);
        if (extendedPath.Count >= 2)
        {
            Vector3 last = extendedPath[extendedPath.Count - 1];
            Vector3 prev = extendedPath[extendedPath.Count - 2];
            Vector3 dir = (last - prev).normalized;
            extendedPath.Add(last + dir * _offScreenDistance);
        }

        float snakeLength = GetPathLength(data.FullPath);
        float totalDist = snakeLength + _offScreenDistance;
        float currentDist = 0f;

        while (currentDist < totalDist)
        {
            // ВИКОРИСТОВУЄМО ПЕРЕДАНУ ШВИДКІСТЬ 'speed' ЗАМІСТЬ '_snakeSpeed'
            currentDist += Time.deltaTime * speed;

            float tail = currentDist;
            float head = currentDist + snakeLength;

            UpdateLineRendererByPath(data.LineRenderer, extendedPath, tail, head);

            if (head < GetPathLength(extendedPath))
            {
                UpdateHeadPosition(data.HeadTransform, extendedPath, head);
            }
            else
            {
                data.HeadTransform.gameObject.SetActive(false);
            }

            yield return null;
        }

        onComplete?.Invoke();
    }

    // Бере шматок шляху і записує його в LineRenderer
    private void UpdateLineRendererByPath(LineRenderer lr, List<Vector3> path, float startDist, float endDist)
    {
        List<Vector3> newPositions = new List<Vector3>();
        float traveled = 0f;

        // Знаходимо точку початку (Хвіст)
        newPositions.Add(GetPointOnPath(path, startDist));

        // Додаємо всі проміжні точки шляху, які потрапляють у діапазон
        for (int i = 0; i < path.Count - 1; i++)
        {
            float segLen = Vector3.Distance(path[i], path[i + 1]);

            // Якщо кінець сегмента знаходиться далі хвоста, і початок сегмента раніше голови
            if (traveled + segLen > startDist && traveled < endDist)
            {
                // Додаємо вершину path[i+1], якщо вона всередині нашої змійки
                if (traveled + segLen < endDist)
                {
                    newPositions.Add(path[i + 1]);
                }
            }
            traveled += segLen;
        }

        // Знаходимо точку кінця (Голова) - вона ж остання точка лінії
        // (Але малюємо лінію трохи коротшою, щоб вона не вилазила з-під спрайта голови, якщо треба)
        newPositions.Add(GetPointOnPath(path, endDist));

        lr.positionCount = newPositions.Count;
        lr.SetPositions(newPositions.ToArray());
    }

    private void UpdateHeadPosition(Transform head, List<Vector3> path, float distance)
    {
        // Позиція
        Vector3 pos = GetPointOnPath(path, distance);
        head.position = pos;

        // Поворот (дивимось трохи вперед по шляху, щоб знати куди повертати)
        Vector3 lookAheadPos = GetPointOnPath(path, distance + 0.1f);
        Vector3 dir = (lookAheadPos - pos).normalized;

        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            // Корекція кута залежно від орієнтації спрайта (зазвичай -90 або +90)
            // Тут використовуємо твою логіку орієнтації, або просто поворот по Z
            head.rotation = Quaternion.Euler(0, 0, angle - 90); // -90 якщо спрайт стрілки дивиться вгору
        }
    }

    // Математика: Отримати точку на ламаній лінії за дистанцією від початку
    private Vector3 GetPointOnPath(List<Vector3> path, float distance)
    {
        if (distance <= 0) return path[0];

        float currentDist = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            float segLen = Vector3.Distance(path[i], path[i + 1]);
            if (currentDist + segLen >= distance)
            {
                float t = (distance - currentDist) / segLen;
                return Vector3.Lerp(path[i], path[i + 1], t);
            }
            currentDist += segLen;
        }
        return path[path.Count - 1]; // Якщо дистанція більша за шлях
    }

    private float GetPathLength(List<Vector3> path)
    {
        float dist = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            dist += Vector3.Distance(path[i], path[i + 1]);
        }
        return dist;
    }

    // -----------------------------------------------------------------------

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
        if (_gridView != null && _gridView.PointPositions != null)
        {
            int x = (int)gridPos.x;
            int y = (int)gridPos.y;
            if (y >= 0 && y < _gridView.PointPositions.GetLength(0) &&
                x >= 0 && x < _gridView.PointPositions.GetLength(1))
            {
                Vector2 worldPos = _gridView.PointPositions[y, x];
                return new Vector3(worldPos.x, worldPos.y, 0);
            }
        }
        return Vector3.zero;
    }
}