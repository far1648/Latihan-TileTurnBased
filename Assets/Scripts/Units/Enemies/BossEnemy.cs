using System.Linq;
using UnityEngine;

public class BossEnemy : BaseEnemy
{
    [Range(0f, 1f)] [SerializeField] private float breakChance = 0.7f;
    [SerializeField] private int breakCooldown = 1;
    [SerializeField] private int minTiles = 1;
    [SerializeField] private int maxTiles = 2;
    [SerializeField] private int breakRadius = 2;

    public int BreakCooldownLeft {get; set;}

    public int TryBreakTiles()
    {
        if (BreakCooldownLeft > 0)
        {
            BreakCooldownLeft--;
            return 0;
        }

        if (Random.value > breakChance) return 0;

        var heroes = UnitManager.Instance.GetUnits(Faction.Hero);
        var candidates = GridManager.Instance
            .GetTilesNear(heroes, breakRadius)
            .OrderBy(_ => Random.value)
            .ToList();

        int wanted = Random.Range(minTiles, maxTiles + 1);
        int broken = 0;

        foreach (var tile in candidates)
        {
            if (broken >= wanted) break;

            if (!GridManager.Instance.CanBreak(tile)) continue;
            if (tile.Break()) broken++;
        }

        if (broken > 0) BreakCooldownLeft = breakCooldown;
        return broken;
    }
}
