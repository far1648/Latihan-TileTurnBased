using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState GameState;
    public Faction Winner {get; private set;}

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ChangeState(GameState.GenerateGrid);
    }

    public void EndGame(Faction winner)
    {
        Winner = winner;
        ChangeState(GameState.GameOver);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ChangeState(GameState newState)
    {
        GameState = newState;
        switch (newState)
        {
            case GameState.GenerateGrid:
                GridManager.Instance.GenerateGrid();
                break;
            case GameState.SpawnHeroes:
                UnitManager.Instance.SpawnHeroes();
                break;
            case GameState.SpawnEnemies:
                UnitManager.Instance.SpawnEnemies();
                break;
            case GameState.HeroesTurn:
                TurnManager.Instance.StartTurn(Faction.Hero);
                if (HeroAI.Instance != null && HeroAI.Instance.UseAI)
                {
                    HeroAI.Instance.BeginTurn();
                }
                break;
            case GameState.EnemiesTurn:
                TurnManager.Instance.StartTurn(Faction.Enemy);
                if (EnemyAI.Instance != null && EnemyAI.Instance.UseAI) {
                    EnemyAI.Instance.BeginTurn();
                }
                break;
            case GameState.GameOver:
                MenuManager.Instance.ShowGameOver(Winner);  
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(GameState), newState, null);
        }
    }
}

public enum GameState
{
    GenerateGrid = 0,
    SpawnHeroes = 1,
    SpawnEnemies = 2,
    HeroesTurn = 3,
    EnemiesTurn = 4,

    GameOver = 5
}