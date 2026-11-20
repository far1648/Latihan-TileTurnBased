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
}
