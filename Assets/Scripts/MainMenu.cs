using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Menu")]
    public GameObject mainMenuCanvas;
    public GameObject gameCanvas;

    public void OnStartGame()
    {
        mainMenuCanvas.SetActive(false);
        gameCanvas.SetActive(true);
        GameManager.Instance.SwitchToEditingMode();
    }

    public void OnQuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}