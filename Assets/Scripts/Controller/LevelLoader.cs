using System.Collections.Generic;
using UnityEngine;

public static class LevelLoader
{
    public static LevelModel LoadFromJSON(string jsonText)
    {
        if (string.IsNullOrEmpty(jsonText)) return null;

        // 1. Парсинг DTO
        LevelDataDTO dto = JsonUtility.FromJson<LevelDataDTO>(jsonText);

        // 2. Створення Моделі
        LevelModel model = new LevelModel();
        model.Width = dto.width;
        model.Height = dto.height;
        model.OccupiedGrid = new int[dto.width, dto.height];

        // 3. Конвертація стрілок
        foreach (var arrowData in dto.arrows)
        {
            ArrowModel arrowModel = new ArrowModel();
            arrowModel.Id = arrowData.id;

            List<Vector2Int> points = new List<Vector2Int>();
            for (int i = 0; i < arrowData.cells.Length; i += 2)
            {
                if (i + 1 < arrowData.cells.Length)
                {
                    // Міняємо місцями Row/Col -> X/Y
                    int x = arrowData.cells[i + 1];
                    int y = arrowData.cells[i];

                    points.Add(new Vector2Int(x, y));

                    // Заповнюємо матрицю
                    if (x >= 0 && x < model.Width && y >= 0 && y < model.Height)
                    {
                        model.OccupiedGrid[y, x] = arrowData.id;
                    }
                }
            }

            if (points.Count > 0)
            {
                ArrowPoint first = new ArrowPoint { GridPosition = points[0] };
                ArrowPoint current = first;

                for (int i = 1; i < points.Count; i++)
                {
                    ArrowPoint nextP = new ArrowPoint { GridPosition = points[i] };
                    current.Next = nextP;
                    nextP.Prev = current;
                    current = nextP;
                }

                arrowModel.StartPoint = first;
                arrowModel.EndPoint = current;

                model.AddArrow(arrowModel);
            }
        }

        return model;
    }
}