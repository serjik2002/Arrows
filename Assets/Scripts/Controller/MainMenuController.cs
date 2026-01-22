using UnityEngine;
using UnityEngine.SceneManagement;
using options;

public class MainMenuController : MonoBehaviour
{
    public void OnPlayButtonClick()
    {
        
        SceneManager.LoadScene("Game");
    }
}
