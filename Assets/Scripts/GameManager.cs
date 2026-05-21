using UnityEngine;

public enum GameState
{
    Playing,
    Editing
}

public class GameManager : MonoBehaviour
{
    public GameObject editorUIContainer;
    public GameObject editModeButton;
    public GameObject playModeButton;
    public GameObject backButton;
    public MainMenu mainMenu;
    public GameObject selector;
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SwitchToEditingMode();
    }

    public void SwitchToEditingMode()
    {
        CurrentState = GameState.Editing;

        if (PlayerController.Instance != null)
            PlayerController.Instance.enabled = false;

        if (RuleManager.Instance != null)
            RuleManager.Instance.enabled = false;

        if (LevelLoader.Instance != null)
            LevelLoader.Instance.ClearCurrentLevel();

        if (editorUIContainer != null)
            editorUIContainer.SetActive(true);
        if (editModeButton != null)
            editModeButton.SetActive(true);
        if (playModeButton != null)
            playModeButton.SetActive(true);
        selector.SetActive(false);
    }

    public void SwitchToPlayingMode()
    {
        CurrentState = GameState.Playing;
        if (RuleManager.Instance != null)
        {
            RuleManager.Instance.enabled = true;
            RuleManager.Instance.RefreshAllRules();
        }

        if (PlayerController.Instance != null)
            PlayerController.Instance.enabled = true;

        if (editorUIContainer != null)
            editorUIContainer.SetActive(false);
        if (editModeButton != null)
            editModeButton.SetActive(true);
        if (playModeButton != null)
            playModeButton.SetActive(true);
      
    }
    
    public void ShowSelector()
    {
        selector.SetActive(true);
    }
    public void BackToMainMenu()
    {
        mainMenu.mainMenuCanvas.SetActive(true);
        mainMenu.gameCanvas.SetActive(false);
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}