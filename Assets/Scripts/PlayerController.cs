using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float gridSize = 1f;
    private bool canMove = true;

    void Update()
    {
        if (!canMove) return;

        if (Input.GetKeyDown(KeyCode.W)) Move(Vector2.up);
        if (Input.GetKeyDown(KeyCode.S)) Move(Vector2.down);
        if (Input.GetKeyDown(KeyCode.A)) Move(Vector2.left);
        if (Input.GetKeyDown(KeyCode.D)) Move(Vector2.right);
    }

    void Move(Vector2 direction)
    {
        Vector2 targetPos = (Vector2)transform.position + direction * gridSize;
        transform.position = targetPos;

        canMove = false;
        Invoke(nameof(MoveCooldown), 0.12f);
    }

    void MoveCooldown()
    {
        canMove = true;
    }
}