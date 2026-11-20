using System;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Tile : MonoBehaviour
{
    public string TileName;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject hoverHighlight;
    [SerializeField] private GameObject moveHighlight;
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

            UnitManager.Instance.UnregisterUnit(enemy);
            Destroy(enemy.gameObject);
            OccupiedUnit = null;

            UnitManager.Instance.SetSelectedHero(null);
            TurnManager.Instance.MarkActed(selected);
        }
        else if (selected != null && Walkable)
        {
            if (!IsInMoveRange(selected))
            {
                UnitManager.Instance.SetSelectedHero(null);
                return;
            }

            if (!isWalkable) return;

            SetUnit(selected);

            UnitManager.Instance.SetSelectedHero(null);
            TurnManager.Instance.MarkActed(selected);
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

            UnitManager.Instance.UnregisterUnit(hero);
            Destroy(hero.gameObject);
            OccupiedUnit = null;

            UnitManager.Instance.SetSelectedEnemy(null);
            TurnManager.Instance.MarkActed(selected);
        }
        else if (selected != null && Walkable)
        {
            if (!IsInMoveRange(selected))
            {
                UnitManager.Instance.SetSelectedEnemy(null);
                return;
            }

            if (!isWalkable) return;

            SetUnit(selected);

            UnitManager.Instance.SetSelectedEnemy(null);
            TurnManager.Instance.MarkActed(selected);
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
}