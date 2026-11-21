using System.Collections.Generic;
using UnityEngine;

public abstract class Tile : MonoBehaviour
{
    public string TileName;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject hoverHighlight;
    [SerializeField] private GameObject moveHighlight;
    [SerializeField] private GameObject attackHighlight;
    [SerializeField] private bool isWalkable;

    public BaseUnit OccupiedUnit;
    public bool Walkable => isWalkable && OccupiedUnit == null;

    public virtual void Init(int x, int y)
    {
        
    }

    void OnMouseEnter()
    {
        if (hoverHighlight!=null) hoverHighlight.SetActive(true);
        MenuManager.Instance.ShowTileInfo(this);
    }

    void OnMouseExit()
    {
        if (hoverHighlight!=null) hoverHighlight.SetActive(false);
        MenuManager.Instance.ShowTileInfo(null);
    }

    void OnMouseDown()
    {
        if (TurnManager.Instance.IsBusy) return;

        switch (GameManager.Instance.GameState)
        {
            case GameState.HeroesTurn:
                HandleHeroTurnClick();
                break;
            case GameState.EnemiesTurn:
                HandleEnemyTurnClick();
                break;
        }
    }

    private void HandleHeroTurnClick()
    {
        var selected = UnitManager.Instance.SelectedHero;

        if (OccupiedUnit is BaseHero hero)
        {
            if (TurnManager.Instance.CanAct(hero))
                UnitManager.Instance.SetSelectedHero(hero);
        }
        else if (OccupiedUnit is BaseEnemy enemy) 
        {
            if (selected == null) return;

            if (!IsInAttackRange(selected))
            {
                UnitManager.Instance.SetSelectedHero(null);
                return;
            }

            enemy.TakeDamage(selected.AttackPower);

            UnitManager.Instance.SetSelectedHero(null);
            TurnManager.Instance.MarkActed(selected);
        }
        else if (selected != null)
        {
            if (!UnitManager.Instance.Reachable.ContainsKey(this))
            {
                UnitManager.Instance.SetSelectedHero(null);
                return;
            }

            if (!Walkable) return;

            var path = GridManager.Instance.BuildPath(UnitManager.Instance.Reachable, this);

            UnitManager.Instance.SetSelectedHero(null);
            MoveAndFinish(selected, path);
        }
    }

    private void HandleEnemyTurnClick()
    {
        var selected = UnitManager.Instance.SelectedEnemy;

        if (OccupiedUnit is BaseEnemy enemy)
        {
            if (TurnManager.Instance.CanAct(enemy))
                UnitManager.Instance.SetSelectedEnemy(enemy);
        }
        else if (OccupiedUnit is BaseHero hero) 
        {
            if (selected == null) return;

            if (!IsInAttackRange(selected))
            {
                UnitManager.Instance.SetSelectedEnemy(null);
                return;
            }

            hero.TakeDamage(selected.AttackPower);

            UnitManager.Instance.SetSelectedEnemy(null);
            TurnManager.Instance.MarkActed(selected);
        }
        else if (selected != null)
        {
            if (!UnitManager.Instance.Reachable.ContainsKey(this))
            {
                UnitManager.Instance.SetSelectedEnemy(null);
                return;
            }

            if (!Walkable) return;

            var path = GridManager.Instance.BuildPath(UnitManager.Instance.Reachable, this);

            UnitManager.Instance.SetSelectedEnemy(null);
            MoveAndFinish(selected, path);
        }
    }

    public void SetUnit(BaseUnit unit)
    {
        if (unit.OccupiedTile != null) unit.OccupiedTile.OccupiedUnit = null;

        unit.transform.position = transform.position;
        OccupiedUnit = unit;
        unit.OccupiedTile = this;
    }

    public int DistanceFrom(BaseUnit unit)
    {
        var from = unit.OccupiedTile.transform.position;
        var to = transform.position;
        return Mathf.RoundToInt(Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y));
    }

    public bool IsInAttackRange(BaseUnit unit) => DistanceFrom(unit) <= unit.AttackRange;

    public void SetMoveHighlight(bool active)
    {
        if (moveHighlight != null) moveHighlight.SetActive(active);
    }

    public void SetAttackHighlight(bool active)
    {
        if (attackHighlight != null) attackHighlight.SetActive(active);
    }

    public void MoveUnit(BaseUnit unit, List<Vector3> path, System.Action onComplete = null)
    {
        unit.CurrentStep = Mathf.Max(0, unit.CurrentStep - path.Count);

        if (unit.OccupiedTile != null) unit.OccupiedTile.OccupiedUnit = null;
        OccupiedUnit = unit;
        unit.OccupiedTile = this;

        MenuManager.Instance.RefreshTileInfo();

        unit.MoveAlong(path, onComplete);
    }

    private void MoveAndFinish(BaseUnit unit, List<Vector3> path)
    {
        TurnManager.Instance.IsBusy = true;

        MoveUnit(unit, path, () =>
        {
            TurnManager.Instance.IsBusy = false;

            if (UnitManager.Instance.CanStillAct(unit)) 
                UnitManager.Instance.SelectUnit(unit);
            else TurnManager.Instance.MarkActed(unit);
        });
    }
}