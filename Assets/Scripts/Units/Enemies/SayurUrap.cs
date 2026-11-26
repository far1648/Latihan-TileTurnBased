using UnityEngine;

public class SayurUrap : BaseEnemy
{
    protected override void Awake()
    {
        MaxHealth = 60;
        AttackPower = 20;
        base.Awake();
    }
}
