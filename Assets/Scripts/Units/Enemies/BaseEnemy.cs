using UnityEngine;

public class BaseEnemy : BaseUnit
{
    void Awake()
    {
        Faction = Faction.Enemy;
    }
}
