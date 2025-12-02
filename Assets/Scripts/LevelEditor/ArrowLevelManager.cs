using ArrowPuzzle;
using System.Collections.Generic;
using UnityEngine;

// Цей скрипт замінює LevelController для процедурної генерації
// Він використовує ваш LevelView та LevelModel для відображення
public class ArrowLevelManager : MonoBehaviour
{
    [Header("Налаштування генерації")]
    public int width = 10;
    public int height = 10;
    public int minLength = 2;
    public int maxLength = 8;
    [Range(0f, 1f)] public float turnChance = 0.6f;
    // Вибір алгоритму прибрано, тепер завжди конструктивний

    [Header("Посилання на ваші компоненти")]
    [SerializeField] private LevelView _levelView;

    // Поточна модель рівня
    private LevelModel _currentLevel;
    private LevelGenerator _generator;

    void Start()
    {
        _generator = new LevelGenerator();
        // Можна згенерувати рівень одразу при старті
        // Generate(); 
    }

    [ContextMenu("Generate New Level")]
    public void Generate()
    {
        if (_levelView == null)
        {
            Debug.LogError("ArrowLevelManager: Не призначено LevelView!");
            return;
        }

        // 1. Генеруємо нову модель за допомогою конструктивного алгоритму
        _currentLevel = _generator.GenerateLevelModel(width, height, minLength, maxLength, turnChance);

        // 2. Передаємо модель у ваш вьювер для відмальовки
        _levelView.RenderLevel(_currentLevel);

        Debug.Log($"Рівень {_currentLevel.Width}x{_currentLevel.Height} згенеровано. Кількість стрілок: {_currentLevel.Arrows.Count}");
    }

    private void Update()
    {
        // Обробка кліку (копія логіки з вашого LevelController)
        if (Input.GetMouseButtonDown(0))
        {
            ProcessClick();
        }
    }

    private void ProcessClick()
    {
        if (_currentLevel == null) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            // Припускаємо, що у вас є компонент GridCell на клітинках
            // Або можна вирахувати координати через GridView, якщо GridCell немає
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
            // Тут можна викликати анімацію тряски через _levelView, якщо там є такий метод
        }
    }

    private void CheckWin()
    {
        if (_currentLevel.Arrows.Count == 0)
        {
            Debug.Log("Рівень пройдено! 🎉");
            // Тут можна автоматично запустити генерацію наступного:
            // Invoke("Generate", 1f);
        }
    }
}