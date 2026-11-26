using UnityEngine;

public class Wiguna : BaseHero
{
    protected override void Awake()
    {
        MaxHealth = 80;
        AttackPower = 30;
        Weapon = WeaponType.Red;
        base.Awake();
    }
}
