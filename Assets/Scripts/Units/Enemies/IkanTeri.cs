using UnityEngine;

public class IkanTeri : BaseEnemy
{
    protected override void Awake()
    {
        MaxHealth = 60;
        AttackPower = 20;
        base.Awake();
    }
}
