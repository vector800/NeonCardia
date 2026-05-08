public sealed class EnemyAI
{
    private int turnCount;

    public void Act(CharacterUnit player, CharacterUnit enemy, BattlePosition playerPosition, BattleLog battleLog)
    {
        turnCount++;

        if (enemy.Hp <= 14 && enemy.Guard == 0 && turnCount % 2 == 0)
        {
            enemy.Guard += 6;
            battleLog.Add("Enemy Drone braces. Enemy Guard +6.");
            return;
        }

        int damage = GetDamageForPosition(playerPosition);
        int blocked;
        int actualDamage = player.TakeDamage(damage, out blocked);
        if (blocked > 0)
        {
            battleLog.Add("Enemy attacks for " + damage + ". Guard blocks " + blocked + "; player takes " + actualDamage + ".");
        }
        else
        {
            battleLog.Add("Enemy attacks. Player takes " + actualDamage + ".");
        }
    }

    private static int GetDamageForPosition(BattlePosition playerPosition)
    {
        switch (playerPosition)
        {
            case BattlePosition.Back:
                return 4;
            case BattlePosition.Middle:
                return 6;
            case BattlePosition.Front:
                return 8;
            default:
                return 6;
        }
    }
}
