using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;

    private List<ScriptableUnit> units;

    public BaseHero SelectedHero;
    public BaseEnemy SelectedEnemy;
    [SerializeField] int heroCount = 3;
    [SerializeField] int enemyCount = 3;

    private readonly List<BaseUnit> allUnits = new List<BaseUnit>();
    
    void Awake()
    {
        Instance = this;

        units = Resources.LoadAll<ScriptableUnit>("Units").ToList();
    }

    public void SpawnHeroes()
    {
        foreach (var prefab in GetRandomUnit<BaseHero>(Faction.Hero, heroCount))
        {
            var spawnedHero = Instantiate(prefab);
            RegisterUnit(spawnedHero);
            var randomSpawnTile = GridManager.Instance.GetHeroSpawnTile();

            randomSpawnTile.SetUnit(spawnedHero);
        }

        GameManager.Instance.ChangeState(GameState.SpawnEnemies);
    }

    public void SpawnEnemies()
    {
        foreach (var prefab in GetRandomUnit<BaseEnemy>(Faction.Enemy, enemyCount))
        {
            var spawnedEnemy = Instantiate(prefab);
            RegisterUnit(spawnedEnemy);
            var randomSpawnTile = GridManager.Instance.GetEnemySpawnTile();

            randomSpawnTile.SetUnit(spawnedEnemy);
        }

        GameManager.Instance.ChangeState(GameState.HeroesTurn);
    }

    private List<T> GetRandomUnit<T>(Faction faction, int count) where T : BaseUnit
    {
        return units
            .Where(u => u.Faction == faction)
            .OrderBy(o => Random.value)
            .Take(count)
            .Select(u => (T)u.UnitPrefab)
            .ToList();
    }

    private void UpdateHighlights(BaseUnit unit)
    {
        foreach (var tile in GridManager.Instance.GetAllTiles())
        {
            bool inMoveRange = unit != null && tile.IsInMoveRange(unit);
            bool inAttackRange = unit != null && tile.IsInAttackRange(unit);

            bool canMove = inMoveRange && tile.Walkable;
            bool canAttack = inAttackRange 
                && tile.OccupiedUnit != null
                && tile.OccupiedUnit.Faction != unit.Faction;

            tile.SetMoveHighlight(canMove);
            tile.SetAttackHighlight(canAttack);
        }
    }

    public void SetSelectedHero(BaseHero hero)
    {
        SelectedHero = hero;
        MenuManager.Instance.ShowSelectedUnit(hero);
        UpdateHighlights(hero);
    }

    public void SetSelectedEnemy(BaseEnemy enemy)
    {
        SelectedEnemy = enemy;
        MenuManager.Instance.ShowSelectedUnit(enemy);
        UpdateHighlights(enemy);
    }

    public void RegisterUnit(BaseUnit unit) => allUnits.Add(unit);
   
    public void UnregisterUnit(BaseUnit unit) => allUnits.Remove(unit);
    
    public IEnumerable<BaseUnit> GetUnits(Faction faction) => allUnits.Where(u => u.Faction == faction);

    public bool CanStillAct(BaseUnit unit)
    {
        foreach (var tile in GridManager.Instance.GetAllTiles())
        {
            if (tile.Walkable && tile.IsInMoveRange(unit)) return true;

            if (tile.IsInAttackRange(unit)
                && tile.OccupiedUnit != null
                && tile.OccupiedUnit.Faction != unit.Faction) return true;
        }

        return false;
    }

    public void SelectUnit(BaseUnit unit)
    {
        if (unit is BaseHero hero) SetSelectedHero(hero);
        else if (unit is BaseEnemy enemy) SetSelectedEnemy(enemy);
    }
}