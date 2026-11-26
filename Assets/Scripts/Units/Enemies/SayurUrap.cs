using UnityEngine;

public class SayurUrap : BaseEnemy
{
    [Range(0f,1f)] [SerializeField] private float healRatio = 0.5f;
    protected override void Awake()
    {
        MaxHealth = 60;
        AttackPower = 20;
        Weapon = WeaponType.Blue;
        base.Awake();
    }

    protected override void Die()
    {
        foreach (var hero in UnitManager.Instance.GetUnits(Faction.Hero))
        {
            int amount = Mathf.CeilToInt(hero.MaxHealth * healRatio);
            hero.CurrentHealth = Mathf.Min(hero.CurrentHealth + amount, hero.MaxHealth);
        }
        base.Die();
    }
}
