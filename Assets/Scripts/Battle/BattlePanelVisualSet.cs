using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BattlePanelVisualSet", menuName = "NeonCardia/Battle Panel Visual Set")]
public sealed class BattlePanelVisualSet : ScriptableObject
{
    [FormerlySerializedAs("PlayerNormal")]
    [SerializeField] private Sprite playerNormal;
    [FormerlySerializedAs("PlayerSelected")]
    [SerializeField] private Sprite playerSelected;
    [FormerlySerializedAs("EnemyNormal")]
    [SerializeField] private Sprite enemyNormal;
    [FormerlySerializedAs("EnemySelected")]
    [SerializeField] private Sprite enemySelected;
    [FormerlySerializedAs("TargetableOverlay")]
    [SerializeField] private Sprite targetableOverlay;
    [FormerlySerializedAs("DangerOverlay")]
    [SerializeField] private Sprite dangerOverlay;
    [FormerlySerializedAs("HoverOverlay")]
    [SerializeField] private Sprite hoverOverlay;
    [FormerlySerializedAs("DisabledOverlay")]
    [SerializeField] private Sprite disabledOverlay;
    [FormerlySerializedAs("BreakHintOverlay")]
    [SerializeField] private Sprite breakHintOverlay;
    [FormerlySerializedAs("HealHintOverlay")]
    [SerializeField] private Sprite healHintOverlay;

    public Sprite PlayerNormal { get { return playerNormal; } }
    public Sprite PlayerSelected { get { return playerSelected; } }
    public Sprite EnemyNormal { get { return enemyNormal; } }
    public Sprite EnemySelected { get { return enemySelected; } }
    public Sprite TargetableOverlay { get { return targetableOverlay; } }
    public Sprite DangerOverlay { get { return dangerOverlay; } }
    public Sprite HoverOverlay { get { return hoverOverlay; } }
    public Sprite DisabledOverlay { get { return disabledOverlay; } }
    public Sprite BreakHintOverlay { get { return breakHintOverlay; } }
    public Sprite HealHintOverlay { get { return healHintOverlay; } }
}
