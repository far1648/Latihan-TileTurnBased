using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    
    public Faction ActiveFaction {get; private set;}

    private readonly HashSet<BaseUnit> actedUnits = new HashSet<BaseUnit>();

    void Awake()
    {
        Instance = this;
    }

    public void StartTurn(Faction faction)
    {
        ActiveFaction = faction;
        actedUnits.Clear();

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

        bool anyoneLeft = UnitManager.Instance
            .GetUnits(ActiveFaction)
            .Any(u => !actedUnits.Contains(u));

        if (!anyoneLeft) EndTurn();
    }

    private void EndTurn()
    {
        var nextState = ActiveFaction == Faction.Hero 
            ? GameState.EnemiesTurn
            : GameState.HeroesTurn;

        GameManager.Instance.ChangeState(nextState);
    }
}