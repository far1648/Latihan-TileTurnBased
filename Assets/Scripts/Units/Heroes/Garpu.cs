using UnityEngine;

public class Garpu : BaseHero
{
    protected override void Awake()
    {
        MaxHealth = 60;
        AttackPower = 20;
        AttackRange = 2;
        Weapon = WeaponType.Blue;
        base.Awake();
    }
}
