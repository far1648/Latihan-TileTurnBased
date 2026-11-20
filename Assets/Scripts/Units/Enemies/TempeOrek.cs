using UnityEngine;

public class TempeOrek : BaseEnemy
{
    protected override void Awake()
    {
        base.Awake();
        MaxHealth = 60;
        AttackPower = 20;
    }
}
