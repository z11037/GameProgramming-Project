using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSmooth = 18f;
    private Vector2Int currentGridPos;
    private Vector2 targetWorldPos;
    private bool isMoving;
    private float moveGap = 0.25f;
    void Start()
    {
        currentGridPos = GridManager.Instance.WorldToGrid(transform.position);
        targetWorldPos = GridManager.Instance.GridToWorld(currentGridPos);
        transform.position = targetWorldPos;
    }

    void Update()
    {
        transform.position = Vector2.Lerp(transform.position, targetWorldPos, moveSmooth * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetWorldPos) < 0.01f && !isMoving)
        {
            if (Input.GetKey(KeyCode.W)) MoveToGrid(Vector2Int.up);
            else if (Input.GetKey(KeyCode.S)) MoveToGrid(Vector2Int.down);
            else if (Input.GetKey(KeyCode.A)) MoveToGrid(Vector2Int.left);
            else if (Input.GetKey(KeyCode.D)) MoveToGrid(Vector2Int.right);
        }
    }

    void MoveToGrid(Vector2Int dir)
    {
        currentGridPos += dir;
        targetWorldPos = GridManager.Instance.GridToWorld(currentGridPos);
        isMoving = true;
        Invoke(nameof(MoveEnd), moveGap);
    }

    void MoveEnd()
    {
        isMoving = false;
    }
}