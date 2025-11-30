using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [Header("Дані")]
    [SerializeField] private TextAsset _levelJsonFile;

    [Header("Посилання")]
    [SerializeField] private LevelView _levelView; 

    private LevelModel _currentLevel;

    private void Start()
    {

        _currentLevel = LevelLoader.LoadFromJSON(_levelJsonFile.text);

        if (_currentLevel != null)
        {
            _levelView.RenderLevel(_currentLevel);
            PrintGridToConsole(_currentLevel);
        }
    }

    private void Update()
    {
        // Обробка кліку лівою кнопкою
        if (Input.GetMouseButtonDown(0))
        {
            ProcessClick();
        }
    }

    private void ProcessClick()
    {
        if (_currentLevel == null) return;

        // 1. Стріляємо променем (Raycast) в точку мишки
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        // 2. Перевіряємо, чи попали в щось
        if (hit.collider != null)
        {
            // Шукаємо компонент GridCell на об'єкті, в який попали
            GridCell cell = hit.collider.GetComponent<GridCell>();

            if (cell != null)
            {
                HandleCellClick(cell.X, cell.Y);
            }
        }
    }

    private void HandleCellClick(int x, int y)
    {
        // 1. Питаємо у Моделі: Яка стрілка знаходиться в цій клітинці?
        // (Тобі треба додати метод GetArrowIdAt в LevelModel, якщо його немає, див. нижче)
        int arrowId = _currentLevel.GetArrowIdAt(y, x);

        // Якщо клітинка пуста (0) або маска (-1) - ігноруємо
        if (arrowId <= 0) return;

        // 2. Питаємо у Моделі: Чи може ця стрілка полетіти?
        if (_currentLevel.CanArrowFlyAway(arrowId))
        {
            Debug.Log($"Стрілка {arrowId} полетіла!");

            // А. Видаляємо логічно (звільняємо матрицю)
            _currentLevel.RemoveArrow(arrowId);

            // Б. Видаляємо візуально (зі сцени)
            _levelView.RemoveVisualArrow(arrowId);
        }
        else
        {
            Debug.Log($"Стрілка {arrowId} заблокована!");
            // Тут можна додати ефект "трусіння" (Wiggle animation)
        }
    }

    private void PrintGridToConsole(LevelModel model)
    {
        string output = "Level Matrix:\n";

        for (int y = 0; y < model.Height; y++)
        {
            for (int x = 0; x < model.Width; x++)
            {
                int id = model.OccupiedGrid[x, y];
                output += (id == 0 ? "." : id.ToString()) + " ";
            }
            output += "\n";
        }
        Debug.Log(output);
    }
}
