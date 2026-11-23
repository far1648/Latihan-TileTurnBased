using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeroAI : MonoBehaviour
{
    public static HeroAI Instance;

    [SerializeField] private bool useAI;

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

        var heroes = UnitManager.Instance.GetUnits(Faction.Hero).ToList();

        foreach (var hero in heroes)
        {
            if (GameManager.Instance.GameState != GameState.HeroesTurn) break;

            if (hero == null || hero.CurrentHealth <= 0) continue;
            if (!TurnManager.Instance.CanAct(hero)) continue;

            yield return new WaitForSeconds(unitDelay);

            yield return ActWith(hero);
        }

        if (GameManager.Instance.GameState == GameState.EnemiesTurn)
            TurnManager.Instance.AITurnRunning = false;
        running = null;
    }

    private IEnumerator ActWith(BaseUnit hero)
    {
        UnitManager.Instance.SelectUnit(hero);
        yield return new WaitForSeconds(unitDelay);

        var target = PickTarget(hero, out var position);

        if (target == null)
        {
            yield return Approach(hero);
        }

        else if (position == hero.OccupiedTile)
        {
            UnitManager.Instance.SetSelectedHero(null);
            UnitActions.Attack(hero, target);
        }
        
        else
        {
            var path = GridManager.Instance.BuildPath(UnitManager.Instance.Reachable, position);

            UnitManager.Instance.SetSelectedHero(null);

            UnitActions.MoveAndAttack(hero, position, path, target);

            while (TurnManager.Instance.IsBusy) yield return null;
        }

        if (TurnManager.Instance.CanAct(hero))
            TurnManager.Instance.MarkActed(hero);

    }

    private BaseUnit PickTarget(BaseUnit hero, out Tile position)
    {
        BaseUnit best = null;
        position = null;

        foreach (var enemy in UnitManager.Instance.GetUnits(Faction.Enemy))
        {
            var attackTile = UnitManager.Instance.FindAttackPosition(hero, enemy.OccupiedTile);
            if (attackTile == null) continue;
            
            if (best == null || IsBetterTarget(hero, enemy, best))
            {
                best = enemy;
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

    private IEnumerator Approach(BaseUnit hero)
    {
        Dictionary<Tile, int> bestMap = null;
        int bestDistance = int.MaxValue;

        foreach(var enemy in UnitManager.Instance.GetUnits(Faction.Enemy))
        {
            var map = GridManager.Instance.FindDistanceMap(enemy.OccupiedTile);

            if (!map.TryGetValue(hero.OccupiedTile, out int d)) continue;

            if (d < bestDistance)
            {
                bestDistance = d;
                bestMap = map;
            }
        }

        if (bestMap == null)
        {
            UnitManager.Instance.SetSelectedHero(null);
            yield break;
        }

        Tile destination = hero.OccupiedTile;
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

        if (destination == hero.OccupiedTile)
        {
            UnitManager.Instance.SetSelectedHero(null);
            yield break;
        }

        var path = GridManager.Instance.BuildPath(UnitManager.Instance.Reachable, destination);
        UnitManager.Instance.SetSelectedHero(null);

        UnitActions.Move(hero, destination, path);

        while (TurnManager.Instance.IsBusy) yield return null;
    }
}
