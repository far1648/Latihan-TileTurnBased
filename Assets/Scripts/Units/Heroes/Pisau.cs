using UnityEngine;

public class Pisau : BaseHero
{
    protected override void Awake()
    {
        MaxHealth = 80;
        AttackPower = 30;
        base.Awake();
    }
}
