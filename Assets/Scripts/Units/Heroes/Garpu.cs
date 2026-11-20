using UnityEngine;

public class Garpu : BaseHero
{
    protected override void Awake()
    {
        MaxHealth = 60;
        AttackPower = 20;
        base.Awake();
    }
}
