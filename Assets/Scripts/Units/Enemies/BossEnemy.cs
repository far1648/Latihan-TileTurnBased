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

    [SerializeField] private Color warningTint = new Color(1f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color lethalTint = new Color(1f, 0.2f, 0.2f, 1f);

    private SpriteRenderer[] bodyRenderers;
    private Color[] bodyColors;
    private int oneShotStage;

    public int OneShotStage
    {
        get => oneShotStage;
        set
        {
            oneShotStage = value;
            ApplyTint();
        }
    }

    public bool OneShotLethal => oneShotStage == 2;

    public int BreakCooldownLeft {get; set;}
    public int RewindsLeft {get; private set;}
    public int SummonTurnLeft {get; set;}

    protected override void Awake()
    {
        bodyRenderers = GetComponentsInChildren<SpriteRenderer>();
        bodyColors = bodyRenderers.Select(r => r.color).ToArray();

        base.Awake();
        RewindsLeft = maxRewinds;
        SummonTurnLeft = summonEvery;
        Shield = maxShield;
    }

    public void AdvanceOneShot()
    {
        if (oneShotStage == 0) return;

        OneShotStage = oneShotStage == 1 ? 2 : 0;
    }

    private void ApplyTint()
    {
        if (bodyRenderers == null) return;

        Color tint = oneShotStage == 2 ? lethalTint
                    : oneShotStage == 1 ? warningTint
                    : Color.white;
        
        for (int i = 0; i < bodyRenderers.Length; i++)
        {
            if (bodyRenderers[i] == null) continue;

            bodyRenderers[i].color = bodyColors[i] * tint;
        }
    }

    public override int DamageAgainst(BaseUnit target)
    {
        int damage = base.DamageAgainst(target);

        if (OneShotLethal && target != null) {
            damage = Mathf.Max(damage, target.CurrentHealth);
        }

        return damage;
    }

    public override void TakeDamage(int amount, BaseUnit attacker = null)
    {
        if (Shield <= 0)
        {
            base.TakeDamage(amount, attacker);
            return;
        }

        bool buffed = attacker != null && attacker.HasAttackBuff;

        int effective = buffed
            ? amount
            : Mathf.Max(1, Mathf.RoundToInt(amount * shieldDamageWithoutBuff));

        Shield = Mathf.Max(0, Shield - effective);

        if (Shield == 0 && oneShotStage == 0) OneShotStage = 1;

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
