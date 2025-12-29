using System.Collections.Generic;
using UnityEngine;

public class LevelModel
{
    public int Width;  // Кількість стовпців (Columns)
    public int Height; // Кількість рядків (Rows)

    // Зберігаємо як [Row, Col] -> [i, j] -> [y, x]
    public int[,] OccupiedGrid;

    public Dictionary<int, ArrowModel> Arrows = new Dictionary<int, ArrowModel>();

    public void AddArrow(ArrowModel arrow)
    {
        if (!Arrows.ContainsKey(arrow.Id))
        {
            Arrows.Add(arrow.Id, arrow);
        }
    }

    public bool CanArrowFlyAway(int arrowId)
    {
        if (!Arrows.TryGetValue(arrowId, out ArrowModel arrow)) return false;

        Vector2Int headPos = arrow.EndPoint.GridPosition;
        Vector2Int prevPos = arrow.EndPoint.Prev.GridPosition;
        Vector2Int direction = headPos - prevPos;

        Vector2Int checkPos = headPos + direction;

        while (IsInsideGrid(checkPos))
        {
            // Звертаємось: [y, x] (тому що Grid[Row, Col])
            int cellValue = OccupiedGrid[checkPos.y, checkPos.x];

            if (cellValue > 0) return false;

            checkPos += direction;
        }

        return true;
    }

    public void RemoveArrow(int arrowId)
    {
        if (!Arrows.TryGetValue(arrowId, out ArrowModel arrow)) return;

        ArrowPoint current = arrow.StartPoint;
        while (current != null)
        {
            // Очищаємо: [y, x]
            OccupiedGrid[current.GridPosition.y, current.GridPosition.x] = 0;
            current = current.Next;
        }

        Arrows.Remove(arrowId);
    }

    public bool IsInsideGrid(Vector2Int pos)
    {
        // X - це стовпець (0..Width-1), Y - це рядок (0..Height-1)
        return pos.x >= 0 && pos.x < Width && pos.y >= 0 && pos.y < Height;
    }

    public int GetArrowIdAt(int x, int y)
    {
        if (IsInsideGrid(new Vector2Int(x, y)))
        {
            // Повертаємо значення з рядка Y, стовпця X
            return OccupiedGrid[y, x];
        }
        return -1;
    }
}