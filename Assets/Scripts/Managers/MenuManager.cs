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

            string hp = $"HP {unit.CurrentHealth}/{unit.MaxHealth}";
            if (unit is BossEnemy boss) hp += $"\nSHIELD {boss.Shield}/{boss.MaxShield}";
            tileUnitHpText.text = hp;

            string att = $"ATT {unit.AttackPower}";
            if (unit.HasAttackBuff) att += $"\nBUFF ({unit.BuffTurnsLeft})";
            tileUnitAttText.text = att;

            tileUnitObject.SetActive(true);
        } 
        else
        {
            tileUnitObject.SetActive(false);
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
            winner == Faction.Hero ? "Tunggu 3 detik" : "Enemy Menang";
        gameOverObject.SetActive(true);
    }

    public void RefreshTileInfo()
    {
        ShowTileInfo(hoveredTile);
    }
}