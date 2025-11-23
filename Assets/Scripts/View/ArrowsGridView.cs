using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowsGridView : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    [SerializeField] private GridView _gridView;

    [SerializeField] private float _arrowWidth = 0.1f;
    [SerializeField] private int _segmentCount = 10;
    [SerializeField] private float _segmentDuration = 0.2f;

    [SerializeField] private GameObject _headArrowPrefab;

    private Vector2[,] _pointPositions;
    private int _currentPointCount;


    //--------------------------------------------
    private List<float> _segmentLengths; // длина каждого сегмента
    private float _totalPathLength; // общая длина пути

    private float _animationOffset = 0f; // смещение всей стрелки
    private bool _isAnimating = false;
    private ArrowPath _currentPath;


    private void Start()
    {
        _pointPositions = _gridView.PointPositions;
        _lineRenderer = GetComponent<LineRenderer>();

        _currentPath = CreateHardcodedArrow();
        CalculatePathMetrics(_currentPath);
        DrawArrowPath(_currentPath);

        // Для теста - запускаем анимацию через 1 секунду
        Invoke("StartDisappearAnimation", 1f);
    }

    public ArrowPath CreateHardcodedArrow()
    {
        // Створюємо всі точки (x,y в матриці)
        ArrowPoint p0 = new ArrowPoint { position = new Vector2Int(2, 0) };
        ArrowPoint p1 = new ArrowPoint { position = new Vector2Int(2, 1) };
        ArrowPoint p2 = new ArrowPoint { position = new Vector2Int(2, 2) };
        ArrowPoint p3 = new ArrowPoint { position = new Vector2Int(3, 2) };
        ArrowPoint p4 = new ArrowPoint { position = new Vector2Int(4, 2) };
        ArrowPoint p5 = new ArrowPoint { position = new Vector2Int(4, 3) };
        ArrowPoint p6 = new ArrowPoint { position = new Vector2Int(4, 4) };

        // Лінкуємо
        p0.next = p1;

        p1.prev = p0; p1.next = p2;
        p2.prev = p1; p2.next = p3;
        p3.prev = p2; p3.next = p4;
        p4.prev = p3; p4.next = p5;
        p5.prev = p4; p5.next = p6;
        p6.prev = p5;

        return new ArrowPath { startPoint = p0, endPoint = p6 };
    }

    // При отрисовке добавляй промежуточные точки на поворотах
    private void DrawArrowPath(ArrowPath arrowPath)
    {
        List<Vector3> positions = new List<Vector3>();
        ArrowPoint current = arrowPath.startPoint;

        positions.Add(MatrixToWorld(current.position));

        while (current.next != null)
        {
            Vector2 from = current.position;
            Vector2 to = current.next.position;

            // Добавляем промежуточные точки между клетками
            AddIntermediatePoints(positions, from, to);

            current = current.next;
        }

        _lineRenderer.positionCount = positions.Count;
        _lineRenderer.SetPositions(positions.ToArray());
    }

    private void AddIntermediatePoints(List<Vector3> positions, Vector2 from, Vector2 to)
    {
        Vector2 direction = to - from;
        int steps = (int)(Mathf.Abs(direction.x) + Mathf.Abs(direction.y));

        for (int i = 1; i <= steps; i++)
        {
            Vector2 intermediatePos = from + direction * i / steps;
            positions.Add(MatrixToWorld(intermediatePos));
        }
    }

    private void DrawArrowHead(ArrowPath path)
    {
        var postion = path.startPoint.position;
        var direction = path.startPoint.next.position - path.startPoint.position;
        var rotation = RotationToDirection(direction);
        Instantiate(_headArrowPrefab, MatrixToWorld(postion), rotation, transform);
    }

    private Quaternion RotationToDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0f, 0f, angle);
    }

    private Vector3 MatrixToWorld(Vector2 matrixPos)
    {
        Vector2 worldPos = _pointPositions[(int)matrixPos.x, (int)matrixPos.y];
        return new Vector3(worldPos.x, worldPos.y, 0f);
    }


    private void CalculatePathMetrics(ArrowPath path)
    {
        _segmentLengths = new List<float>();
        _totalPathLength = 0f;

        ArrowPoint current = path.startPoint;
        while (current.next != null)
        {
            Vector3 start = MatrixToWorld(current.position);
            Vector3 end = MatrixToWorld(current.next.position);
            float length = Vector3.Distance(start, end);

            _segmentLengths.Add(length);
            _totalPathLength += length;
            current = current.next;
        }
    }

    private Vector3 GetPositionAtDistance(ArrowPath path, float distance)
    {
        if (distance > _totalPathLength)
        {
            // За пределами пути - продолжаем в последнем направлении
            Vector3 lastPos = MatrixToWorld(path.endPoint.position);
            Vector2 lastDir = path.endPoint.position - path.endPoint.prev.position;

            // Направление строго по оси
            Vector3 worldDir = new Vector3(lastDir.x, lastDir.y, 0).normalized;
            return lastPos + worldDir * (distance - _totalPathLength);
        }

        float accumulated = 0f;
        ArrowPoint current = path.startPoint;
        int segmentIndex = 0;

        while (current.next != null)
        {
            float segmentLength = _segmentLengths[segmentIndex];

            if (distance <= accumulated + segmentLength)
            {
                // Интерполируем только по одной оси
                float t = (distance - accumulated) / segmentLength;
                Vector3 start = MatrixToWorld(current.position);
                Vector3 end = MatrixToWorld(current.next.position);

                // Определяем направление движения
                Vector2 gridDir = current.next.position - current.position;

                if (gridDir.x != 0)
                {
                    // Движение по оси X
                    return new Vector3(
                        Mathf.Lerp(start.x, end.x, t),
                        start.y,  // Y не меняется!
                        0f
                    );
                }
                else
                {
                    // Движение по оси Y
                    return new Vector3(
                        start.x,  // X не меняется!
                        Mathf.Lerp(start.y, end.y, t),
                        0f
                    );
                }
            }

            accumulated += segmentLength;
            current = current.next;
            segmentIndex++;
        }

        return MatrixToWorld(path.endPoint.position);
    }


    public void StartDisappearAnimation()
    {
        _isAnimating = true;
        _animationOffset = 0f;
    }

    private void Update()
    {
        if (_isAnimating)
        {
            AnimateDisappear();
        }
    }

    private void AnimateDisappear()
    {
        float speed = 2f;
        float movement = speed * Time.deltaTime;

        // Сохраняем старые позиции
        Vector3[] oldPositions = new Vector3[_lineRenderer.positionCount];
        for (int i = 0; i < _lineRenderer.positionCount; i++)
        {
            oldPositions[i] = _lineRenderer.GetPosition(i);
        }

        // 1. Двигаем голову в её направлении (строго по оси)
        Vector3 headPos = oldPositions[0];
        Vector2 headDir = _currentPath.startPoint.next.position - _currentPath.startPoint.position;

        if (headDir.x != 0)
        {
            headPos.x += Mathf.Sign(headDir.x) * movement;
        }
        else if (headDir.y != 0)
        {
            headPos.y += Mathf.Sign(headDir.y) * movement;
        }

        _lineRenderer.SetPosition(0, headPos);

        // 2. Каждая следующая точка движется к предыдущей (строго по осям)
        for (int i = 1; i < _lineRenderer.positionCount; i++)
        {
            Vector3 currentPos = oldPositions[i];
            Vector3 targetPos = oldPositions[i - 1];

            Vector3 diff = targetPos - currentPos;

            // Двигаемся только по одной оси - по той, где разница больше
            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
            {
                // Движение по X
                currentPos.x += Mathf.Sign(diff.x) * movement;
            }
            else
            {
                // Движение по Y
                currentPos.y += Mathf.Sign(diff.y) * movement;
            }

            _lineRenderer.SetPosition(i, currentPos);
        }

        // Обновляем голову стрелки
        Transform head = transform.GetChild(0);
        head.position = _lineRenderer.GetPosition(0);
    }

    private void UpdateArrowHead()
    {
        // Голова двигается вместе с первой точкой LineRenderer
        Transform head = transform.GetChild(0); // предполагаем что голова первый child
        head.position = _lineRenderer.GetPosition(0);
    }



}



public class ArrowPoint
{
    public Vector2 position;
    public ArrowPoint prev;
    public ArrowPoint next;
}

public class ArrowPath
{
    public ArrowPoint startPoint;
    public ArrowPoint endPoint;
}