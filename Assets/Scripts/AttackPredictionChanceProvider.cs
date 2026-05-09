public interface IAttackPredictionChanceProvider
{
    float GetPredictionChance(EnemyType enemyType);
}

public sealed class TestAttackPredictionChanceProvider : IAttackPredictionChanceProvider
{
    public float GetPredictionChance(EnemyType enemyType)
    {
        return 1f;
    }
}
