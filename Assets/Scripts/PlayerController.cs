using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSmooth = 18f;
    public float moveGap = 0.25f;
    private bool isMoving;

    void Update()
    {
        // 1. 基础安全检查
        if (LevelManager.Instance == null || RuleManager.Instance == null || GridManager.Instance == null)
            return;

        // 2. 获取所有可控制的玩家物体
        var allYouObjects = LevelManager.Instance.GetAllYouObjects();
        if (allYouObjects.Count == 0) return;

        // 3. 等待所有玩家物体都到达目标位置
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
            // ========== 新增：简单的视觉反馈 ==========
            // 1. 暂停游戏
            Time.timeScale = 0f;
            // 2. 可以在这里加通关弹窗、粒子特效、场景跳转等
            // 示例：改变所有物体颜色表示通关
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

        // 5. 输入检测
        Vector2Int dir = Vector2Int.zero;
        if (Input.GetKey(KeyCode.W)) dir = Vector2Int.up;
        if (Input.GetKey(KeyCode.S)) dir = Vector2Int.down;
        if (Input.GetKey(KeyCode.A)) dir = Vector2Int.left;
        if (Input.GetKey(KeyCode.D)) dir = Vector2Int.right;

        // 6. 触发移动 + 刷新规则
        if (dir != Vector2Int.zero && !isMoving)
        {
            bool moveSuccess = true;

            // ========== 修复1：移动所有isYou的物体，而不是只移动单个playerObj ==========
            foreach (var youObj in allYouObjects)
            {
                if (!LevelManager.Instance.TryMove(youObj, dir))
                {
                    moveSuccess = false;
                    break;
                }
            }

            // ========== 修复2：移动成功后，必须刷新规则！ ==========
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