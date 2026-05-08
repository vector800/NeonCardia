using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    MeleeEnemy,
    ShooterEnemy,
    GuardEnemy
}

public sealed class EnemyAI
{
    private readonly EnemyType enemyType;
    private int turnCount;

    public EnemyAI(EnemyType enemyType)
    {
        this.enemyType = enemyType;
    }

    public void Act(CharacterUnit player, CharacterUnit enemy, IEnumerable<CharacterUnit> units, BattleLog battleLog)
    {
        turnCount++;

        switch (enemyType)
        {
            case EnemyType.MeleeEnemy:
                ActAsMelee(player, enemy, units, battleLog);
                break;
            case EnemyType.ShooterEnemy:
                ActAsShooter(player, enemy, units, battleLog);
                break;
            case EnemyType.GuardEnemy:
                ActAsGuard(player, enemy, units, battleLog);
                break;
            default:
                ActAsMelee(player, enemy, units, battleLog);
                break;
        }
    }

    private void ActAsMelee(CharacterUnit player, CharacterUnit enemy, IEnumerable<CharacterUnit> units, BattleLog battleLog)
    {
        if (enemy.Position.Row == player.Position.Row)
        {
            UseAttack(player, true, 6, "エネミーはクローを使用。", battleLog);
            return;
        }

        BattleGridPosition nextPosition = enemy.Position.Offset(enemy.Position.Row < player.Position.Row ? 1 : -1, 0);
        if (TryMove(enemy, nextPosition, units, battleLog))
        {
            return;
        }

        UseAttack(player, false, 4, "エネミーはクローを使用。", battleLog);
    }

    private void ActAsShooter(CharacterUnit player, CharacterUnit enemy, IEnumerable<CharacterUnit> units, BattleLog battleLog)
    {
        if (enemy.Hp <= 14)
        {
            BattleGridPosition retreat = enemy.Position.Offset(0, BattleGridPosition.BackColumnDelta(enemy.Position.Side));
            if (TryMove(enemy, retreat, units, battleLog))
            {
                return;
            }
        }

        if (enemy.Position.Row == player.Position.Row)
        {
            UseAttack(player, true, 7, "エネミーはショットを使用。", battleLog);
            return;
        }

        BattleGridPosition nextPosition = enemy.Position.Offset(enemy.Position.Row < player.Position.Row ? 1 : -1, 0);
        if (!TryMove(enemy, nextPosition, units, battleLog))
        {
            UseAttack(player, false, 4, "エネミーはショットを使用。", battleLog);
        }
    }

    private void ActAsGuard(CharacterUnit player, CharacterUnit enemy, IEnumerable<CharacterUnit> units, BattleLog battleLog)
    {
        if (turnCount % 3 == 0 && enemy.Position.Row == player.Position.Row)
        {
            UseAttack(player, true, 8, "エネミーはカウンターショットを使用。", battleLog);
            return;
        }

        if (turnCount % 3 == 0)
        {
            UseAttack(player, false, 8, "エネミーはカウンターショットを使用。", battleLog);
            return;
        }

        enemy.Guard += 6;
        battleLog.Add("エネミーは身構えた。");
        battleLog.Add("エネミーのガード +6。");
    }

    private static bool TryMove(CharacterUnit unit, BattleGridPosition destination, IEnumerable<CharacterUnit> units, BattleLog battleLog)
    {
        if (!destination.IsValid)
        {
            return false;
        }

        foreach (CharacterUnit other in units)
        {
            if (other != unit && other.Position.Side == destination.Side && other.Position.Row == destination.Row && other.Position.Column == destination.Column)
            {
                return false;
            }
        }

        unit.MoveTo(destination);
        battleLog.Add(unit.Name + "は" + destination + "へ移動しました。");
        return true;
    }

    private static void UseAttack(CharacterUnit target, bool hasTarget, int damage, string actionText, BattleLog battleLog)
    {
        battleLog.Add(actionText);
        if (!hasTarget)
        {
            battleLog.Add("しかし攻撃範囲内にプレイヤーはいなかった。");
            battleLog.Add("攻撃は空振りした。");
            return;
        }

        DealDamage(target, damage, battleLog);
    }

    private static void DealDamage(CharacterUnit target, int damage, BattleLog battleLog)
    {
        int blocked;
        int actualDamage = target.TakeDamage(damage, out blocked);
        if (blocked > 0)
        {
            battleLog.Add("プレイヤーは" + blocked + "ダメージをガードし、" + actualDamage + "ダメージを受けた。");
        }
        else
        {
            battleLog.Add("プレイヤーに" + actualDamage + "ダメージ。");
        }
    }
}
