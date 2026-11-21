using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    public string UnitName;
    public Tile OccupiedTile;
    public Faction Faction;
    public int MaxStep = 5;
    public int CurrentStep;
    public int AttackRange = 1;

    public int MaxHealth;
    public int CurrentHealth {get; set;}
    public int AttackPower;
    public float MoveSpeed = 6f;

    protected virtual void Awake()
    {
        CurrentHealth = MaxHealth;
        CurrentStep = MaxStep;
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

        if (CurrentHealth == 0) Die();

        MenuManager.Instance.RefreshTileInfo();
    }

    protected virtual void Die()
    {
        UnitManager.Instance.UnregisterUnit(this);

        if (OccupiedTile != null) OccupiedTile.OccupiedUnit = null;

        Destroy(gameObject);
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