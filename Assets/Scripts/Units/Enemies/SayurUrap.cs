using UnityEngine;

public class SayurUrap : BaseEnemy
{
    [SerializeField] private int healAmount = 20;
    protected override void Awake()
    {
        MaxHealth = 60;
        AttackPower = 20;
        base.Awake();
    }

    protected override void Die()
    {
        foreach (var hero in UnitManager.Instance.GetUnits(Faction.Hero))
        {
            hero.CurrentHealth = Mathf.Max(hero.CurrentHealth + healAmount, hero.MaxHealth);
        }
        base.Die();
    }
}
