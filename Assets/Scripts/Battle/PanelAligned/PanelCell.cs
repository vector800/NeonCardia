using UnityEngine;

public enum PanelCellSide
{
    Ally,
    Enemy
}

[DisallowMultipleComponent]
public sealed class PanelCell : MonoBehaviour
{
    [SerializeField] private PanelCellSide side;
    [SerializeField, Range(0, 2)] private int row;
    [SerializeField, Range(0, 2)] private int col;
    [SerializeField] private RectTransform rectTransformReference;

    private Collider2D cachedCollider;

    public PanelCellSide Side { get { return side; } }
    public int Row { get { return row; } }
    public int Col { get { return col; } }
    public int GlobalColumn { get { return side == PanelCellSide.Ally ? col : col + 3; } }
    public Vector3 CenterWorldPosition { get { return transform.position; } }
    public Bounds Bounds { get { return GetBounds(); } }
    public RectTransform RectTransform { get { return rectTransformReference; } }

    public PanelCellSide sideValue { get { return side; } }
    public int rowValue { get { return row; } }
    public int colValue { get { return col; } }
    public Vector3 centerWorldPosition { get { return CenterWorldPosition; } }
    public Bounds bounds { get { return Bounds; } }
    public RectTransform rectTransform { get { return rectTransformReference; } }

    public void Configure(PanelCellSide panelSide, int panelRow, int panelCol, RectTransform panelRectTransform)
    {
        side = panelSide;
        row = Mathf.Clamp(panelRow, 0, 2);
        col = Mathf.Clamp(panelCol, 0, 2);
        rectTransformReference = panelRectTransform;
        cachedCollider = GetComponent<Collider2D>();
    }

    public bool ContainsLogicalPosition(PanelCellSide targetSide, int targetRow, int targetCol)
    {
        return side == targetSide && row == targetRow && col == targetCol;
    }

    private Bounds GetBounds()
    {
        if (cachedCollider == null)
        {
            cachedCollider = GetComponent<Collider2D>();
        }

        if (cachedCollider != null)
        {
            return cachedCollider.bounds;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            return spriteRenderer.bounds;
        }

        return new Bounds(transform.position, Vector3.one);
    }

    private void OnDrawGizmos()
    {
        Bounds panelBounds = GetBounds();
        Gizmos.color = side == PanelCellSide.Ally
            ? new Color(1f, 0.38f, 0.08f, 0.72f)
            : new Color(0.08f, 0.85f, 1f, 0.72f);
        Gizmos.DrawWireCube(panelBounds.center, panelBounds.size);

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(CenterWorldPosition, 0.035f);
    }
}
