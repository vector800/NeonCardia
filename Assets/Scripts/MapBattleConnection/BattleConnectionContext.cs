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
        : this(
            enemyGroupId,
            returnSceneName,
            string.Empty,
            new Vector3Int(returnPosition.x, 0, returnPosition.y),
            encounterAreaId,
            stepCountAtEncounter,
            string.Empty,
            string.Empty,
            0,
            Vector2Int.down)
    {
    }

    public BattleStartData(
        string enemyGroupId,
        string returnSceneName,
        string returnMapId,
        Vector3Int returnCell,
        string encounterAreaId,
        int stepCountAtEncounter,
        string battleBackgroundId,
        string battleBgmId,
        int encounterCooldownStepsAfterBattle,
        Vector2Int playerDirection)
    {
        EnemyGroupId = string.IsNullOrEmpty(enemyGroupId) ? "default" : enemyGroupId;
        ReturnSceneName = string.IsNullOrEmpty(returnSceneName) ? "MapBattleConnectionTest" : returnSceneName;
        ReturnMapId = string.IsNullOrEmpty(returnMapId) ? string.Empty : returnMapId;
        ReturnCell = returnCell;
        ReturnPosition = new Vector2Int(returnCell.x, returnCell.z);
        EncounterAreaId = string.IsNullOrEmpty(encounterAreaId) ? "default" : encounterAreaId;
        StepCountAtEncounter = Mathf.Max(0, stepCountAtEncounter);
        BattleBackgroundId = string.IsNullOrEmpty(battleBackgroundId) ? string.Empty : battleBackgroundId;
        BattleBgmId = string.IsNullOrEmpty(battleBgmId) ? string.Empty : battleBgmId;
        EncounterCooldownStepsAfterBattle = Mathf.Max(0, encounterCooldownStepsAfterBattle);
        PlayerDirection = playerDirection == Vector2Int.zero ? Vector2Int.down : playerDirection;
    }

    public string EnemyGroupId { get; private set; }
    public string ReturnSceneName { get; private set; }
    public string ReturnMapId { get; private set; }
    public Vector2Int ReturnPosition { get; private set; }
    public Vector3Int ReturnCell { get; private set; }
    public string EncounterAreaId { get; private set; }
    public int StepCountAtEncounter { get; private set; }
    public string BattleBackgroundId { get; private set; }
    public string BattleBgmId { get; private set; }
    public int EncounterCooldownStepsAfterBattle { get; private set; }
    public Vector2Int PlayerDirection { get; private set; }
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
        ReturnMapId = startData.ReturnMapId;
        ReturnPosition = startData.ReturnPosition;
        ReturnCell = startData.ReturnCell;
        EncounterAreaId = startData.EncounterAreaId;
        StepCountAtEncounter = startData.StepCountAtEncounter;
        BattleBackgroundId = startData.BattleBackgroundId;
        BattleBgmId = startData.BattleBgmId;
        EncounterCooldownStepsAfterBattle = startData.EncounterCooldownStepsAfterBattle;
        PlayerDirection = startData.PlayerDirection;
        VictoryTurn = Mathf.Max(0, victoryTurn);
        PlayerDamageTakenCount = Mathf.Max(0, playerDamageTakenCount);
        MaxSimultaneousDefeatCount = Mathf.Max(0, maxSimultaneousDefeatCount);
    }

    public BattleConnectionResultType ResultType { get; private set; }
    public string EnemyGroupId { get; private set; }
    public string ReturnSceneName { get; private set; }
    public string ReturnMapId { get; private set; }
    public Vector2Int ReturnPosition { get; private set; }
    public Vector3Int ReturnCell { get; private set; }
    public string EncounterAreaId { get; private set; }
    public int StepCountAtEncounter { get; private set; }
    public string BattleBackgroundId { get; private set; }
    public string BattleBgmId { get; private set; }
    public int EncounterCooldownStepsAfterBattle { get; private set; }
    public Vector2Int PlayerDirection { get; private set; }
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
