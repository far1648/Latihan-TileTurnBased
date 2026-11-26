using UnityEngine;

public class Testing : MonoBehaviour
{
    [SerializeField] private bool refillHeroHealth = true;

    void Update()
    {
#if UNITY_EDITOR

    if (!refillHeroHealth) return;
    if (GameManager.Instance == null || UnitManager.Instance == null) return;
    if (GameManager.Instance.GameState != GameState.HeroesTurn) return;

    bool changed = false;

    foreach (var hero in UnitManager.Instance.GetUnits(Faction.Hero))
        {
            if (hero.CurrentHealth >= hero.MaxHealth) continue;
            
            hero.CurrentHealth = hero.MaxHealth;
            changed = true;
        }
        
        if (changed) MenuManager.Instance.RefreshTileInfo();
#endif        
    }
}
