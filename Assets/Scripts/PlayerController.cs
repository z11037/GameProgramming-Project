using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Move Settings")]
    public float moveGap = 0.25f;
    public float longPressThreshold = 0.15f;

    private bool isMoving;
    private float longPressTimer;
    private Vector2Int lastMoveDir;
    private bool hasWon;
    private bool pendingWin;

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

        if (allArrived && pendingWin)
        {
            HandleWin();
            return;
        }

        if (!allArrived) return;

        if (!hasWon && LevelManager.Instance.CheckWinCondition())
        {
            HandleWin();
            return;
        }

        Vector2Int dir = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.W)) dir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S)) dir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A)) dir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D)) dir = Vector2Int.right;
        else if (Input.GetKey(KeyCode.W)) dir = Vector2Int.up;
        else if (Input.GetKey(KeyCode.S)) dir = Vector2Int.down;
        else if (Input.GetKey(KeyCode.A)) dir = Vector2Int.left;
        else if (Input.GetKey(KeyCode.D)) dir = Vector2Int.right;

        if (dir != Vector2Int.zero)
        {
            if (dir != lastMoveDir)
            {
                longPressTimer = 0f;
                lastMoveDir = dir;
            }

            bool shouldMove = false;
            if (Input.GetKeyDown(dir == Vector2Int.up ? KeyCode.W :
                                 dir == Vector2Int.down ? KeyCode.S :
                                 dir == Vector2Int.left ? KeyCode.A : KeyCode.D))
            {
                shouldMove = true;
                longPressTimer = 0f;
            }
            else
            {
                longPressTimer += Time.deltaTime;
                if (longPressTimer >= longPressThreshold && !isMoving)
                {
                    shouldMove = true;
                    longPressTimer = 0f;
                }
            }

            if (shouldMove)
                TryMoveAllPlayers(dir);
        }
        else
        {
            longPressTimer = 0f;
            lastMoveDir = Vector2Int.zero;
        }
    }

    private void TryMoveAllPlayers(Vector2Int dir)
    {
        if (isMoving || dir == Vector2Int.zero) return;

        var allYouObjects = LevelManager.Instance.GetAllYouObjects();
        if (allYouObjects.Count == 0) return;

        var sortedYou = LevelManager.Instance.SortObjectsByDirection(allYouObjects, dir);
        HashSet<GridObject> movedThisTurn = new HashSet<GridObject>();
        bool anyMoved = false;

        foreach (var you in sortedYou)
        {
            if (LevelManager.Instance.MoveObject(you, dir, movedThisTurn))
                anyMoved = true;
        }

        if (anyMoved)
        {
            RuleManager.Instance.RefreshAllRules();

            if (LevelManager.Instance.CheckWinCondition())
            {
                pendingWin = true;
                return;
            }

            isMoving = true;
            Invoke(nameof(MoveEnd), moveGap);
        }
    }

    private void HandleWin()
    {
        hasWon = true;
        pendingWin = false;
        Time.timeScale = 0f;
        foreach (var obj in LevelManager.Instance.GetAllObjects())
        {
            if (obj != null && obj.TryGetComponent<SpriteRenderer>(out var sr))
                sr.color = Color.green;
        }
        Debug.Log("LEVEL COMPLETE! Press R to restart.");
        enabled = false;
    }

    void MoveEnd()
    {
        isMoving = false;
    }

    public void ResetMoveLock()
    {
        isMoving = false;
        longPressTimer = 0f;
        lastMoveDir = Vector2Int.zero;
        CancelInvoke(nameof(MoveEnd));
    }

    public void ResetWinState()
    {
        hasWon = false;
        pendingWin = false;
    }
}