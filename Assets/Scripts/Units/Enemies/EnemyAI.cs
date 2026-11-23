using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance;

    [SerializeField] private bool useAI = true;

    [SerializeField] private float turnStartDelay = 0.6f;
    [SerializeField] private float unitDelay = 0.5f;

    public bool UseAI => useAI;

    private Coroutine running;

    private void Awake()
    {
        Instance = this;
    }

    public void BeginTurn()
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(RunTurn());
    }

    private IEnumerator RunTurn()
    {
        TurnManager.Instance.AITurnRunning = true;

        yield return new WaitForSeconds(turnStartDelay);

        var enemies = UnitManager.Instance.GetUnits(Faction.Enemy).ToList();

        foreach (var enemy in enemies)
        {
            if (GameManager.Instance.GameState != GameState.EnemiesTurn) break;

            if (enemy == null || enemy.CurrentHealth <= 0) continue;
            if (!TurnManager.Instance.CanAct(enemy)) continue;

            yield return new WaitForSeconds(unitDelay);

            yield return ActWith(enemy);
        }

        TurnManager.Instance.AITurnRunning = false;
        running = null;
    }

    private IEnumerator ActWith(BaseUnit enemy)
    {
        UnitManager.Instance.SelectUnit(enemy);
        yield return new WaitForSeconds(unitDelay);

        var target = PickTarget(enemy, out var position);

        if (target == null)
        {
            yield return Approach(enemy);
        }

        else if (position == enemy.OccupiedTile)
        {
            UnitManager.Instance.SetSelectedEnemy(null);
            UnitActions.Attack(enemy, target);
        }
        
        else
        {
            var path = GridManager.Instance.BuildPath(UnitManager.Instance.Reachable, position);

            UnitManager.Instance.SetSelectedEnemy(null);

            UnitActions.MoveAndAttack(enemy, position, path, target);

            while (TurnManager.Instance.IsBusy) yield return null;
        }

        if (TurnManager.Instance.CanAct(enemy))
            TurnManager.Instance.MarkActed(enemy);

    }

    private BaseUnit PickTarget(BaseUnit enemy, out Tile position)
    {
        BaseUnit best = null;
        position = null;

        foreach (var hero in UnitManager.Instance.GetUnits(Faction.Hero))
        {
            var attackTile = UnitManager.Instance.FindAttackPosition(enemy, hero.OccupiedTile);
            if (attackTile == null) continue;
            
            if (best == null || IsBetterTarget(enemy, hero, best))
            {
                best = hero;
                position = attackTile;
            }
        }

        return best;
    }

    private bool IsBetterTarget(BaseUnit attacker, BaseUnit a, BaseUnit b)
    {
        bool aKill = a.CurrentHealth <= attacker.AttackPower;
        bool bKill = b.CurrentHealth <= attacker.AttackPower;
        if (aKill != bKill) return aKill;

        if (a.CurrentHealth != b.CurrentHealth) return a.CurrentHealth < b.CurrentHealth;

        var from = attacker.OccupiedTile;
        return a.OccupiedTile.DistanceTo(from) < b.OccupiedTile.DistanceTo(from);
    }

    private IEnumerator Approach(BaseUnit enemy)
    {
        Dictionary<Tile, int> bestMap = null;
        int bestDistance = int.MaxValue;

        foreach(var hero in UnitManager.Instance.GetUnits(Faction.Hero))
        {
            var map = GridManager.Instance.FindDistanceMap(hero.OccupiedTile);

            if (!map.TryGetValue(enemy.OccupiedTile, out int d)) continue;

            if (d < bestDistance)
            {
                bestDistance = d;
                bestMap = map;
            }
        }

        if (bestMap == null)
        {
            UnitManager.Instance.SetSelectedEnemy(null);
            yield break;
        }

        Tile destination = enemy.OccupiedTile;
        int destinationDistance = bestDistance;

        foreach(var tile in UnitManager.Instance.Reachable.Keys)
        {
            if (!bestMap.TryGetValue(tile, out int d)) continue;

            if (d < destinationDistance)
            {
                destination = tile;
                destinationDistance = d;
            }
        }

        if (destination == enemy.OccupiedTile)
        {
            UnitManager.Instance.SetSelectedEnemy(null);
            yield break;
        }

        var path = GridManager.Instance.BuildPath(UnitManager.Instance.Reachable, destination);
        UnitManager.Instance.SetSelectedEnemy(null);

        UnitActions.Move(enemy, destination, path);

        while (TurnManager.Instance.IsBusy) yield return null;
    }
}
