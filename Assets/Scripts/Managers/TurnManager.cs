using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public Faction ActiveFaction {get; private set;}
    private readonly HashSet<BaseUnit> actedUnits = new HashSet<BaseUnit>();
    public bool IsBusy;
    public bool AITurnRunning;
    public bool InputBlocked => IsBusy || AITurnRunning;
    void Awake()
    {
        Instance = this;
    }

    public void StartTurn(Faction faction)
    {
        ActiveFaction = faction;
        actedUnits.Clear();

        foreach (var unit in UnitManager.Instance.GetUnits(faction))
        {
            unit.CurrentStep = unit.MaxStep;
        }

        UnitManager.Instance.SetSelectedHero(null);
        UnitManager.Instance.SetSelectedEnemy(null);
    }

    public bool CanAct(BaseUnit unit)
    {
        return unit.Faction == ActiveFaction && !actedUnits.Contains(unit);
    }

    public void MarkActed(BaseUnit unit)
    {
        actedUnits.Add(unit);

        if (CheckGameOver()) return;

        bool anyoneLeft = UnitManager.Instance
            .GetUnits(ActiveFaction)
            .Any(u => !actedUnits.Contains(u));

        if (!anyoneLeft) EndTurn();
    }

    private bool CheckGameOver()
    {
        bool heroesAlive = UnitManager.Instance.GetUnits(Faction.Hero).Any();
        bool enemyAlive = UnitManager.Instance.GetUnits(Faction.Enemy).Any();

        if (heroesAlive && enemyAlive) return false;

        GameManager.Instance.EndGame(heroesAlive ? Faction.Hero : Faction.Enemy);
        return true;
    }

    private void EndTurn()
    {
        var nextState = ActiveFaction == Faction.Hero 
            ? GameState.EnemiesTurn
            : GameState.HeroesTurn;

        GameManager.Instance.ChangeState(nextState);
    }

    public void EndTurnEarly()
    {
        if (InputBlocked) return;

        var state = GameManager.Instance.GameState;
        if (state != GameState.HeroesTurn && state != GameState.EnemiesTurn) return;

        EndTurn();
    }
}