using UnityEngine;

public class NasiKuning : BaseEnemy
{
    protected override void Awake()
    {
        base.Awake();
        MaxHealth = 60;
        AttackPower = 20;
    }
}
