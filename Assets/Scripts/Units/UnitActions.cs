using System.Collections.Generic;
using UnityEngine;

public static class UnitActions
{
    public static void Attack(BaseUnit attacker, BaseUnit target)
    {
        target.TakeDamage(attacker.AttackDamage, attacker);
        TurnManager.Instance.MarkActed(attacker);
    }

    public static void Move(BaseUnit unit, Tile destination, List<Vector3> path, System.Action onArrive = null)
    {
        TurnManager.Instance.IsBusy = true;

        destination.MoveUnit(unit, path, () =>
        {
            TurnManager.Instance.IsBusy = false;
            onArrive?.Invoke();
        });
    }

    public static void ContinueOrFinish(BaseUnit unit)
    {
        if (UnitManager.Instance.CanStillAct(unit)) 
            UnitManager.Instance.SelectUnit(unit);
        else 
            TurnManager.Instance.MarkActed(unit);
    }

    public static void MoveAndAttack(BaseUnit unit, Tile destination, List<Vector3> path, BaseUnit target) 
    {
        Move(unit, destination, path, () =>
        {
            if (target != null
                && target.OccupiedTile != null
                && target.OccupiedTile.IsInAttackRange(unit))
            {
                Attack(unit, target);
            }
            else
            {
                ContinueOrFinish(unit);
            }
        });
    }
}