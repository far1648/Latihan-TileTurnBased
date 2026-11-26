using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UndoManager : MonoBehaviour
{
    public static UndoManager Instance;
    [SerializeField] private int maxUndos = 3;

    private class UnitState 
    {
        public BaseUnit Unit;
        public bool Alive;
        public Tile Tile;
        public int Health;
        public int Step;
    }

    private class Snapshot
    {
        public List<UnitState> Units = new List<UnitState>();
        public HashSet<Tile> BrokenTiles = new HashSet<Tile>();
        public int BossCooldown;
    }

    private readonly List<Snapshot> history = new List<Snapshot>();

    public int UndosLeft {get; private set;}
    public bool HasPrevious => history.Count >= 2;

    public bool CanUndo =>
        UndosLeft > 0
        && HasPrevious
        && GameManager.Instance.GameState == GameState.HeroesTurn
        && !TurnManager.Instance.InputBlocked;

    public bool CanRewind =>
        HasPrevious
        && GameManager.Instance.GameState == GameState.EnemiesTurn
        && !TurnManager.Instance.IsBusy;

    void Awake()
    {
        Instance = this;
        UndosLeft = maxUndos;
    }
    
    public void Capture()
    {
        var snapshot = new Snapshot();

        foreach (var unit in UnitManager.Instance.EverSpawned)
        {
            snapshot.Units.Add(new UnitState
            {
               Unit = unit,
               Alive = unit.CurrentHealth > 0,
               Tile = unit.OccupiedTile,
               Health = unit.CurrentHealth,
               Step = unit.CurrentStep
            });
        }

        foreach (var tile in GridManager.Instance.GetAllTiles())
        {
            if (tile.IsBroken) snapshot.BrokenTiles.Add(tile);
        }
        
        var boss = FindBoss();
        snapshot.BossCooldown = boss != null ? boss.BreakCooldownLeft : 0;

        history.Add(snapshot);
    }

    public bool Undo()
    {
        if (!CanUndo) return false;

        history.RemoveAt(history.Count-1);
        var target = history[history.Count-1];

        Restore(target, true);
        UndosLeft--;
        
        return true;
    }

    public bool Rewind()
    {
        if (!CanRewind) return false;

        history.RemoveAt(history.Count-1);
        var target = history[history.Count-1];

        Restore(target, false);

        return true;
    }

    public bool TryGetHealthAtLastCapture(BaseUnit unit, out int health)
    {
        health = 0;
        if (history.Count == 0) return false;

        foreach (var s in history[history.Count-1].Units)
        {
            if (s.Unit != unit) continue;

            health = s.Health;
            return true;
        }

        return false;
    }

    private BossEnemy FindBoss()
    {
        return UnitManager.Instance.EverSpawned.OfType<BossEnemy>().FirstOrDefault();
    }

    private void Restore(Snapshot snapshot, bool asUndo)
    {
        foreach (var s in snapshot.Units) {
            var tile = s.Unit.OccupiedTile;

            if (tile != null && tile.OccupiedUnit == s.Unit) tile.OccupiedUnit = null;
        }

        foreach (var tile in GridManager.Instance.GetAllTiles())
        {
            bool shouldBeBroken = snapshot.BrokenTiles.Contains(tile);

            if (tile.IsBroken && !shouldBeBroken) tile.Repair();
            else if (!tile.IsBroken && shouldBeBroken) tile.Break();
        }

        foreach (var s in snapshot.Units)
        {
            var unit = s.Unit;

            UnitManager.Instance.SetAlive(unit, s.Alive);
            unit.CurrentHealth = s.Health;
            unit.CurrentStep = s.Step;
            
            if (!s.Alive) continue;

            s.Tile.OccupiedUnit = unit;
            unit.OccupiedTile = s.Tile;
            unit.transform.position = s.Tile.transform.position;
        }

        var boss = FindBoss();
        if (boss != null) boss.BreakCooldownLeft = snapshot.BossCooldown;

        if (asUndo)
        {
            GameManager.Instance.GameState = GameState.HeroesTurn;
            TurnManager.Instance.StartTurn(Faction.Hero);
        }

        else
        {
            RefillEnemySteps();
            UnitManager.Instance.SetSelectedEnemy(null);
        }

        MenuManager.Instance.RefreshTileInfo();
    }

    private void RefillEnemySteps()
    {
        foreach (var enemy in UnitManager.Instance.GetUnits(Faction.Enemy))
        {
            if (TurnManager.Instance.CanAct(enemy)) enemy.CurrentStep = enemy.MaxStep;
        }
    }
}
