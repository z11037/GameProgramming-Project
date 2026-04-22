using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSmooth = 18f;
    public float moveGap = 0.25f;

    private GridObject playerObj;
    private bool isMoving;

    void Start()
    {
        playerObj = GetComponent<GridObject>();
        if (playerObj == null)
        {
            Debug.LogError("Player missing GridObject component!");
            return;
        }

        transform.position = GridManager.Instance.GridToWorld(playerObj.TargetGridPos);
    }

    void Update()
    {
        if (playerObj == null || LevelManager.Instance == null || GridManager.Instance == null)
            return;

        Vector2 targetWorld = GridManager.Instance.GridToWorld(playerObj.TargetGridPos);
        if (Vector2.Distance(transform.position, targetWorld) > 0.01f)
        {
            transform.position = Vector2.Lerp(transform.position, targetWorld, moveSmooth * Time.deltaTime);
            return;
        }

        Vector2Int dir = Vector2Int.zero;
        if (Input.GetKey(KeyCode.W)) dir = Vector2Int.up;
        if (Input.GetKey(KeyCode.S)) dir = Vector2Int.down;
        if (Input.GetKey(KeyCode.A)) dir = Vector2Int.left;
        if (Input.GetKey(KeyCode.D)) dir = Vector2Int.right;

        if (dir != Vector2Int.zero && !isMoving)
        {
            if (LevelManager.Instance.TryMove(playerObj, dir))
            {
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