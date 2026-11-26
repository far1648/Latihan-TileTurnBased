using UnityEngine;

public class IkanTeri : BaseEnemy
{
    protected override void Awake()
    {
        MaxHealth = 60;
        AttackPower = 20;
        base.Awake();
    }

    protected override void Die()
    {
        foreach (var hero in UnitManager.Instance.GetUnits(Faction.Hero))
        {
            hero.GrantAttackBuff();
        }
        base.Die();
    }
}
