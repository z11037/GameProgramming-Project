using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSmooth = 18f;
    public float moveGap = 0.25f;
    private bool isMoving;

    void Update()
    {
        if (LevelManager.Instance == null || RuleManager.Instance == null || GridManager.Instance == null)
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
            enabled = false;
            return;
        }

        Vector2Int dir = Vector2Int.zero;
        if (Input.GetKey(KeyCode.W)) dir = Vector2Int.up;
        if (Input.GetKey(KeyCode.S)) dir = Vector2Int.down;
        if (Input.GetKey(KeyCode.A)) dir = Vector2Int.left;
        if (Input.GetKey(KeyCode.D)) dir = Vector2Int.right;

        if (dir != Vector2Int.zero && !isMoving)
        {
            bool moveSuccess = true;

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
    }

    void MoveEnd()
    {
        isMoving = false;
    }
}