using UnityEngine;

public class Garpu : BaseHero
{
    protected override void Awake()
    {
        base.Awake();
        MaxHealth = 60;
        AttackPower = 20;
    }
}
