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
                GameObject temp = new GameObject("GridManager_AutoCreated");
                _instance = temp.AddComponent<GridManager>();
            }
            return _instance;
        }
    }

    public float gridUnit = 0.6f;
    public int gridRange = 30;
    public Color gridColor = Color.white;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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
            Gizmos.DrawLine(new(x * gridUnit, -gridRange * gridUnit, 0), new(x * gridUnit, gridRange * gridUnit, 0));
        for (int y = -gridRange; y <= gridRange; y++)
            Gizmos.DrawLine(new(-gridRange * gridUnit, y * gridUnit, 0), new(gridRange * gridUnit, y * gridUnit, 0));
    }
}