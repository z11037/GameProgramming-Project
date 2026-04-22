using UnityEngine;

public class GridManager : MonoBehaviour
{
    private static GridManager _instance;
    private static readonly object _lock = new object();

    public static GridManager Instance
    {
        get
        {
            if (_instance != null) return _instance;

            lock (_lock)
            {
                if (_instance != null) return _instance;

                _instance = FindObjectOfType<GridManager>();

                if (_instance == null)
                {
                    GameObject temp = new GameObject("GridManager");
                    _instance = temp.AddComponent<GridManager>();
                }
            }

            return _instance;
        }
    }

    public float gridUnit = 0.6f;
    public int gridRange = 30;
    public Color gridColor = Color.white;

    private void Awake()
    {
        // Awake里立即处理单例：如果已有实例，销毁自己
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    private Vector2 GridOffset => new(gridUnit * 0.5f, gridUnit * 0.5f);

    public Vector2Int WorldToGrid(Vector2 worldPos)
    {
        Vector2 pos = worldPos - GridOffset;
        int x = Mathf.FloorToInt(pos.x / gridUnit);
        int y = Mathf.FloorToInt(pos.y / gridUnit);
        return new Vector2Int(x, y);
    }

    public Vector2 GridToWorld(Vector2Int gridPos)
    {
        return new Vector2(
            gridPos.x * gridUnit + gridUnit / 2f,
            gridPos.y * gridUnit + gridUnit / 2f
        );
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gridColor;
        for (int x = -gridRange; x <= gridRange; x++)
            Gizmos.DrawLine(new Vector3(x * gridUnit, -gridRange * gridUnit, 0), new Vector3(x * gridUnit, gridRange * gridUnit, 0));
        for (int y = -gridRange; y <= gridRange; y++)
            Gizmos.DrawLine(new Vector3(-gridRange * gridUnit, y * gridUnit, 0), new Vector3(gridRange * gridUnit, y * gridUnit, 0));
    }
}