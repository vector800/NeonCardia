using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class BattlePartyStatusEntryView : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image accentImage;
    [SerializeField] private Image portraitFrameImage;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image hpBackImage;
    [SerializeField] private Image hpFillImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float reserveAlpha = 0.56f;

    private static readonly Color ActiveBackground = new Color(0.012f, 0.040f, 0.055f, 0.92f);
    private static readonly Color ReserveBackground = new Color(0.010f, 0.020f, 0.030f, 0.92f);
    private static readonly Color ActiveAccent = new Color(0.12f, 0.92f, 1f, 0.95f);
    private static readonly Color ReserveAccent = new Color(0.22f, 0.62f, 0.74f, 0.72f);
    private static readonly Color HealthyHp = new Color(0.18f, 0.96f, 0.55f, 1f);
    private static readonly Color WarningHp = new Color(1f, 0.78f, 0.22f, 1f);
    private static readonly Color DangerHp = new Color(1f, 0.28f, 0.32f, 1f);

    private void Reset()
    {
        CacheReferences();
    }

    private void Awake()
    {
        CacheReferences();
    }

    private void OnValidate()
    {
        CacheReferences();
    }

    public void SetStatus(string displayName, Sprite faceIcon, int currentHp, int maxHp, bool reserve)
    {
        CacheReferences();

        int safeMaxHp = Mathf.Max(0, maxHp);
        int safeCurrentHp = safeMaxHp > 0 ? Mathf.Clamp(currentHp, 0, safeMaxHp) : Mathf.Max(0, currentHp);
        float hpRate = safeMaxHp > 0 ? Mathf.Clamp01((float)safeCurrentHp / safeMaxHp) : 0f;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = reserve ? reserveAlpha : 1f;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = reserve ? ReserveBackground : ActiveBackground;
        }

        if (accentImage != null)
        {
            accentImage.color = reserve ? ReserveAccent : ActiveAccent;
        }

        if (portraitFrameImage != null)
        {
            portraitFrameImage.color = reserve
                ? new Color(0.34f, 0.62f, 0.70f, 0.86f)
                : new Color(0.64f, 1f, 1f, 0.96f);
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = faceIcon;
            portraitImage.enabled = faceIcon != null;
            portraitImage.preserveAspect = true;
            portraitImage.color = Color.white;
        }

        if (hpBackImage != null)
        {
            hpBackImage.color = reserve
                ? new Color(0.00f, 0.018f, 0.024f, 0.86f)
                : new Color(0.00f, 0.026f, 0.036f, 0.96f);
        }

        if (hpFillImage != null)
        {
            hpFillImage.type = Image.Type.Filled;
            hpFillImage.fillMethod = Image.FillMethod.Horizontal;
            hpFillImage.fillOrigin = 0;
            hpFillImage.fillAmount = hpRate;
            hpFillImage.color = ResolveHpColor(hpRate);
        }

        if (nameText != null)
        {
            nameText.text = string.IsNullOrEmpty(displayName) ? string.Empty : displayName.ToUpperInvariant();
            nameText.color = reserve ? new Color(0.72f, 0.88f, 0.92f, 1f) : new Color(0.88f, 1f, 1f, 1f);
        }

        if (hpText != null)
        {
            hpText.text = safeMaxHp > 0 ? safeCurrentHp + " / " + safeMaxHp : "-- / --";
            hpText.color = reserve ? new Color(0.84f, 0.94f, 0.96f, 1f) : Color.white;
        }
    }

    public void CacheReferences()
    {
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        accentImage = FindImage("Accent", accentImage);
        portraitFrameImage = FindImage("PortraitFrame", portraitFrameImage);
        portraitImage = FindImage("PortraitFrame/PortraitImage", portraitImage);
        hpBackImage = FindImage("HpBack", hpBackImage);
        hpFillImage = FindImage("HpBack/HpFill", hpFillImage);
        nameText = FindText("NameText", nameText);
        hpText = FindText("HpText", hpText);
    }

    private Color ResolveHpColor(float hpRate)
    {
        if (hpRate > 0.45f)
        {
            return HealthyHp;
        }

        return hpRate > 0.20f ? WarningHp : DangerHp;
    }

    private Image FindImage(string path, Image fallback)
    {
        if (fallback != null)
        {
            return fallback;
        }

        Transform child = transform.Find(path);
        return child != null ? child.GetComponent<Image>() : null;
    }

    private TMP_Text FindText(string path, TMP_Text fallback)
    {
        if (fallback != null)
        {
            return fallback;
        }

        Transform child = transform.Find(path);
        return child != null ? child.GetComponent<TMP_Text>() : null;
    }
}
