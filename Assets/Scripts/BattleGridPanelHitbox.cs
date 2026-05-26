using UnityEngine;

[DisallowMultipleComponent]
public sealed class BattleGridPanelHitbox : MonoBehaviour
{
    [SerializeField] private GridSide side;
    [SerializeField, Range(0, BattleGridPosition.GridSize - 1)] private int row;
    [SerializeField, Range(0, BattleGridPosition.GridSize - 1)] private int column;
    [SerializeField] private Transform unitAnchor;

    public GridSide Side { get { return side; } }
    public int Row { get { return row; } }
    public int Column { get { return column; } }
    public Transform UnitAnchor { get { return unitAnchor != null ? unitAnchor : transform; } }
    public BattleGridPosition Position { get { return new BattleGridPosition(side, row, column); } }

    public void Configure(GridSide panelSide, int panelRow, int panelColumn, Transform anchor)
    {
        side = panelSide;
        row = Mathf.Clamp(panelRow, 0, BattleGridPosition.GridSize - 1);
        column = Mathf.Clamp(panelColumn, 0, BattleGridPosition.GridSize - 1);
        unitAnchor = anchor;
    }

    private void Reset()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }

        collider.isTrigger = true;
    }

    private void OnMouseDown()
    {
        Debug.Log("Battle grid panel clicked: " + Position);
    }

    private void OnDrawGizmos()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            return;
        }

        Gizmos.color = side == GridSide.Player
            ? new Color(1f, 0.25f, 0.08f, 0.65f)
            : new Color(0.05f, 0.85f, 1f, 0.65f);

        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        PolygonCollider2D polygon = collider as PolygonCollider2D;
        if (polygon != null)
        {
            for (int pathIndex = 0; pathIndex < polygon.pathCount; pathIndex++)
            {
                Vector2[] points = polygon.GetPath(pathIndex);
                for (int i = 0; i < points.Length; i++)
                {
                    Vector2 next = points[(i + 1) % points.Length];
                    Gizmos.DrawLine(points[i], next);
                }
            }

            Gizmos.matrix = previousMatrix;
            return;
        }

        BoxCollider2D box = collider as BoxCollider2D;
        if (box != null)
        {
            Gizmos.DrawWireCube(box.offset, box.size);
        }

        Gizmos.matrix = previousMatrix;
    }
}
