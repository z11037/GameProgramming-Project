using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    [Header("MoveSetting")]
    public float moveGap = 0.25f;
    public float longPressThreshold = 0.15f;
    private bool isMoving;
    private float longPressTimer;
    private Vector2Int lastMoveDir;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    void Start()
    {
        Invoke(nameof(InitialRuleRefresh), 0.5f);
    }

    void InitialRuleRefresh()
    {
        RuleManager.Instance.RefreshAllRules();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            LevelManager.Instance.ResetLevel();
            return;
        }

        if (LevelManager.Instance == null || RuleManager.Instance == null)
            return;

        var allYouObjects = LevelManager.Instance.GetAllYouObjects();
        if (allYouObjects.Count == 0) return;

        bool allArrived = true;
        foreach (var youObj in allYouObjects)
        {
            Vector2 targetWorld = GridManager.Instance.GridToWorld(youObj.TargetGridPos);
            if (Vector2.Distance(youObj.transform.position, targetWorld) > 0.01f)
            {
                allArrived = false;
                break;
            }
        }
        if (!allArrived) return;

        if (LevelManager.Instance.CheckWinCondition())
        {
            Debug.Log("LEVEL COMPLETE!");
            Time.timeScale = 0f;
            foreach (var obj in LevelManager.Instance.GetAllObjects())
            {
                if (obj.TryGetComponent<SpriteRenderer>(out var sr))
                {
                    sr.color = Color.green;
                }
            }
            Invoke(nameof(LoadNextLevelDelayed), 0.5f);
            enabled = false;
            return;
        }

        Vector2Int dir = Vector2Int.zero;
        if (Input.GetKey(KeyCode.W)) dir = Vector2Int.up;
        if (Input.GetKey(KeyCode.S)) dir = Vector2Int.down;
        if (Input.GetKey(KeyCode.A)) dir = Vector2Int.left;
        if (Input.GetKey(KeyCode.D)) dir = Vector2Int.right;

        if(dir != Vector2Int.zero)
        {
            if (Input.GetKeyDown(dir == Vector2Int.up ? KeyCode.W :
                dir == Vector2Int.down ? KeyCode.S :
                dir == Vector2Int.left ? KeyCode.A : KeyCode.D))
            {
                longPressTimer = 0;
                lastMoveDir = dir;
                TryMoveAllPlayers(dir);
            }
            else
            {
                longPressTimer += Time.deltaTime;
                if (longPressTimer >= longPressThreshold && dir == lastMoveDir && !isMoving)
                {
                    TryMoveAllPlayers(dir);
                    longPressTimer = 0;
                }
            }
        }
        else
        {
            longPressTimer = 0;
            lastMoveDir = Vector2Int.zero;
        }
    }

    private void TryMoveAllPlayers(Vector2Int dir)
    {
        if (isMoving || dir == Vector2Int.zero) return;

        var allYouObjects = LevelManager.Instance.GetAllYouObjects();
        bool moveSuccess = true;

        foreach (var youObj in allYouObjects)
        {
            Vector2Int targetPos = youObj.TargetGridPos + dir;
            if (Mathf.Abs(targetPos.x) > 20 || Mathf.Abs(targetPos.y) > 20)
            {
                moveSuccess = false;
                break;
            }
        }

        if (!moveSuccess) return;
        foreach (var youObj in allYouObjects)
        {
            if (!LevelManager.Instance.TryMove(youObj, dir))
            {
                moveSuccess = false;
                break;
            }
        }
        if (moveSuccess)
        {
            RuleManager.Instance.RefreshAllRules();
            isMoving = true;
            Invoke(nameof(MoveEnd), moveGap);
        }
    }

    void LoadNextLevelDelayed()
    {
        LevelLoader.Instance.LoadNextLevel();
    }

    void MoveEnd()
    {
        isMoving = false;
    }
}