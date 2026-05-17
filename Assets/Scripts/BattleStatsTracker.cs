public sealed class BattleStatsTracker
{
    public int PlayerDamageTakenCount { get; private set; }
    public int MaxSimultaneousDefeatCount { get; private set; }

    public void Reset()
    {
        PlayerDamageTakenCount = 0;
        MaxSimultaneousDefeatCount = 0;
    }

    public void RecordPlayerDamageTaken(int actualDamage)
    {
        if (actualDamage > 0)
        {
            PlayerDamageTakenCount++;
        }
    }

    public void RecordEnemyDefeatBatch(int defeatedCount)
    {
        if (defeatedCount > MaxSimultaneousDefeatCount)
        {
            MaxSimultaneousDefeatCount = defeatedCount;
        }
    }

    public BattleResultData CreateResultData(bool isBossBattle, int victoryTurn)
    {
        int defeatCount = MaxSimultaneousDefeatCount > 0 ? MaxSimultaneousDefeatCount : 1;
        return new BattleResultData(isBossBattle, victoryTurn, PlayerDamageTakenCount, defeatCount);
    }
}
