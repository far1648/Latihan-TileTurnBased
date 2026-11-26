using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance;

    [SerializeField] private bool useAI = true;

    [SerializeField] private float turnStartDelay = 0.2f;
    [SerializeField] private float unitDelay = 0.2f;

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

        while (GameManager.Instance.GameState == GameState.EnemiesTurn)
        {
            var enemy = UnitManager.Instance
                .GetUnits(Faction.Enemy)
                .FirstOrDefault(u => u.CurrentHealth > 0 && TurnManager.Instance.CanAct(u));

            if (enemy == null) break;

            yield return new WaitForSeconds(unitDelay);
            
            if (enemy is BossEnemy boss)
            {
                if (boss.TryRewind())
                    yield return new WaitForSeconds(unitDelay * 4f);

                if (boss.TrySummon() > 0)
                    yield return new WaitForSeconds(unitDelay * 2f);

                if (boss.TryBreakTiles() > 0)
                    yield return new WaitForSeconds(unitDelay * 2f);
            }

            yield return ActWith(enemy);
        }
        if (GameManager.Instance.GameState == GameState.HeroesTurn)
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
            if (tile != enemy.OccupiedTile && !tile.Walkable) continue;
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
