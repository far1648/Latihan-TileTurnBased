using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    [SerializeField] private GameObject selectedUnitObject, tileObject, tileUnitObject, gameOverObject;
    [SerializeField] private TextMeshProUGUI tileUnitNameText, tileUnitHpText, tileUnitAttText;

    private Tile hoveredTile;

    void Awake()
    {
        Instance = this;
    }

    public void ShowTileInfo(Tile tile)
    {
        hoveredTile = tile;

        if (tile == null)
        {
            tileObject.SetActive(false);
            tileUnitObject.SetActive(false);
            return;
        }

        tileObject.GetComponentInChildren<TextMeshProUGUI>().text = tile.TileName;
        tileObject.SetActive(true);

        if (tile.OccupiedUnit)
        {
            var unit = tile.OccupiedUnit;

            tileUnitNameText.text = unit.UnitName;
            tileUnitHpText.text = $"HP {unit.CurrentHealth}/{unit.MaxHealth}";
            tileUnitAttText.text = $"ATT {unit.AttackPower}";

            tileUnitObject.SetActive(true);
        }
    }

    public void ShowSelectedUnit(BaseUnit unit)
    {
        if (unit == null) 
        {
            selectedUnitObject.SetActive(false);
            return;
        }
        
        selectedUnitObject.GetComponentInChildren<TextMeshProUGUI>().text = unit.UnitName;
        selectedUnitObject.SetActive(true);
    }

    public void ShowGameOver(Faction winner)
    {
        gameOverObject.GetComponentInChildren<TextMeshProUGUI>().text = 
            winner == Faction.Hero ? "Hero Menang" : "Enemy Menang";
        gameOverObject.SetActive(true);
    }

    public void RefreshTileInfo()
    {
        ShowTileInfo(hoveredTile);
    }
}