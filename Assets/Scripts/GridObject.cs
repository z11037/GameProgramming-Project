using UnityEngine;

[ExecuteInEditMode]
public class GridObject : MonoBehaviour
{
    public enum ObjectType { Empty, Wall, Rock, Player, Text }
    public ObjectType type;

    [Header("Rule")]
    public bool isYou;
    public bool isStop;
    public bool isPush;
    public bool isWin;

    [Header("Move")]
    public float moveLerpSpeed = 20f; 
    public Vector2Int TargetGridPos { get; private set; }
    private Vector2 targetWorldPos;

    public Vector2Int GridPos
    {
        get
        {
            if (GridManager.Instance == null) return Vector2Int.zero;
            return GridManager.Instance.WorldToGrid(transform.position);
        }
    }

    private void Awake()
    {
        if (!Application.isPlaying && GridManager.Instance == null)
        {
            GameObject temp = new GameObject("TempGridManager");
            temp.AddComponent<GridManager>();
        }

        if (GridManager.Instance != null)
        {
            Vector2Int gridPos = GridManager.Instance.WorldToGrid(transform.position);
            transform.position = GridManager.Instance.GridToWorld(gridPos);
            TargetGridPos = gridPos;
            targetWorldPos = transform.position;
        }
    }

    private void Start()
    {
        if (Application.isPlaying && LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterObject(this);
        }
    }

    public void SetTargetGrid(Vector2Int target)
    {
        TargetGridPos = target;
        if (GridManager.Instance != null)
        {
            targetWorldPos = GridManager.Instance.GridToWorld(target);
        }
    }
    private void Update()
    {
        if (!Application.isPlaying) return;
        transform.position = Vector2.Lerp(transform.position, targetWorldPos, moveLerpSpeed * Time.deltaTime);
    }
}