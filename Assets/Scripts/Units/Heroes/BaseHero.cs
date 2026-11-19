using UnityEngine;

public class BaseHero : BaseUnit
{
    void Awake()
    {
        Faction = Faction.Hero;
    }
}
