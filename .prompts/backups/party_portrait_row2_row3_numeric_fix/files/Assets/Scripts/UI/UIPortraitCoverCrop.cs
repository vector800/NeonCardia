using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class UIPortraitCoverCrop : MonoBehaviour
{
    [SerializeField] private RectTransform maskRect;
    [SerializeField] private Image targetImage;
    [SerializeField] private float zoom = 1.12f;
    [SerializeField] private Vector2 overscan = new Vector2(14f, 10f);
    [SerializeField] private Vector2 offset;

    private Vector2 lastMaskSize = new Vector2(-1f, -1f);
    private Sprite lastSprite;
    private float lastZoom = -1f;
    private Vector2 lastOverscan = new Vector2(-1f, -1f);
    private Vector2 lastOffset = new Vector2(float.NaN, float.NaN);

    public float Zoom => zoom;
    public Vector2 Overscan => overscan;
    public Vector2 Offset => offset;

    private void OnEnable()
    {
        Apply();
    }

    private void LateUpdate()
    {
        ApplyIfNeeded();
    }

    public void Bind(RectTransform mask, Image image)
    {
        maskRect = mask;
        targetImage = image;
    }

    public void Configure(RectTransform mask, Image image, float zoomValue, Vector2 overscanValue, Vector2 offsetValue)
    {
        maskRect = mask;
        targetImage = image;
        zoom = Mathf.Max(0.01f, zoomValue);
        overscan = new Vector2(Mathf.Max(0f, overscanValue.x), Mathf.Max(0f, overscanValue.y));
        offset = offsetValue;
        Apply();
    }

    public void ApplyIfNeeded()
    {
        if (maskRect == null || targetImage == null)
        {
            return;
        }

        Vector2 maskSize = GetMaskSize();
        Sprite sprite = targetImage.sprite;
        if (sprite == lastSprite
            && Approximately(maskSize, lastMaskSize)
            && Mathf.Approximately(zoom, lastZoom)
            && Approximately(overscan, lastOverscan)
            && Approximately(offset, lastOffset))
        {
            return;
        }

        Apply();
    }

    public void Apply()
    {
        if (maskRect == null || targetImage == null || targetImage.sprite == null)
        {
            return;
        }

        Vector2 maskSize = GetMaskSize();
        if (maskSize.x <= 1f || maskSize.y <= 1f)
        {
            return;
        }

        Rect spriteRect = targetImage.sprite.rect;
        float spriteAspect = spriteRect.height > 0f ? spriteRect.width / spriteRect.height : 1f;
        Vector2 targetSize = maskSize + overscan * 2f;
        float targetAspect = targetSize.x / Mathf.Max(0.01f, targetSize.y);

        float width = targetSize.x;
        float height = targetSize.y;
        if (spriteAspect > targetAspect)
        {
            width = height * spriteAspect;
        }
        else
        {
            height = width / Mathf.Max(0.01f, spriteAspect);
        }

        width *= Mathf.Max(0.01f, zoom);
        height *= Mathf.Max(0.01f, zoom);

        RectTransform imageRect = targetImage.rectTransform;
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = offset;
        imageRect.sizeDelta = new Vector2(width, height);
        imageRect.localScale = Vector3.one;

        targetImage.type = Image.Type.Simple;
        targetImage.preserveAspect = true;
        targetImage.raycastTarget = false;

        lastMaskSize = maskSize;
        lastSprite = targetImage.sprite;
        lastZoom = zoom;
        lastOverscan = overscan;
        lastOffset = offset;
    }

    private Vector2 GetMaskSize()
    {
        if (maskRect == null)
        {
            return Vector2.zero;
        }

        Vector2 size = maskRect.rect.size;
        if (size.x <= 1f || size.y <= 1f)
        {
            size = maskRect.sizeDelta;
        }

        return new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
    }

    private static bool Approximately(Vector2 a, Vector2 b)
    {
        return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
    }
}
