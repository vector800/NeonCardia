using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PartyMemberStatusRowView : MonoBehaviour
{
    [SerializeField] private Image rowFrame;
    [SerializeField] private Image namePlateImage;
    [SerializeField] private Image hpPlateImage;
    [SerializeField] private Image selectedHighlight;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image portraitMask;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image slashDivider;
    [SerializeField] private UIPortraitCoverCrop portraitCoverCrop;
    [SerializeField] private Image hpBarBackground;
    [SerializeField] private Image hpBarFill;
    [SerializeField] private Image mpBarBackground;
    [SerializeField] private Image mpBarFill;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text hpLabelText;
    [SerializeField] private TMP_Text hpValueText;
    [SerializeField] private TMP_Text mpLabelText;
    [SerializeField] private TMP_Text mpValueText;

    private static readonly Color Disabled = new Color(0.46f, 0.54f, 0.58f, 0.82f);
    private static readonly Color HpHealthy = new Color(0.92f, 0.98f, 1f, 1f);
    private static readonly Color HpWarning = new Color(1f, 0.82f, 0.34f, 1f);
    private static readonly Color HpDanger = new Color(1f, 0.32f, 0.38f, 1f);
    private static readonly Color ActivePlate = new Color(0.88f, 0.96f, 1f, 0.96f);
    private static readonly Color ActivePlateSelected = new Color(1f, 1f, 1f, 1f);
    private static readonly Color ActiveHpPlate = new Color(0.80f, 0.94f, 1f, 0.96f);
    private static readonly Color ActiveHpPlateSelected = new Color(0.95f, 1f, 1f, 1f);
    private static readonly Color ReservePlate = new Color(0.58f, 0.68f, 0.72f, 0.66f);
    private static readonly Color ReserveHpPlate = new Color(0.52f, 0.62f, 0.67f, 0.70f);
    private static readonly Color ActiveName = new Color(0.86f, 0.98f, 1f, 1f);
    private static readonly Color ReserveName = new Color(0.68f, 0.82f, 0.86f, 0.92f);
    private static readonly Color ReserveHp = new Color(0.76f, 0.90f, 0.94f, 0.94f);
    private static readonly Color ReservePortrait = new Color(0.72f, 0.82f, 0.86f, 0.74f);

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

    public void SetStatus(string displayName, Sprite portrait, int currentHp, int maxHp, int currentMp, int maxMp, bool selected)
    {
        SetStatus(displayName, portrait, currentHp, maxHp, currentMp, maxMp, selected, false);
    }

    public void SetStatus(string displayName, Sprite portrait, int currentHp, int maxHp, int currentMp, int maxMp, bool selected, bool reserve)
    {
        CacheReferences();
        gameObject.SetActive(true);

        int safeMaxHp = Mathf.Max(0, maxHp);
        int safeCurrentHp = safeMaxHp > 0 ? Mathf.Clamp(currentHp, 0, safeMaxHp) : Mathf.Max(0, currentHp);
        float hpRate = safeMaxHp > 0 ? Mathf.Clamp01((float)safeCurrentHp / safeMaxHp) : 0f;

        if (nameText != null)
        {
            nameText.gameObject.SetActive(true);
            nameText.text = string.IsNullOrWhiteSpace(displayName) ? "-" : displayName;
            nameText.color = reserve ? ReserveName : ActiveName;
            ConfigureNameText(nameText);
        }

        if (hpLabelText != null)
        {
            hpLabelText.text = string.Empty;
            hpLabelText.gameObject.SetActive(false);
        }

        if (mpLabelText != null)
        {
            mpLabelText.text = string.Empty;
            mpLabelText.gameObject.SetActive(false);
        }

        if (hpValueText != null)
        {
            hpValueText.gameObject.SetActive(true);
            hpValueText.text = safeCurrentHp.ToString();
            hpValueText.color = reserve ? ReserveHp : ResolveHpNumberColor(safeCurrentHp, safeMaxHp, hpRate);
            ConfigureHpNumberText(hpValueText);
        }

        if (mpValueText != null)
        {
            mpValueText.text = string.Empty;
            mpValueText.gameObject.SetActive(false);
        }

        SetImageActive(hpBarBackground, false);
        SetImageActive(hpBarFill, false);
        SetImageActive(mpBarBackground, false);
        SetImageActive(mpBarFill, false);
        SetImageActive(rowFrame, false);
        SetImageActive(slashDivider, false);
        SetImageActive(namePlateImage, true);
        SetImageActive(hpPlateImage, true);
        SetSelected(selected, reserve);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = reserve ? 0.74f : 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
            portraitImage.preserveAspect = true;
            portraitImage.maskable = true;
            portraitImage.material = null;
            portraitImage.raycastTarget = false;
            portraitImage.color = reserve ? ReservePortrait : Color.white;
            ApplyPortraitCoverCrop();
        }
    }

    public void SetSelected(bool selected)
    {
        SetSelected(selected, false);
    }

    public void SetSelected(bool selected, bool reserve)
    {
        if (selectedHighlight != null)
        {
            selectedHighlight.gameObject.SetActive(false);
        }

        if (namePlateImage != null)
        {
            namePlateImage.color = reserve ? ReservePlate : selected ? ActivePlateSelected : ActivePlate;
        }

        if (hpPlateImage != null)
        {
            hpPlateImage.color = reserve ? ReserveHpPlate : selected ? ActiveHpPlateSelected : ActiveHpPlate;
        }
    }

    public void Clear()
    {
        gameObject.SetActive(false);
    }

    public void CacheReferences()
    {
        rowFrame = FindImage(new[] { "RowFrame_Image", "RowFrame" }, rowFrame);
        namePlateImage = FindImage(new[] { "NamePlate", "RowBackground" }, namePlateImage);
        hpPlateImage = FindImage(new[] { "HpPlate", "HpPlate_Image", "SimpleRectHpPlate_Image", "RowBackground" }, hpPlateImage);
        selectedHighlight = FindImage(new[] { "SelectedHighlight_Image", "SelectedHighlight" }, selectedHighlight);
        canvasGroup = canvasGroup != null ? canvasGroup : GetComponent<CanvasGroup>();
        portraitMask = FindImage(new[] { "FaceIconRoot/PortraitClipMask", "FaceIcon/PortraitClipMask", "PortraitArea/PortraitClipMask", "PortraitArea/PortraitMask", "PortraitMask" }, portraitMask);
        portraitImage = FindImage(new[] { "FaceIconRoot/PortraitClipMask/FaceIconImage", "FaceIconRoot/PortraitClipMask/PortraitImage", "FaceIconRoot/FaceIconImage", "FaceIcon/PortraitClipMask/PortraitImage", "PortraitArea/PortraitClipMask/PortraitImage", "PortraitArea/PortraitMask/PortraitImage", "PortraitMask/PortraitImage" }, portraitImage);
        slashDivider = FindImage(new[] { "FaceIconRoot/SlashDivider_Image", "FaceIcon/SlashDivider_Image", "PortraitArea/SlashDivider_Image", "SlashDivider_Image", "SlashDivider" }, slashDivider);
        portraitCoverCrop = FindPortraitCoverCrop(portraitCoverCrop);
        hpBarBackground = FindImage("HPBar_Background", hpBarBackground);
        hpBarFill = FindImage("HPBar_Background/HPBar_Fill", hpBarFill);
        mpBarBackground = FindImage("MPBar_Background", mpBarBackground);
        mpBarFill = FindImage("MPBar_Background/MPBar_Fill", mpBarFill);
        nameText = FindText(new[] { "NamePlate/NameText", "NameText", "NameText_TMP" }, nameText);
        hpLabelText = FindText("HPLabel_TMP", hpLabelText);
        hpValueText = FindText(new[] { "HpPlate/HpText", "HpText", "HPValue_TMP" }, hpValueText);
        mpLabelText = FindText("MPLabel_TMP", mpLabelText);
        mpValueText = FindText("MPValue_TMP", mpValueText);

        if (rowFrame != null)
        {
            rowFrame.raycastTarget = false;
        }

        if (namePlateImage != null)
        {
            namePlateImage.raycastTarget = false;
        }

        if (hpPlateImage != null)
        {
            hpPlateImage.raycastTarget = false;
        }

        if (slashDivider != null)
        {
            slashDivider.raycastTarget = false;
        }

        if (portraitMask != null)
        {
            portraitMask.raycastTarget = false;
        }

        if (portraitImage != null)
        {
            portraitImage.maskable = true;
            portraitImage.material = null;
            portraitImage.raycastTarget = false;
        }

        ApplyPortraitCoverCrop();
    }

    private static void ConfigureNameText(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.enableAutoSizing = true;
        text.fontSizeMin = 10f;
        text.fontSizeMax = 14f;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Truncate;
        text.raycastTarget = false;
        text.outlineWidth = 0.06f;
        text.outlineColor = new Color(0f, 0f, 0f, 0.92f);
    }

    private static void ConfigureHpNumberText(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.MidlineRight;
        text.enableAutoSizing = true;
        text.fontSizeMin = 13f;
        text.fontSizeMax = 16f;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        text.outlineWidth = 0.07f;
        text.outlineColor = new Color(0f, 0f, 0f, 0.92f);
    }

    private static Color ResolveHpNumberColor(int currentHp, int maxHp, float hpRate)
    {
        if (maxHp <= 0 && currentHp <= 0)
        {
            return Disabled;
        }

        if (currentHp <= 0)
        {
            return HpDanger;
        }

        return hpRate > 0.45f ? HpHealthy : hpRate > 0.20f ? HpWarning : HpDanger;
    }

    private static void SetImageActive(Image image, bool active)
    {
        if (image == null)
        {
            return;
        }

        image.gameObject.SetActive(active);
        image.raycastTarget = false;
    }

    private void ApplyPortraitCoverCrop()
    {
        if (portraitImage == null || portraitMask == null || portraitImage.sprite == null)
        {
            return;
        }

        if (portraitCoverCrop != null)
        {
            portraitCoverCrop.Bind(portraitMask.rectTransform, portraitImage);
            portraitCoverCrop.Apply();
            return;
        }

        FitPortraitToMask();
    }

    private void FitPortraitToMask()
    {
        if (portraitImage == null || portraitMask == null || portraitImage.sprite == null)
        {
            return;
        }

        RectTransform maskRect = portraitMask.rectTransform;
        RectTransform portraitRect = portraitImage.rectTransform;
        if (maskRect == null || portraitRect == null)
        {
            return;
        }

        Vector2 maskSize = maskRect.rect.size;
        if (maskSize.x <= 1f || maskSize.y <= 1f)
        {
            portraitRect.anchorMin = Vector2.zero;
            portraitRect.anchorMax = Vector2.one;
            portraitRect.offsetMin = Vector2.zero;
            portraitRect.offsetMax = Vector2.zero;
            portraitRect.localScale = Vector3.one;
            return;
        }

        Rect spriteRect = portraitImage.sprite.rect;
        float spriteAspect = spriteRect.height > 0f ? spriteRect.width / spriteRect.height : 1f;
        float maskAspect = maskSize.x / maskSize.y;
        float width = maskSize.x;
        float height = maskSize.y;

        if (spriteAspect < maskAspect)
        {
            height = width / Mathf.Max(0.01f, spriteAspect);
        }
        else
        {
            width = height * spriteAspect;
        }

        portraitRect.anchorMin = new Vector2(0.5f, 0.5f);
        portraitRect.anchorMax = new Vector2(0.5f, 0.5f);
        portraitRect.pivot = new Vector2(0.5f, 0.5f);
        portraitRect.anchoredPosition = Vector2.zero;
        portraitRect.sizeDelta = new Vector2(width, height);
        portraitRect.localScale = Vector3.one;
    }

    private UIPortraitCoverCrop FindPortraitCoverCrop(UIPortraitCoverCrop fallback)
    {
        if (fallback != null)
        {
            return fallback;
        }

        Transform portraitArea = transform.Find("FaceIconRoot");
        if (portraitArea == null)
        {
            portraitArea = transform.Find("FaceIcon");
        }
        if (portraitArea == null)
        {
            portraitArea = transform.Find("PortraitArea");
        }
        if (portraitArea != null)
        {
            UIPortraitCoverCrop crop = portraitArea.GetComponent<UIPortraitCoverCrop>();
            if (crop != null)
            {
                return crop;
            }

            crop = portraitArea.GetComponentInChildren<UIPortraitCoverCrop>(true);
            if (crop != null)
            {
                return crop;
            }
        }

        return GetComponentInChildren<UIPortraitCoverCrop>(true);
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

    private Image FindImage(string[] paths, Image fallback)
    {
        if (fallback != null)
        {
            return fallback;
        }

        for (int i = 0; i < paths.Length; i++)
        {
            Transform child = transform.Find(paths[i]);
            if (child == null)
            {
                continue;
            }

            Image image = child.GetComponent<Image>();
            if (image != null)
            {
                return image;
            }
        }

        return null;
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

    private TMP_Text FindText(string[] paths, TMP_Text fallback)
    {
        if (fallback != null)
        {
            return fallback;
        }

        for (int i = 0; i < paths.Length; i++)
        {
            Transform child = transform.Find(paths[i]);
            if (child == null)
            {
                continue;
            }

            TMP_Text text = child.GetComponent<TMP_Text>();
            if (text != null)
            {
                return text;
            }
        }

        return null;
    }
}
