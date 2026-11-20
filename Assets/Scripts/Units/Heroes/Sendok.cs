using UnityEngine;

public class Sendok : BaseHero
{
    protected override void Awake()
    {
        base.Awake();
        MaxHealth = 100;
        AttackPower = 10;
    }
}
