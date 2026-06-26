using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PartyMemberStatusRowView : MonoBehaviour
{
    [SerializeField] private Image rowFrame;
    [SerializeField] private Image selectedHighlight;
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

    private static readonly Color TextPrimary = new Color(0.92f, 0.98f, 1f, 1f);
    private static readonly Color TextSecondary = new Color(0.76f, 0.88f, 0.92f, 1f);
    private static readonly Color HpLabel = new Color(0.70f, 1f, 0.88f, 1f);
    private static readonly Color MpLabel = new Color(0.68f, 0.78f, 1f, 1f);
    private static readonly Color Disabled = new Color(0.46f, 0.54f, 0.58f, 0.82f);

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
        CacheReferences();

        int safeMaxHp = Mathf.Max(0, maxHp);
        int safeCurrentHp = safeMaxHp > 0 ? Mathf.Clamp(currentHp, 0, safeMaxHp) : Mathf.Max(0, currentHp);
        int safeMaxMp = Mathf.Max(0, maxMp);
        int safeCurrentMp = safeMaxMp > 0 ? Mathf.Clamp(currentMp, 0, safeMaxMp) : Mathf.Max(0, currentMp);

        float hpRate = safeMaxHp > 0 ? Mathf.Clamp01((float)safeCurrentHp / safeMaxHp) : 0f;
        float mpRate = safeMaxMp > 0 ? Mathf.Clamp01((float)safeCurrentMp / safeMaxMp) : 0f;

        SetSelected(selected);

        if (nameText != null)
        {
            nameText.text = string.IsNullOrEmpty(displayName) ? "EMPTY" : displayName.ToUpperInvariant();
            nameText.color = safeMaxHp > 0 ? TextPrimary : Disabled;
        }

        if (hpLabelText != null)
        {
            hpLabelText.text = "HP";
            hpLabelText.color = HpLabel;
        }

        if (mpLabelText != null)
        {
            mpLabelText.text = "MP";
            mpLabelText.color = MpLabel;
        }

        if (hpValueText != null)
        {
            hpValueText.text = safeMaxHp > 0 ? safeCurrentHp + "/" + safeMaxHp : "--/--";
            hpValueText.color = safeMaxHp > 0 ? TextPrimary : Disabled;
        }

        if (mpValueText != null)
        {
            mpValueText.text = safeMaxMp > 0 ? safeCurrentMp + "/" + safeMaxMp : "--/--";
            mpValueText.color = safeMaxMp > 0 ? TextSecondary : Disabled;
        }

        ConfigureFill(hpBarFill, hpRate);
        ConfigureFill(mpBarFill, mpRate);

        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
            portraitImage.preserveAspect = true;
            portraitImage.maskable = true;
            portraitImage.material = null;
            portraitImage.raycastTarget = false;
            portraitImage.color = Color.white;
            ApplyPortraitCoverCrop();
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectedHighlight != null)
        {
            selectedHighlight.gameObject.SetActive(selected);
        }
    }

    public void CacheReferences()
    {
        rowFrame = FindImage(new[] { "RowFrame_Image", "RowFrame" }, rowFrame);
        selectedHighlight = FindImage(new[] { "SelectedHighlight_Image", "SelectedHighlight" }, selectedHighlight);
        portraitMask = FindImage(new[] { "PortraitArea/PortraitClipMask", "PortraitArea/PortraitMask", "PortraitMask" }, portraitMask);
        portraitImage = FindImage(new[] { "PortraitArea/PortraitClipMask/PortraitImage", "PortraitArea/PortraitMask/PortraitImage", "PortraitMask/PortraitImage" }, portraitImage);
        slashDivider = FindImage(new[] { "PortraitArea/SlashDivider_Image", "SlashDivider_Image", "SlashDivider" }, slashDivider);
        portraitCoverCrop = FindPortraitCoverCrop(portraitCoverCrop);
        hpBarBackground = FindImage("HPBar_Background", hpBarBackground);
        hpBarFill = FindImage("HPBar_Background/HPBar_Fill", hpBarFill);
        mpBarBackground = FindImage("MPBar_Background", mpBarBackground);
        mpBarFill = FindImage("MPBar_Background/MPBar_Fill", mpBarFill);
        nameText = FindText("NameText_TMP", nameText);
        hpLabelText = FindText("HPLabel_TMP", hpLabelText);
        hpValueText = FindText("HPValue_TMP", hpValueText);
        mpLabelText = FindText("MPLabel_TMP", mpLabelText);
        mpValueText = FindText("MPValue_TMP", mpValueText);

        ConfigureFill(hpBarFill, hpBarFill != null ? hpBarFill.fillAmount : 1f);
        ConfigureFill(mpBarFill, mpBarFill != null ? mpBarFill.fillAmount : 1f);

        if (rowFrame != null)
        {
            rowFrame.raycastTarget = false;
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

    private static void ConfigureFill(Image image, float amount)
    {
        if (image == null)
        {
            return;
        }

        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = 0;
        image.fillAmount = Mathf.Clamp01(amount);
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

        Transform portraitArea = transform.Find("PortraitArea");
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
}
