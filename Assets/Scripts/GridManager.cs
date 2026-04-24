using UnityEngine;

public class GridManager : MonoBehaviour
{
    private static GridManager _instance;
    public static GridManager Instance
    {
        get
        {
            if (_instance == null)
            {
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

    [Header("GridSetting")]
    public float gridUnit = 0.6f;
    public int gridRange = 30;
    public Color gridColor = Color.white;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }
    public Vector2Int WorldToGrid(Vector2 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / gridUnit);
        int y = Mathf.RoundToInt(worldPos.y / gridUnit);
        return new Vector2Int(x, y);
    }

    public Vector2 GridToWorld(Vector2Int gridPos)
    {
        return new Vector2(gridPos.x * gridUnit, gridPos.y * gridUnit);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gridColor;
        for (int x = -gridRange; x <= gridRange; x++)
        {
            Gizmos.DrawLine(
                new Vector2(x * gridUnit, -gridRange * gridUnit),
                new Vector2(x * gridUnit, gridRange * gridUnit)
            );
        }
        for (int y = -gridRange; y <= gridRange; y++)
        {
            Gizmos.DrawLine(
                new Vector2(-gridRange * gridUnit, y * gridUnit),
                new Vector2(gridRange * gridUnit, y * gridUnit)
            );
        }
    }
}