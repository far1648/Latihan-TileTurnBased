using UnityEngine;

public class NasiKuning : BaseEnemy
{
    protected override void Awake()
    {
        MaxHealth = 60;
        AttackPower = 20;
        Weapon = WeaponType.Green;
        base.Awake();
    }

    protected override void Die()
    {
        TurnManager.Instance.SkipNextEnemyTurn = true;
        base.Die();
    }
}
