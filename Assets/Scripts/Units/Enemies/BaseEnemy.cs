using UnityEngine;

public class BaseEnemy : BaseUnit
{
    protected override void Awake()
    {
        base.Awake();
        Faction = Faction.Enemy;
    }
}
