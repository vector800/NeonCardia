public static class HuntingLevelEvaluator
{
    public static HuntingLevel Evaluate(BattleResultData result)
    {
        if (result == null)
        {
            return HuntingLevel.B;
        }

        return result.IsBossBattle ? EvaluateBossBattle(result) : EvaluateNormalBattle(result);
    }

    public static HuntingLevel EvaluateNormalBattle(BattleResultData result)
    {
        if (result.VictoryTurn <= 1 || (result.VictoryTurn <= 2 && result.MaxSimultaneousDefeatCount >= 2))
        {
            return HuntingLevel.S;
        }

        if (result.VictoryTurn >= 3 || result.PlayerDamageTakenCount >= 2)
        {
            return HuntingLevel.B;
        }

        if (result.VictoryTurn == 2 || result.PlayerDamageTakenCount == 1)
        {
            return HuntingLevel.A;
        }

        return HuntingLevel.B;
    }

    public static HuntingLevel EvaluateBossBattle(BattleResultData result)
    {
        if (result.VictoryTurn <= 3)
        {
            return HuntingLevel.S;
        }

        if (result.VictoryTurn >= 5 || result.PlayerDamageTakenCount >= 4)
        {
            return HuntingLevel.B;
        }

        if (result.VictoryTurn == 4 || result.PlayerDamageTakenCount == 3)
        {
            return HuntingLevel.A;
        }

        return HuntingLevel.B;
    }
}
