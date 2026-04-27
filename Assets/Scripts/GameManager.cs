using UnityEngine;

public enum GameState
{
    Playing,
    Editing
}

public class GameManager : MonoBehaviour
{
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
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SwitchToPlayingMode();
    }

    public void SwitchToEditingMode()
    {
        CurrentState = GameState.Editing;
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.enabled = false;
        }
        RuleManager.Instance.enabled = false;
        LevelLoader.Instance.ClearCurrentLevel();
        Debug.Log("Switched to Editing Mode");
    }

    public void SwitchToPlayingMode()
    {
        CurrentState = GameState.Playing;
        RuleManager.Instance.enabled = true;
        RuleManager.Instance.RefreshAllRules();
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.enabled = true;
        }
        Debug.Log("Switched to Playing Mode");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}