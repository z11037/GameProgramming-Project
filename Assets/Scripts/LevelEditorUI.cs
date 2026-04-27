using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelEditorUI : MonoBehaviour
{
    [Header("Change Mode")]
    public Button editModeBtn;
    public Button playModeBtn;

    [Header("Objects Selection")]
    public Transform objectSelectionParent;
    public GameObject selectionBtnPrefab;

    [Header("Operations")]
    public Button saveLevelBtn;
    public Button loadLevelBtn;
    public Button testLevelBtn;

    private void Start()
    {
        if (GameManager.Instance == null || LevelEditorManager.Instance == null)
        {
            Debug.LogError("GameManager/LevelEditorManager 单例未找到！");
            return;
        }

        editModeBtn.onClick.AddListener(GameManager.Instance.SwitchToEditingMode);
        playModeBtn.onClick.AddListener(GameManager.Instance.SwitchToPlayingMode);

        saveLevelBtn.onClick.AddListener(LevelEditorManager.Instance.SaveCurrentLevel);
        loadLevelBtn.onClick.AddListener(LevelEditorManager.Instance.LoadCustomLevel);
        testLevelBtn.onClick.AddListener(LevelEditorManager.Instance.TestEditedLevel);

        GenerateSelectionButtons();
    }

    private void GenerateSelectionButtons()
    {
        if (selectionBtnPrefab == null || objectSelectionParent == null)
        {
            return;
        }

        foreach (var prefab in LevelEditorManager.Instance.editableObjectPrefabs)
        {
            if (prefab != null)
            {
                CreateSelectionButton(prefab);
            }
        }

        foreach (var prefab in LevelEditorManager.Instance.editableTextPrefabs)
        {
            if (prefab != null)
            {
                CreateSelectionButton(prefab);
            }
        }

        Destroy(selectionBtnPrefab);
    }

    private void CreateSelectionButton(GameObject prefab)
    {
        GameObject btnObj = Instantiate(selectionBtnPrefab, objectSelectionParent);
        Button btn = btnObj.GetComponent<Button>();
        TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();

        if (btnText != null)
        {
            btnText.text = prefab.name;
        }

        btn.onClick.AddListener(() =>
        {
            LevelEditorManager.Instance.SelectPrefab(prefab);
            Debug.Log($"已选择物体：{prefab.name}");
        });
    }
}