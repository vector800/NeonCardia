using System;
using UnityEngine;

[Serializable]
public sealed class BattleCommandDisplayData
{
    public BattleCommandOptionType OptionType;
    public int SourceHandIndex = -1;
    public string Title;
    public string Description;
    public string PowerText;
    public string AttributeText;
    public string HpText;
    public string TargetText;
    public string DelayText;
    public Sprite NormalBackgroundSprite;
    public Sprite SelectedBackgroundSprite;
    public Sprite Icon;
    public Sprite FaceIcon;
    public Sprite AttributeIcon;
    public Sprite BackgroundDesignSprite;
    public Color AccentColor = Color.cyan;
    public Color NormalBackgroundColor = Color.white;
    public Color SelectedBackgroundColor = Color.white;
    public bool HasStats;
    public bool Interactable = true;
}
