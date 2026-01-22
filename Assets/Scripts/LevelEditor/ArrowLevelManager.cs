using ArrowPuzzle;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ArrowLevelManager : MonoBehaviour
{
    [Header("Налаштування генерації")]
    public int width = 6;  // Columns (j)
    public int height = 8; // Rows (i)
    public int minLength = 2;
    public int maxLength = 8;
    [Range(0f, 1f)] public float turnChance = 0.6f;

    [Header("Посилання")]
    [SerializeField] private LevelView _levelView;

    [Header("Налаштування камери")]
    [SerializeField] private Camera _camera; // Перетягніть сюди Main Camera
    [SerializeField] private float _padding = 1.5f; // Відступ від країв екрану

    [Header("Збереження")]
    public string saveFileName = "level_1";

    // ДОДАЙТЕ ЦЕ ПОСИЛАННЯ В ІНСПЕКТОРІ:
    [SerializeField] private GridView _gridView;

    private LevelModel _currentLevel;
    private LevelGenerator _generator;

    private void Awake()
    {
        if (_camera == null) _camera = Camera.main;
    }

    void Start()
    {
        _generator = new LevelGenerator();
    }

    [ContextMenu("Generate New Level")]
    public void Generate()
    {
        if (_levelView == null || _gridView == null)
        {
            Debug.LogError("ArrowLevelManager: Не призначено LevelView або GridView!");
            return;
        }

        // КРОК 1: Перебудовуємо візуальну сітку під нові розміри
        // (Rows = height, Cols = width)
        _gridView.Init(height, width);

        // КРОК 2: Генеруємо логіку рівня
        if (_generator == null) _generator = new LevelGenerator();
        _currentLevel = _generator.GenerateLevelModel(width, height, minLength, maxLength, turnChance);

        // КРОК 3: Малюємо стрілки
        _levelView.RenderLevel(_currentLevel);
        FitCameraToLevel();
        Debug.Log($"Рівень {_currentLevel.Width}x{_currentLevel.Height} згенеровано.");
    }

    // ... решта коду Update, ProcessClick, HandleCellClick ...
    // (вона залишається без змін, бо використовує _currentLevel, який ми щойно оновили)

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) ProcessClick();
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveCurrentLevel();
        }

        // НОВЕ: Натисни 'G', щоб згенерувати новий (для швидкого пошуку крутого рівня)
        if (Input.GetKeyDown(KeyCode.G))
        {
            Generate();
        }
    }

    public void SaveCurrentLevel()
    {
        if (_currentLevel == null)
        {
            Debug.LogError("Немає рівня для збереження!");
            return;
        }

        // 1. Конвертуємо рівень у текст JSON
        string json = LevelLoader.SaveToJSON(_currentLevel);

        // 2. Визначаємо шлях до папки Resources/Levels
        // Використовуємо Application.dataPath, щоб зберегти прямо в папку проекту Unity
        string path = Path.Combine(Application.dataPath, "Resources/Levels");

        // Створюємо папку, якщо її немає
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        // 3. Формуємо повний шлях до файлу
        string fullPath = Path.Combine(path, saveFileName + ".json");

        // 4. Записуємо файл
        File.WriteAllText(fullPath, json);

        Debug.Log($"✅ Рівень збережено! Шлях: {fullPath}");

#if UNITY_EDITOR
        // Оновлюємо базу ассетів Unity, щоб файл одразу з'явився у вікні Project
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    private void ProcessClick()
    {
        if (_currentLevel == null) return;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            var cell = hit.collider.GetComponent<GridCell>();
            if (cell != null)
            {
                HandleCellClick(cell.X, cell.Y);
            }
        }
    }

    private void HandleCellClick(int x, int y)
    {
        int arrowId = _currentLevel.GetArrowIdAt(x, y);
        if (arrowId <= 0) return;

        if (_currentLevel.CanArrowFlyAway(arrowId))
        {
            Debug.Log($"Стрілка {arrowId} полетіла!");
            _currentLevel.RemoveArrow(arrowId);
            _levelView.RemoveVisualArrow(arrowId);
            CheckWin();
        }
        else
        {
            Debug.Log($"Стрілка {arrowId} заблокована!");
        }
    }

    private void CheckWin()
    {
        if (_currentLevel.Arrows.Count == 0)
        {
            Debug.Log("Рівень пройдено!");
            // Invoke("Generate", 1f); // Авто-рестарт
        }
    }

    private void FitCameraToLevel()
    {
        if (_camera == null) return;

        float cellSize = _gridView.CellSize;
        Vector3 startPos = _gridView.StartPosition;

        // 1. Розрахунок розмірів сітки у світових одиницях
        float gridWorldWidth = width * cellSize;
        float gridWorldHeight = height * cellSize;

        // 2. Розрахунок центру сітки
        // X: зміщуємося від старту вправо на половину ширини, але враховуємо, що позиція клітинки - це її центр (або край, залежно від реалізації).
        // У вашому GridView позиція - це центр об'єкта.
        // Центр масиву точок: (Start + End) / 2
        // StartX (col 0) = startPos.x
        // EndX (col w-1) = startPos.x + (width - 1) * cellSize
        float centerX = startPos.x + (width - 1) * cellSize / 2.0f;
        float centerY = startPos.y - (height - 1) * cellSize / 2.0f; // Y йде вниз

        // Переміщуємо камеру в центр (зберігаємо Z)
        _camera.transform.position = new Vector3(centerX, centerY, _camera.transform.position.z);

        // 3. Розрахунок зуму (Orthographic Size)
        // OrthographicSize — це половина висоти екрану в одиницях Unity.

        // Потрібна висота + відступи
        float targetHeight = gridWorldHeight / 2.0f + _padding;

        // Потрібна ширина + відступи (переведена у висоту через Aspect Ratio)
        float aspect = _camera.aspect;
        float targetWidth = (gridWorldWidth / 2.0f + _padding) / aspect;

        // Вибираємо більше значення, щоб сітка влізла і по ширині, і по висоті
        _camera.orthographicSize = Mathf.Max(targetHeight, targetWidth);
    }
}