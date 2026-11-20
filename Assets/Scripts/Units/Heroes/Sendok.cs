using UnityEngine;

public class Sendok : BaseHero
{
    protected override void Awake()
    {
        MaxHealth = 100;
        AttackPower = 10;
        base.Awake();
    }
}
