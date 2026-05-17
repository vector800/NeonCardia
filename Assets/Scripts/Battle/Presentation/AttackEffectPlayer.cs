using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum BattleEffectType
{
    WaterHit,
    FireHit,
    BreakHit,
    IceHit,
    WeaponHit,
    EnemyHit,
    StageChange_Ice,
    StageChange_Grass,
    StageChange_Magma
}

public sealed class EffectPresentationSettings
{
    public EffectPresentationSettings(BattleEffectType effectType, float duration, float sizeMultiplier, bool isStageEffect)
    {
        EffectType = effectType;
        Duration = duration;
        SizeMultiplier = sizeMultiplier;
        IsStageEffect = isStageEffect;
    }

    public BattleEffectType EffectType { get; private set; }
    public float Duration { get; private set; }
    public float SizeMultiplier { get; private set; }
    public bool IsStageEffect { get; private set; }
}

public static class EffectAssetResolver
{
    public static BattleEffectType GetHitEffect(CardAttribute attribute)
    {
        switch (attribute)
        {
            case CardAttribute.Water:
                return BattleEffectType.WaterHit;
            case CardAttribute.Fire:
                return BattleEffectType.FireHit;
            case CardAttribute.Break:
                return BattleEffectType.BreakHit;
            case CardAttribute.Ice:
                return BattleEffectType.IceHit;
            default:
                return BattleEffectType.WeaponHit;
        }
    }

    public static BattleEffectType GetStageEffect(PanelType panelType)
    {
        switch (panelType)
        {
            case PanelType.Ice:
                return BattleEffectType.StageChange_Ice;
            case PanelType.Grass:
                return BattleEffectType.StageChange_Grass;
            case PanelType.Magma:
                return BattleEffectType.StageChange_Magma;
            default:
                return BattleEffectType.StageChange_Ice;
        }
    }

    public static EffectPresentationSettings GetSettings(BattleEffectType effectType)
    {
        switch (effectType)
        {
            case BattleEffectType.FireHit:
                return new EffectPresentationSettings(effectType, 0.35f, 1.2f, false);
            case BattleEffectType.BreakHit:
                return new EffectPresentationSettings(effectType, 0.4f, 1.3f, false);
            case BattleEffectType.IceHit:
                return new EffectPresentationSettings(effectType, 0.35f, 1.1f, false);
            case BattleEffectType.WeaponHit:
                return new EffectPresentationSettings(effectType, 0.25f, 0.8f, false);
            case BattleEffectType.EnemyHit:
                return new EffectPresentationSettings(effectType, 0.3f, 1f, false);
            case BattleEffectType.StageChange_Ice:
            case BattleEffectType.StageChange_Grass:
            case BattleEffectType.StageChange_Magma:
                return new EffectPresentationSettings(effectType, 0.5f, 1f, true);
            default:
                return new EffectPresentationSettings(effectType, 0.3f, 1f, false);
        }
    }
}

public sealed class AttackEffectPlayer
{
    private const int SheetColumns = 4;
    private const int SheetRows = 4;
    private const int TotalFrames = SheetColumns * SheetRows;

    private readonly Dictionary<BattleEffectType, Sprite[]> frameCache = new Dictionary<BattleEffectType, Sprite[]>();
    private RectTransform root;
    private RectTransform fieldRoot;
    private Font font;
    private MonoBehaviour coroutineHost;

    public void Build(Transform parent, Font uiFont, MonoBehaviour host)
    {
        font = uiFont;
        coroutineHost = host;
        root = CreateRect("BattleEffectRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        root.SetAsLastSibling();
        fieldRoot = CreateRect("StageEffectRoot", root, new Vector2(0.19f, 0.43f), new Vector2(0.81f, 0.72f), Vector2.zero, Vector2.zero);
    }

    public void PlayEffectAtPanel(BattleEffectType effectType, BattleGridPosition position)
    {
        if (root == null || coroutineHost == null || !position.IsValid)
        {
            return;
        }

        EffectPresentationSettings settings = EffectAssetResolver.GetSettings(effectType);
        Image image = CreateImage("Effect " + effectType, root, GetPanelAnchor(position), GetPanelAnchor(position), Vector2.zero, Vector2.zero, Color.white);
        float panelSize = 86f * settings.SizeMultiplier;
        image.rectTransform.sizeDelta = new Vector2(panelSize, panelSize);
        image.preserveAspect = true;
        coroutineHost.StartCoroutine(PlaySpriteSequence(image, effectType, settings.Duration));
    }

    public void PlayStageEffect(BattleEffectType effectType)
    {
        if (fieldRoot == null || coroutineHost == null)
        {
            return;
        }

        EffectPresentationSettings settings = EffectAssetResolver.GetSettings(effectType);
        Image image = CreateImage("Stage Effect " + effectType, fieldRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
        image.preserveAspect = false;
        coroutineHost.StartCoroutine(PlaySpriteSequence(image, effectType, settings.Duration));
    }

    public void ShowCardName(string cardName)
    {
        if (root == null || coroutineHost == null || string.IsNullOrEmpty(cardName))
        {
            return;
        }

        Text banner = CreateText("Card Name Banner", root, new Vector2(0.32f, 0.78f), new Vector2(0.68f, 0.86f), Vector2.zero, Vector2.zero, cardName, 34, TextAnchor.MiddleCenter, new Color(1f, 0.96f, 0.7f));
        coroutineHost.StartCoroutine(FadeAndDestroy(banner.gameObject, 0.55f));
    }

    public void PlayDamagePopup(BattleGridPosition position, int amount)
    {
        if (root == null || coroutineHost == null || amount <= 0 || !position.IsValid)
        {
            return;
        }

        Vector2 anchor = GetPanelAnchor(position);
        Text popup = CreateText("Damage Popup", root, anchor, anchor, new Vector2(-44f, 16f), new Vector2(44f, 58f), amount.ToString(), 28, TextAnchor.MiddleCenter, new Color(1f, 0.96f, 0.72f));
        coroutineHost.StartCoroutine(FloatAndDestroy(popup.rectTransform, 0.6f));
    }

    private IEnumerator PlaySpriteSequence(Image image, BattleEffectType effectType, float duration)
    {
        Sprite[] frames = GetFrames(effectType);
        if (frames.Length == 0)
        {
            Object.Destroy(image.gameObject);
            yield break;
        }

        float frameDuration = Mathf.Max(0.01f, duration / frames.Length);
        for (int i = 0; i < frames.Length; i++)
        {
            image.sprite = frames[i];
            image.color = Color.white;
            yield return new WaitForSeconds(frameDuration);
        }

        Object.Destroy(image.gameObject);
    }

    private IEnumerator FadeAndDestroy(GameObject target, float duration)
    {
        Graphic[] graphics = target.GetComponentsInChildren<Graphic>();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / duration);
            for (int i = 0; i < graphics.Length; i++)
            {
                Color color = graphics[i].color;
                color.a = alpha;
                graphics[i].color = color;
            }

            yield return null;
        }

        Object.Destroy(target);
    }

    private IEnumerator FloatAndDestroy(RectTransform rectTransform, float duration)
    {
        Vector2 start = rectTransform.anchoredPosition;
        Graphic[] graphics = rectTransform.GetComponentsInChildren<Graphic>();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rectTransform.anchoredPosition = start + new Vector2(0f, 28f * t);
            for (int i = 0; i < graphics.Length; i++)
            {
                Color color = graphics[i].color;
                color.a = 1f - t;
                graphics[i].color = color;
            }

            yield return null;
        }

        Object.Destroy(rectTransform.gameObject);
    }

    private Sprite[] GetFrames(BattleEffectType effectType)
    {
        Sprite[] frames;
        if (frameCache.TryGetValue(effectType, out frames))
        {
            return frames;
        }

        Texture2D texture = LoadTexture(effectType);
        if (texture == null)
        {
            texture = CreateFallbackTexture(effectType);
        }

        frames = SliceTexture(texture);
        frameCache[effectType] = frames;
        return frames;
    }

    private static Texture2D LoadTexture(BattleEffectType effectType)
    {
        string fileName = effectType + ".png";
        string folder = IsStageEffect(effectType) ? "Stage" : "Hit";
#if UNITY_EDITOR
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Effects/" + folder + "/" + fileName);
        if (texture != null)
        {
            return texture;
        }

        texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Effects/Placeholder/Placeholder_" + fileName);
        if (texture != null)
        {
            return texture;
        }
#endif
        return Resources.Load<Texture2D>("Effects/" + folder + "/" + effectType);
    }

    private static Sprite[] SliceTexture(Texture2D texture)
    {
        List<Sprite> frames = new List<Sprite>(TotalFrames);
        int cellWidth = texture.width / SheetColumns;
        int cellHeight = texture.height / SheetRows;
        for (int row = 0; row < SheetRows; row++)
        {
            for (int column = 0; column < SheetColumns; column++)
            {
                Rect rect = new Rect(column * cellWidth, texture.height - (row + 1) * cellHeight, cellWidth, cellHeight);
                frames.Add(Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f));
            }
        }

        return frames.ToArray();
    }

    private static Texture2D CreateFallbackTexture(BattleEffectType effectType)
    {
        const int size = 512;
        const int cell = size / SheetColumns;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Color baseColor = GetEffectColor(effectType);
        Color clear = new Color(0f, 0f, 0f, 0f);
        bool stage = IsStageEffect(effectType);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int frameX = x / cell;
                int frameY = y / cell;
                int localX = x - frameX * cell;
                int localY = y - frameY * cell;
                int frame = frameY * SheetColumns + frameX;
                float progress = (frame + 1) / (float)TotalFrames;
                Vector2 center = new Vector2(cell * 0.5f, cell * 0.5f);
                float distance = Vector2.Distance(new Vector2(localX, localY), center);
                float radius = stage ? 56f : Mathf.Lerp(18f, 58f, progress);
                bool filled = stage ? localX > 8 && localX < cell - 8 && localY > 18 && localY < cell - 18 : distance <= radius;
                if (!filled)
                {
                    texture.SetPixel(x, y, clear);
                    continue;
                }

                Color color = Color.Lerp(baseColor, Color.white, 0.25f + 0.35f * progress);
                color.a = Mathf.Lerp(0.9f, 0.25f, progress);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    private static bool IsStageEffect(BattleEffectType effectType)
    {
        return effectType == BattleEffectType.StageChange_Ice
            || effectType == BattleEffectType.StageChange_Grass
            || effectType == BattleEffectType.StageChange_Magma;
    }

    private static Color GetEffectColor(BattleEffectType effectType)
    {
        switch (effectType)
        {
            case BattleEffectType.WaterHit:
                return new Color(0.22f, 0.72f, 1f, 1f);
            case BattleEffectType.FireHit:
            case BattleEffectType.StageChange_Magma:
                return new Color(1f, 0.28f, 0.08f, 1f);
            case BattleEffectType.BreakHit:
                return new Color(0.88f, 0.82f, 0.68f, 1f);
            case BattleEffectType.IceHit:
            case BattleEffectType.StageChange_Ice:
                return new Color(0.52f, 0.88f, 1f, 1f);
            case BattleEffectType.EnemyHit:
                return new Color(0.9f, 0.18f, 0.42f, 1f);
            case BattleEffectType.StageChange_Grass:
                return new Color(0.22f, 0.86f, 0.26f, 1f);
            default:
                return new Color(0.94f, 0.96f, 1f, 1f);
        }
    }

    private static Vector2 GetPanelAnchor(BattleGridPosition position)
    {
        int globalColumn = position.Side == GridSide.Player ? position.Column : BattleGridPosition.GridSize + position.Column;
        float fieldMinX = 0.19f;
        float fieldMaxX = 0.81f;
        float fieldMinY = 0.43f;
        float fieldMaxY = 0.72f;
        float x = Mathf.Lerp(fieldMinX, fieldMaxX, (globalColumn + 0.5f) / (BattleGridPosition.GridSize * 2f));
        float y = Mathf.Lerp(fieldMinY, fieldMaxY, 1f - (position.Row + 0.5f) / BattleGridPosition.GridSize);
        return new Vector2(x, y);
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        return rect;
    }

    private static Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string text, int fontSize, TextAnchor alignment, Color color)
    {
        RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        Text label = rect.gameObject.AddComponent<Text>();
        label.font = font;
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = FontStyle.Bold;
        label.alignment = alignment;
        label.color = color;
        label.raycastTarget = false;

        Shadow shadow = label.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        shadow.effectDistance = new Vector2(2f, -2f);
        return label;
    }
}
