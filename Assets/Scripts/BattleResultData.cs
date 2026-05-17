public enum HuntingLevel
{
    S,
    A,
    B
}

public sealed class BattleResultData
{
    public BattleResultData(bool isBossBattle, int victoryTurn, int playerDamageTakenCount, int maxSimultaneousDefeatCount)
    {
        IsBossBattle = isBossBattle;
        VictoryTurn = victoryTurn < 1 ? 1 : victoryTurn;
        PlayerDamageTakenCount = playerDamageTakenCount < 0 ? 0 : playerDamageTakenCount;
        MaxSimultaneousDefeatCount = maxSimultaneousDefeatCount < 0 ? 0 : maxSimultaneousDefeatCount;
        HuntingLevel = HuntingLevel.B;
    }

    public bool IsBossBattle { get; private set; }
    public int VictoryTurn { get; private set; }
    public int PlayerDamageTakenCount { get; private set; }
    public int MaxSimultaneousDefeatCount { get; private set; }
    public HuntingLevel HuntingLevel { get; set; }
}
