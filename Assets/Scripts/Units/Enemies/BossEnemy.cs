using System.Linq;
using UnityEngine;

public class BossEnemy : BaseEnemy
{
    [Range(0f, 1f)] [SerializeField] private float breakChance = 0.7f;
    [SerializeField] private int breakCooldown = 1;
    [SerializeField] private int minTiles = 1;
    [SerializeField] private int maxTiles = 2;
    [SerializeField] private int breakRadius = 2;

    [Range(0f, 1f)] [SerializeField] private float rewindHpThreshold = 0.5f;
    [SerializeField] private int maxRewinds = 3;

    public int BreakCooldownLeft {get; set;}
    public int RewindsLeft {get; private set;}

    protected override void Awake()
    {
        base.Awake();
        RewindsLeft = maxRewinds;
    }

    public bool TryRewind()
    {
        if (RewindsLeft <= 0 || CurrentHealth <= 0) return false;

        var undo = UndoManager.Instance;
        if (undo == null || !undo.CanRewind) return false;

        if (!undo.TryGetHealthAtLastCapture(this, out int healthAtRoundStart)) return false;

        float limit = MaxHealth * rewindHpThreshold;
        bool crossedThreshold = healthAtRoundStart > limit && CurrentHealth <= limit;
        if (!crossedThreshold) return false;

        if (!undo.Rewind()) return false;

        RewindsLeft--;
        return true;
    }

    public int TryBreakTiles()
    {
        if (BreakCooldownLeft > 0)
        {
            BreakCooldownLeft--;
            return 0;
        }

        if (Random.value > breakChance) return 0;

        var heroes = UnitManager.Instance.GetUnits(Faction.Hero).ToList();
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
