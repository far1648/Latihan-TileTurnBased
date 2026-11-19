using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;

    private List<ScriptableUnit> units;

    public BaseHero SelectedHero;
    [SerializeField] int heroCount = 3;
    [SerializeField] int enemyCount = 3;
    
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

    public void SetSelectedHero(BaseHero hero)
    {
        SelectedHero = hero;
        MenuManager.Instance.ShowSelectedHero(hero);
    }
}