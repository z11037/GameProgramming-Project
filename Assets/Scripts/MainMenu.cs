using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Menu")]
    public GameObject mainMenuCanvas;
    public GameObject gameCanvas;

    public void OnStartGame()
    {
        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(false);

        if (gameCanvas != null)
            gameCanvas.SetActive(true);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SwitchToEditingMode();
            GameObject editButton = GameObject.Find("EditModeButton");
            if (editButton != null)
                editButton.SetActive(true);

            GameObject playButton = GameObject.Find("PlayModeButton");
            if (playButton != null)
                playButton.SetActive(true);
        }

    }

    public void OnQuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}