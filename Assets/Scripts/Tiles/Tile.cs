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

            if (!IsInMoveRange(selected))
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
            if (!IsInMoveRange(selected))
            {
                UnitManager.Instance.SetSelectedHero(null);
                return;
            }

            if (!Walkable) return;

            UnitManager.Instance.SetSelectedHero(null);
            MoveAndFinish(selected);
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

            if (!IsInMoveRange(selected))
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
            if (!IsInMoveRange(selected))
            {
                UnitManager.Instance.SetSelectedEnemy(null);
                return;
            }

            if (!Walkable) return;

            UnitManager.Instance.SetSelectedEnemy(null);
            MoveAndFinish(selected);
        }
    }

    public void SetUnit(BaseUnit unit)
    {
        if (unit.OccupiedTile != null) unit.OccupiedTile.OccupiedUnit = null;

        unit.transform.position = transform.position;
        OccupiedUnit = unit;
        unit.OccupiedTile = this;
    }

    public bool IsInMoveRange(BaseUnit unit)
    {
        var from  = unit.OccupiedTile.transform.position;
        var to = transform.position;

        int distance = Mathf.RoundToInt(Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y));
        return distance <= unit.MaxStep;
    }

    public void SetMoveHighlight(bool active)
    {
        if (moveHighlight != null) moveHighlight.SetActive(active);
    }

    public void SetAttackHighlight(bool active)
    {
        if (attackHighlight != null) attackHighlight.SetActive(active);
    }


    public void MoveUnit(BaseUnit unit, System.Action onComplete = null)
    {
        var path = BuildPath(unit.transform.position, transform.position);

        if (unit.OccupiedTile != null) unit.OccupiedTile.OccupiedUnit = null;
        OccupiedUnit = unit;
        unit.OccupiedTile = this;

        MenuManager.Instance.RefreshTileInfo();

        unit.MoveAlong(path, onComplete);
    }

    private List<Vector3> BuildPath(Vector3 from, Vector3 to)
    {
        var path = new List<Vector3>();

        int x = Mathf.RoundToInt(from.x);
        int y = Mathf.RoundToInt(from.y);
        int targetX = Mathf.RoundToInt(to.x);
        int targetY = Mathf.RoundToInt(to.y);

        while (x!=targetX)
        {
            x += x < targetX? 1 : -1;
            path.Add(new Vector3(x, y, to.z));
        }

        while (y!=targetY)
        {
            y += y < targetY? 1 : -1;
            path.Add(new Vector3(x, y, to.z));
        }

        return path;
    }

    private void MoveAndFinish(BaseUnit unit)
    {
        TurnManager.Instance.IsBusy = true;

        MoveUnit(unit, () =>
        {
            TurnManager.Instance.IsBusy = false;
            TurnManager.Instance.MarkActed(unit);
        });
    }
}