using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [Header("Посилання")]
    [SerializeField] private LevelView _levelView;
    [SerializeField] private GridView _gridView;
    [SerializeField] private InputHandler _inputHandler;

    [Header("Камера")]
    [SerializeField] private Camera _camera; // Перетягни сюди Main Camera
    [SerializeField] private float _padding = 1.5f; // Відступ від країв екрану

    private LevelModel _currentLevel;

    public event Action OnLevelCompleted;

    private void OnEnable()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnGridClick += OnGridClicked;
        }
    }

    private void OnDisable()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnGridClick -= OnGridClicked;
        }
    }


    public void Initialize(LevelModel levelModel)
    {
        _currentLevel = levelModel;
        _gridView.Init(levelModel.Height, levelModel.Width);
        if (levelModel != null)
        {
            _levelView.RenderLevel(levelModel);
            PrintGridToConsole(levelModel);
        }
        FitCameraToGrid(levelModel);
    }

    private void FitCameraToGrid(LevelModel model)
    {
        if (_camera == null || _gridView == null) return;

        float cellSize = _gridView.CellSize;
        Vector3 startPos = _gridView.StartPosition;

        // --- 1. Знаходимо центр сітки ---
        // Ширина сітки в одиницях світу
        float totalWidth = model.Width * cellSize;
        // Висота сітки (Rows)
        float totalHeight = model.Height * cellSize;

        // Центр X: від старту зміщуємося вправо на половину ширини
        // (Мінус половина клітинки, бо координата об'єкта - це його центр, а не край,
        // але для простоти беремо (cols-1))
        float centerX = startPos.x + (model.Width - 1) * cellSize / 2.0f;

        // Центр Y: від старту зміщуємося ВНИЗ (тому мінус)
        // У твоєму GridView (Source 289) Y йде як: start.y - i * size
        float centerY = startPos.y - (model.Height - 1) * cellSize / 2.0f;

        // Ставимо камеру в центр
        _camera.transform.position = new Vector3(centerX, centerY, _camera.transform.position.z);

        // --- 2. Розраховуємо Зум (Orthographic Size) ---
        // OrthographicSize = половина висоти екрану, яку бачить камера.

        // Варіант А: Підганяємо по висоті (Height / 2 + відступи)
        float targetHeight = (totalHeight / 2.0f) + _padding;

        // Варіант Б: Підганяємо по ширині.
        // Оскільки камера налаштовується через висоту, конвертуємо ширину у висоту через Aspect Ratio.
        float aspect = _camera.aspect; // Ширина / Висота
        float targetWidth = ((totalWidth / 2.0f) + _padding) / aspect;

        // Вибираємо більше значення, щоб сітка точно влізла і по ширині, і по висоті
        _camera.orthographicSize = Mathf.Max(targetHeight, targetWidth);
    }

    private void OnGridClicked(GridCoordinate coord)
    {
        HandleCellClick(coord.Column, coord.Row);
        print($"Clicked Cell: X={coord.Column}, Y={coord.Row}");
    }

    private void HandleCellClick(int x, int y)
    {
        int arrowId = _currentLevel.GetArrowIdAt(x, y); // y, x (Row, Col)

        if (arrowId <= 0) return;

        if (_currentLevel.CanArrowFlyAway(arrowId))
        {
            Debug.Log($"Стрілка {arrowId} полетіла!");
            _currentLevel.RemoveArrow(arrowId);
            _levelView.RemoveVisualArrow(arrowId);

            CheckLevelEnd();
        }
        else
        {
            Debug.Log($"Стрілка {arrowId} заблокована!");
            // Тут можна додати анімацію трусіння
        }
    }

    private void CheckLevelEnd()
    {
        if (_currentLevel.Arrows.Count == 0)
        {
            Debug.Log("LEVEL COMPLETE!");

            // Можна вимкнути введення, щоб гравець не клікав під час екрану перемоги
            if (_inputHandler != null) _inputHandler.SetInputActive(false);

            OnLevelCompleted?.Invoke();
        }
    }

    private void PrintGridToConsole(LevelModel model)
    {
        string output = "Level Matrix:\n";

        // СТАНДАРТ: i (рядки), j (стовпці)
        for (int i = 0; i < model.Height; i++)
        {
            for (int j = 0; j < model.Width; j++)
            {
                // Доступ [i, j]
                int id = model.OccupiedGrid[i, j];
                output += (id == 0 ? "." : id.ToString()) + " ";
            }
            output += "\n";
        }
        Debug.Log(output);
    }
}
