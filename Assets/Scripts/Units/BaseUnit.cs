using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    None = 0,
    Red = 1,
    Green = 2,
    Blue = 3
}

public class BaseUnit : MonoBehaviour
{
    public const float AdvantageMultiplier = 1.5f;
    public const float DisadvantageMultiplier = 0.75f;

    public WeaponType Weapon = WeaponType.None;
    public string UnitName;
    public Tile OccupiedTile;
    public Faction Faction;
    public int MaxStep = 3;
    public int CurrentStep;
    public int AttackRange = 1;

    public int MaxHealth;
    public int CurrentHealth {get; set;}
    public int AttackPower;
    public float MoveSpeed = 6f;

    private int buffDuration = 3;
    private int buffDamageMultiplier = 2;

    public int BuffTurnsLeft {get; set;}
    public bool HasAttackBuff => BuffTurnsLeft > 0;
    public int AttackDamage => AttackPower * (HasAttackBuff ? buffDamageMultiplier : 1);

    public BaseUnit LastAttacker {get; private set;}

    protected virtual void Awake()
    {
        CurrentHealth = MaxHealth;
        CurrentStep = MaxStep;

        if (GetComponent<UnitBars>() == null) gameObject.AddComponent<UnitBars>();
    }

    public void GrantAttackBuff()
    {
        BuffTurnsLeft = buffDuration;
    }

    public void TickBuff()
    {
        if (BuffTurnsLeft > 0) BuffTurnsLeft--;
    }

    public static int GetMatchup(WeaponType attacker, WeaponType defender)
    {
        if (attacker == WeaponType.None || defender == WeaponType.None || attacker == defender) return 0;

        bool advantage =
            (attacker == WeaponType.Red && defender == WeaponType.Green) ||
            (attacker == WeaponType.Green && defender == WeaponType.Blue) ||
            (attacker == WeaponType.Blue && defender == WeaponType.Red);

        return advantage ? 1 : -1;
    }

    public int MatchupAgainst(BaseUnit target)
    {
        return target == null ? 0 : GetMatchup(Weapon, target.Weapon);
    }

    public virtual int DamageAgainst(BaseUnit target)
    {
        int matchup = MatchupAgainst(target);
        float multiplier = matchup > 0 ? AdvantageMultiplier
                        : matchup < 0 ? DisadvantageMultiplier
                        : 1f;

        return Mathf.Max(1, Mathf.RoundToInt(AttackDamage * multiplier));
    }

    public virtual void TakeDamage(int amount, BaseUnit attacker = null)
    {
        LastAttacker = attacker;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

        if (CurrentHealth == 0) Die();

        MenuManager.Instance.RefreshTileInfo();
    }

    protected virtual void Die()
    {
        UnitManager.Instance.UnregisterUnit(this);

        if (OccupiedTile != null) OccupiedTile.OccupiedUnit = null;

        gameObject.SetActive(false);
    }

    public void MoveAlong(List<Vector3> path, System.Action onComplete = null)
    {
        StartCoroutine(MoveRoutine(path, onComplete));
    }

    private IEnumerator MoveRoutine(List<Vector3> path, System.Action onComplete)
    {
        foreach (var point in path)
        {
            while (transform.position != point)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, point, MoveSpeed * Time.deltaTime
                );
                yield return null;
            }
        }

        onComplete?.Invoke();
    }
}