using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnit : MonoBehaviour
{
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

    private int buffDuration = 2;
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

    public virtual int DamageAgainst(BaseUnit target)
    {
        return AttackDamage;
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