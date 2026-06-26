using System;
using UnityEngine;

public enum BattleConnectionResultType
{
    Win,
    Lose,
    Escape
}

public sealed class BattleStartData
{
    public BattleStartData(string enemyGroupId, string returnSceneName, Vector2Int returnPosition, string encounterAreaId, int stepCountAtEncounter)
    {
        EnemyGroupId = string.IsNullOrEmpty(enemyGroupId) ? "default" : enemyGroupId;
        ReturnSceneName = string.IsNullOrEmpty(returnSceneName) ? "MapBattleConnectionTest" : returnSceneName;
        ReturnPosition = returnPosition;
        EncounterAreaId = string.IsNullOrEmpty(encounterAreaId) ? "default" : encounterAreaId;
        StepCountAtEncounter = Mathf.Max(0, stepCountAtEncounter);
    }

    public string EnemyGroupId { get; private set; }
    public string ReturnSceneName { get; private set; }
    public Vector2Int ReturnPosition { get; private set; }
    public string EncounterAreaId { get; private set; }
    public int StepCountAtEncounter { get; private set; }
}

public sealed class BattleConnectionResultData
{
    public BattleConnectionResultData(BattleStartData startData, BattleConnectionResultType resultType, int victoryTurn, int playerDamageTakenCount, int maxSimultaneousDefeatCount)
    {
        if (startData == null)
        {
            throw new ArgumentNullException("startData");
        }

        ResultType = resultType;
        EnemyGroupId = startData.EnemyGroupId;
        ReturnSceneName = startData.ReturnSceneName;
        ReturnPosition = startData.ReturnPosition;
        EncounterAreaId = startData.EncounterAreaId;
        StepCountAtEncounter = startData.StepCountAtEncounter;
        VictoryTurn = Mathf.Max(0, victoryTurn);
        PlayerDamageTakenCount = Mathf.Max(0, playerDamageTakenCount);
        MaxSimultaneousDefeatCount = Mathf.Max(0, maxSimultaneousDefeatCount);
    }

    public BattleConnectionResultType ResultType { get; private set; }
    public string EnemyGroupId { get; private set; }
    public string ReturnSceneName { get; private set; }
    public Vector2Int ReturnPosition { get; private set; }
    public string EncounterAreaId { get; private set; }
    public int StepCountAtEncounter { get; private set; }
    public int VictoryTurn { get; private set; }
    public int PlayerDamageTakenCount { get; private set; }
    public int MaxSimultaneousDefeatCount { get; private set; }
}

public static class BattleConnectionContext
{
    private static BattleStartData activeStartData;
    private static BattleConnectionResultData lastResultData;

    public static bool HasActiveBattle
    {
        get { return activeStartData != null; }
    }

    public static BattleStartData ActiveStartData
    {
        get { return activeStartData; }
    }

    public static void BeginBattle(BattleStartData startData)
    {
        if (startData == null)
        {
            throw new ArgumentNullException("startData");
        }

        activeStartData = startData;
        lastResultData = null;
    }

    public static BattleConnectionResultData CompleteBattle(BattleConnectionResultType resultType, int victoryTurn, int playerDamageTakenCount, int maxSimultaneousDefeatCount)
    {
        if (activeStartData == null)
        {
            return null;
        }

        lastResultData = new BattleConnectionResultData(activeStartData, resultType, victoryTurn, playerDamageTakenCount, maxSimultaneousDefeatCount);
        activeStartData = null;
        return lastResultData;
    }

    public static bool TryConsumeResultForScene(string sceneName, out BattleConnectionResultData resultData)
    {
        resultData = null;
        if (lastResultData == null || !string.Equals(lastResultData.ReturnSceneName, sceneName, StringComparison.Ordinal))
        {
            return false;
        }

        resultData = lastResultData;
        lastResultData = null;
        return true;
    }

    public static void Clear()
    {
        activeStartData = null;
        lastResultData = null;
    }
}
