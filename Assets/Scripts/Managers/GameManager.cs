using System;
using UnityEngine;

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
                break;
            case GameState.EnemiesTurn:
                TurnManager.Instance.StartTurn(Faction.Enemy);
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