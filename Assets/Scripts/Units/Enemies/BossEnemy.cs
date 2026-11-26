using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossEnemy : BaseEnemy
{
    [Range(0f, 1f)] [SerializeField] private float breakChance = 0.7f;
    [SerializeField] private int breakCooldown = 2;
    [SerializeField] private int minTiles = 1;
    [SerializeField] private int maxTiles = 2;
    [SerializeField] private int breakRadius = 2;

    [Range(0f, 1f)] [SerializeField] private float rewindHpThreshold = 0.5f;
    [SerializeField] private int maxRewinds = 1;

    [SerializeField] private List<BaseUnit> minionPrefabs = new List<BaseUnit>();
    [SerializeField] private int summonEvery = 3;
    [SerializeField] private int summonCount = 2;
    [SerializeField] private int summonRadius = 2;

    [SerializeField] private int maxShield = 200;
    [Range(0f, 1f)] [SerializeField] private float shieldDamageWithoutBuff = 0.5f;

    public int MaxShield => maxShield;
    public int Shield {get; set;}

    public int BreakCooldownLeft {get; set;}
    public int RewindsLeft {get; private set;}
    public int SummonTurnLeft {get; set;}

    protected override void Awake()
    {
        base.Awake();
        RewindsLeft = maxRewinds;
        SummonTurnLeft = summonEvery;
        Shield = maxShield;
    }

    public override void TakeDamage(int amount, BaseUnit attacker = null)
    {
        if (Shield <= 0)
        {
            base.TakeDamage(amount, attacker);
        }

        bool buffed = attacker != null && attacker.HasAttackBuff;

        int effective = buffed
            ? amount
            : Mathf.Max(1, Mathf.RoundToInt(amount * shieldDamageWithoutBuff));

        Shield = Mathf.Max(0, Shield - effective);

        MenuManager.Instance.RefreshTileInfo();
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

    public int TrySummon()
    {
        if (minionPrefabs == null || minionPrefabs.Count == 0) return 0;

        SummonTurnLeft = Mathf.Max(0, SummonTurnLeft-1);
        if (SummonTurnLeft > 0) return 0;

        var tiles = GridManager.Instance
            .GetTilesNear(new BaseUnit[] { this }, summonRadius)
            .OrderBy(_ => Random.value)
            .ToList();

        int spawned = 0;

        foreach (var tile in tiles)
        {
            if (spawned >= summonCount) break;
            var prefab = minionPrefabs[Random.Range(0, minionPrefabs.Count)];
            var minion = UnitManager.Instance.SpawnUnit(prefab, tile);
            if (minion == null) continue;

            TurnManager.Instance.RegisterAsActed(minion);
            spawned++;
        }

        if (spawned > 0) SummonTurnLeft = summonEvery;

        return spawned;
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
