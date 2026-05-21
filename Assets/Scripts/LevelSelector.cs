using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelector : MonoBehaviour
{
    public GameObject levelButtonPrefab;
    public Transform levelButtonContainer;
    public GameObject selectorCanvas;
    public GameObject gameCanvas;
    public GameObject mainMenuCanvas;

    private void OnEnable()
    {
        GenerateLevelButtons();
    }

    private void GenerateLevelButtons()
    {
        if (LevelLoader.Instance == null || LevelLoader.Instance.levelsJsonFile == null)
        {
            return;
        }

        LevelDataList allLevels = JsonUtility.FromJson<LevelDataList>(LevelLoader.Instance.levelsJsonFile.text);
        if (allLevels == null || allLevels.levels == null || allLevels.levels.Count == 0)
        {
            return;
        }

        foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < allLevels.levels.Count; i++)
        {
            int levelIndex = i;
            LevelData levelData = allLevels.levels[levelIndex];

            GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonContainer);
            buttonObj.name = $"LevelButton_{levelIndex}";

            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"Level {levelData.levelName}";
            }

            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnLevelSelected(levelIndex));
            }
        }

    }

    public void OnLevelSelected(int levelIndex)
    {
        if (selectorCanvas != null)
            selectorCanvas.SetActive(false);

        if (gameCanvas != null)
            gameCanvas.SetActive(true);

        if (LevelLoader.Instance != null)
            LevelLoader.Instance.LoadLevel(levelIndex);

    }

    public void OnBackToMainMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.BackToMainMenu();
        }
    }
}