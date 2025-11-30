using System.Collections.Generic;
using UnityEngine;
// 3. Клас, що описує весь рівень (Контейнер)
public class LevelModel
{
    public int Width;
    public int Height;

    public int[,] OccupiedGrid;

    // Словник для миттєвого доступу до стрілок за ID
    public Dictionary<int, ArrowModel> Arrows = new Dictionary<int, ArrowModel>();

    // Метод, щоб додати стрілку і не думати про ключі словника
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

        // 1. Напрямок польоту
        Vector2Int headPos = arrow.EndPoint.GridPosition;
        Vector2Int prevPos = arrow.EndPoint.Prev.GridPosition;
        Vector2Int direction = headPos - prevPos;

        // 2. Починаємо перевірку з наступної клітинки
        Vector2Int checkPos = headPos + direction;

        // "Промінь" летить вперед
        while (IsInsideGrid(checkPos))
        {
            int cellValue = OccupiedGrid[checkPos.y, checkPos.x];

            // ВАРІАНТ Б: Це інша стрілка (ID > 0). Шлях заблокований.
            if (cellValue > 0)
            {
                return false;
            }

            // ВАРІАНТ В: Це пуста клітинка (0). Летимо далі, перевіряємо наступну.
            checkPos += direction;
        }

        // Ми вийшли за межі масиву (IsInsideGrid == false). Це теж означає "полетіла".
        return true;
    }

    public void RemoveArrow(int arrowId)
    {
        if (!Arrows.TryGetValue(arrowId, out ArrowModel arrow)) return;

        // 1. Проходимо по всіх точках стрілки і ставимо 0 в матриці
        ArrowPoint current = arrow.StartPoint;
        while (current != null)
        {
            OccupiedGrid[current.GridPosition.y, current.GridPosition.x] = 0;
            current = current.Next;
        }

        // 2. Видаляємо стрілку зі словника
        Arrows.Remove(arrowId);
    }

    // Допоміжний метод перевірки меж
    public bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < Width && pos.y >= 0 && pos.y < Height;
    }

    public int GetArrowIdAt(int x, int y)
    {
        // Перевірка меж масиву
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            return OccupiedGrid[x, y];
        }
        return -1; // Повертаємо -1, якщо клікнули повз поле
    }

}
