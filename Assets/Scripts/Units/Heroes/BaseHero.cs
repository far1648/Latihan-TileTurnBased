using UnityEngine;

public class BaseHero : BaseUnit
{
    protected override void Awake()
    {
        base.Awake();
        Faction = Faction.Hero;
    }
}
