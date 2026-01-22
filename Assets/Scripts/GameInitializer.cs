using options;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private LevelController _levelController;
    private int _currentLevelIndex;

    private void Start()
    {
        int lastCompletedLevel = Options.GetInt("Levels.lastLevel", 0);
        _currentLevelIndex = lastCompletedLevel + 1;

        Debug.Log($"Завантажую рівень: {_currentLevelIndex}");

        TextAsset levelFile = Resources.Load<TextAsset>($"Levels/level_{_currentLevelIndex}");
        if (levelFile != null)
        {
            var levelModel = LevelLoader.LoadFromJSON(levelFile.text);
            _levelController.Initialize(levelModel);

            // 3. Підписуємося на перемогу
            _levelController.OnLevelCompleted += OnLevelWon;
        }
        else
        {
            Debug.LogError("Не знайдено жодного файлу рівня в Resources/Levels/!");
        }
    }


    private void OnLevelWon()
    {
        Debug.Log($"Зберігаємо прогрес: пройдено рівень {_currentLevelIndex}");

        Options.SetInt("Levels.lastLevel", _currentLevelIndex);
        Options.Save();

        SceneManager.LoadScene("MainMenu");


    }
}