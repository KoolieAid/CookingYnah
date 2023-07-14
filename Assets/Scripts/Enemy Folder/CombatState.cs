using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatState : MonsterState
{
    public CombatState(MonsterStateManager manager, Enemy enemy) : base(manager, enemy) { }

    public override void Enter()
    {
        enemy.ControlAnimations(MonsterStates.Combat, true);
    }

    public override void Update(float deltaTime)
    {
        if (!enemy.IsAlive())
        {
            statManager.ChangeState(enemy, new DeathState(statManager, enemy));
            return;
        }

        if (enemy.GetTargetUnit() == null)
        {
            statManager.ChangeState(enemy, new ChaseState(statManager, enemy));
            return;
        }

        float distanceToTarget = Vector3.Distance(enemy.transform.position, enemy.GetTargetUnit().transform.position);

        if (distanceToTarget > enemy.GetEnemyUnitData().AttackRange)
        {
            statManager.ChangeState(enemy, new ChaseState(statManager, enemy));
            return;
        }
        else if (distanceToTarget <= enemy.GetEnemyUnitData().AttackRange && enemy.GetCanAttack())
        {
            statManager.ChangeState(enemy, new AttackState(statManager, enemy));
            return;
        }
    }
    public override void Exit()
    {
        enemy.ControlAnimations(MonsterStates.Combat, false);
    }
}
