using UnityEngine;

public abstract class Tile : MonoBehaviour
{
    public string TileName;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    [SerializeField] private GameObject highlight;
    [SerializeField] private bool isWalkable;

    public BaseUnit OccupiedUnit;
    public bool Walkable => isWalkable && OccupiedUnit == null;

    public virtual void Init(int x, int y)
    {
        
    }

    void OnMouseEnter()
    {
        if (highlight!=null) highlight.SetActive(true);
        MenuManager.Instance.ShowTileInfo(this);
    }

    void OnMouseExit()
    {
        if (highlight!=null) highlight.SetActive(false);
        MenuManager.Instance.ShowTileInfo(null);
    }

    void OnMouseDown()
    {
        if (GameManager.Instance.GameState != GameState.HeroesTurn) return;

        var selected = UnitManager.Instance.SelectedHero;

        if (OccupiedUnit is BaseHero hero)
        {
            if (TurnManager.Instance.CanAct(hero))
                UnitManager.Instance.SetSelectedHero(hero);
        }
        else if (OccupiedUnit is BaseEnemy enemy) 
        {
            if (selected == null) return;

            UnitManager.Instance.UnregisterUnit(enemy);
            Destroy(enemy.gameObject);
            OccupiedUnit = null;

            UnitManager.Instance.SetSelectedHero(null);
            TurnManager.Instance.MarkActed(selected);
        }
        else if (selected != null && Walkable)
        {
            SetUnit(selected);

            UnitManager.Instance.SetSelectedHero(null);
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
}
