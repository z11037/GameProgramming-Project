using UnityEngine;

[ExecuteInEditMode]
public class GridObject : MonoBehaviour
{
    public enum ObjectType { Empty, Wall, Rock, Player, Text, Man, Cherry }
    public ObjectType type;

    public enum TextType { None, Noun, Verb, Property }
    [Header("Text")]
    public TextType textType;
    public enum TextContent { None, Man, Wall, Rock, Cherry, Is, You, Stop, Push, Win }
    public TextContent textContent;

    [Header("ObjectVisual")]
    public Sprite wallSprite;
    public Sprite rockSprite;
    public Sprite manSprite;
    public Sprite cherrySprite;
    private SpriteRenderer _spriteRenderer;

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
        _spriteRenderer = GetComponent<SpriteRenderer>();
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
        ForceAlignToGrid();

        if (type == ObjectType.Text)
        {
            isPush = true;
        }

        UpdateVisualForType();
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

    public void ResetRuleProperties()
    {
        isYou = false;
        isStop = false;
        isPush = type == ObjectType.Text;
        isWin = false;
    }
    private void Update()
    {
        if (!Application.isPlaying) return;
        transform.position = Vector2.Lerp(transform.position, targetWorldPos, moveLerpSpeed * Time.deltaTime);
    }

    public void UpdateVisualForType()
    {
        ResetRuleProperties();

        if (type == ObjectType.Text || _spriteRenderer == null) return;

        switch (type)
        {
            case ObjectType.Wall:
                if (wallSprite != null) _spriteRenderer.sprite = wallSprite;
                break;
            case ObjectType.Rock:
                if (rockSprite != null) _spriteRenderer.sprite = rockSprite;
                break;
            case ObjectType.Man:
                if (manSprite != null) _spriteRenderer.sprite = manSprite;
                break;
            case ObjectType.Cherry:
                if (cherrySprite != null) _spriteRenderer.sprite = cherrySprite;
                break;
        }
    }
    private void OnValidate()
    {
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        if (!Application.isPlaying)
        {
            ForceAlignToGrid();
        }
        UpdateVisualForType();
    }

    public void ForceAlignToGrid()
    {
        if (GridManager.Instance == null) return;

        Vector2Int gridPos = GridManager.Instance.WorldToGrid(transform.position);
        Vector2 alignedWorldPos = GridManager.Instance.GridToWorld(gridPos);
        transform.position = new Vector3(alignedWorldPos.x, alignedWorldPos.y, transform.position.z);
        TargetGridPos = gridPos;
        targetWorldPos = alignedWorldPos;
    }
    private void OnDestroy()
    {
        if (Application.isPlaying && LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterObject(this);
        }
    }
}