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
    public bool IsTerrainWalkable => isWalkable;
    [SerializeField] private Color threatColor = new Color(1f, 0.55f, 0f, 0.35f);
    [SerializeField] private Color targetColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private Color rangeColor = new Color(0.005f, 0f, 1f, 0.45f);
    [SerializeField] private Color pathColor = new Color(0.35f, 0.8f, 1f, 0.6f);
    [SerializeField] private Color destinationColor = new Color(0.2f, 1f, 0.4f, 0.75f);

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
                var position = UnitManager.Instance.FindAttackPosition(selected, this);

                if (position == null) {
                    UnitManager.Instance.SetSelectedHero(null);
                    return;
                }

                if (UnitManager.Instance.PendingMoveTile != position)
                {
                    UnitManager.Instance.PreviewMove(position);
                    return;
                }

                var path = GridManager.Instance.BuildPath(UnitManager.Instance.Reachable, position);
                
                UnitManager.Instance.SetSelectedHero(null);
                UnitActions.MoveAndAttack(selected, position, path, enemy);
                return;
            }

            UnitManager.Instance.SetSelectedHero(null); 
            UnitActions.Attack(selected, enemy);
        }
        else if (selected != null)
        {
            if (!UnitManager.Instance.Reachable.ContainsKey(this))
            {
                UnitManager.Instance.SetSelectedHero(null);
                return;
            }

            if (!Walkable) return;

            if (UnitManager.Instance.PendingMoveTile != this)
            {
                UnitManager.Instance.PreviewMove(this);
                return;
            }

            var path = GridManager.Instance.BuildPath(UnitManager.Instance.Reachable, this);

            UnitManager.Instance.SetSelectedHero(null);
            UnitActions.Move(selected, this, path, () => UnitActions.ContinueOrFinish(selected));
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
                var position = UnitManager.Instance.FindAttackPosition(selected, this);

                if (position == null)
                {
                    UnitManager.Instance.SetSelectedEnemy(null);
                    return;
                }

                if (UnitManager.Instance.PendingMoveTile != position)
                {
                    UnitManager.Instance.PreviewMove(position);
                    return;
                }

                var path = GridManager.Instance.BuildPath(UnitManager.Instance.Reachable, position);
                
                UnitManager.Instance.SetSelectedEnemy(null);
                UnitActions.MoveAndAttack(selected, position, path, hero);
                return;
            }

            UnitManager.Instance.SetSelectedEnemy(null);
            UnitActions.Attack(selected, hero);
        }
        else if (selected != null)
        {
            if (!UnitManager.Instance.Reachable.ContainsKey(this))
            {
                UnitManager.Instance.SetSelectedEnemy(null);
                return;
            }

            if (!Walkable) return;

            if (UnitManager.Instance.PendingMoveTile != this)
            {
                UnitManager.Instance.PreviewMove(this);
                return;
            }

            var path = GridManager.Instance.BuildPath(UnitManager.Instance.Reachable, this);

            UnitManager.Instance.SetSelectedEnemy(null);
            UnitActions.Move(selected, this, path, () => UnitActions.ContinueOrFinish(selected));
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

    public int DistanceTo(Tile other)
    {
        var a = transform.position;
        var b = other.transform.position;
        return Mathf.RoundToInt(Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y));
    }

    public bool IsInAttackRange(BaseUnit unit) => DistanceFrom(unit) <= unit.AttackRange;

    public void SetMoveHighlight(MoveHighlightMode mode)
    {
        if (moveHighlight == null) return;
        
        moveHighlight.SetActive(mode != MoveHighlightMode.None);
        if (mode == MoveHighlightMode.None) return;

        var sr = moveHighlight.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        switch (mode)
        {
            case MoveHighlightMode.Range: 
                sr.color = rangeColor;
                break;
            case MoveHighlightMode.Path:
                sr.color = pathColor;
                break;
            case MoveHighlightMode.Destination:
                sr.color = destinationColor;
                break;
        }
    }

    public void SetAttackHighlight(AttackHighlightMode mode)
    {
        if (attackHighlight == null) return;

        attackHighlight.SetActive(mode != AttackHighlightMode.None);
        if (mode == AttackHighlightMode.None) return;

        var sr = attackHighlight.GetComponent<SpriteRenderer>();
        if (sr != null) 
            sr.color = mode == AttackHighlightMode.Target ? targetColor : threatColor;
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

    

}

public enum AttackHighlightMode
{
    None,
    Threat,
    Target
}

public enum MoveHighlightMode
{
    None,
    Range,
    Path,
    Destination
}