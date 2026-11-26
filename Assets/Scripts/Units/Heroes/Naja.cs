using UnityEngine;

public class Naja : BaseHero
{
    protected override void Awake()
    {
        MaxHealth = 100;
        AttackPower = 10;
        base.Awake();
    }
}
