using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    public string UnitName;
    public Tile OccupiedTile;
    public Faction Faction;
    public int MaxStep = 3;

    public int MaxHealth;
    public int CurrentHealth {get; set;}
    public int AttackPower;

    protected virtual void Awake()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(int amount)
    {
        Debug.Log($"{name}: HP {CurrentHealth}/{MaxHealth}, damage {amount}");

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

        if (CurrentHealth == 0) Die();
    }

    protected virtual void Die()
    {
        UnitManager.Instance.UnregisterUnit(this);

        if (OccupiedTile != null) OccupiedTile.OccupiedUnit = null;

        Destroy(gameObject);
    }
}
