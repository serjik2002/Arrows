using System.Collections.Generic;
using UnityEngine;


public static class LevelLoader
{
    public static LevelModel LoadFromJSON(string jsonText)
    {
        if (string.IsNullOrEmpty(jsonText)) return null;

        LevelDataDTO dto = JsonUtility.FromJson<LevelDataDTO>(jsonText);

        LevelModel model = new LevelModel();
        model.Width = dto.width;
        model.Height = dto.height;

        // Ініціалізуємо як [Rows, Cols] -> [Height, Width]
        model.OccupiedGrid = new int[dto.height, dto.width];

        foreach (var arrowData in dto.arrows)
        {
            ArrowModel arrowModel = new ArrowModel();
            arrowModel.Id = arrowData.id;

            List<Vector2Int> points = new List<Vector2Int>();
            for (int k = 0; k < arrowData.cells.Length; k += 2)
            {
                if (k + 1 < arrowData.cells.Length)
                {
                    // JSON зазвичай зберігає [row, col] або [x, y], тут вважаємо що це [y, x] в масиві
                    // Але в коді було: arrowData.cells[i] це row, i+1 це col.
                    int y = arrowData.cells[k];     // Row
                    int x = arrowData.cells[k + 1]; // Col

                    points.Add(new Vector2Int(x, y));

                    if (x >= 0 && x < model.Width && y >= 0 && y < model.Height)
                    {
                        // Записуємо в [Row, Col] -> [y, x]
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