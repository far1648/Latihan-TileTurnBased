using UnityEngine;

public class Pisau : BaseHero
{
    protected override void Awake()
    {
        base.Awake();
        MaxHealth = 80;
        AttackPower = 30;
    }
}
