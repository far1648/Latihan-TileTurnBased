using UnityEngine;

public class TelurDadar : BaseEnemy
{
    protected override void Awake()
    {
        base.Awake();
        MaxHealth = 60;
        AttackPower = 20;
    }
}
