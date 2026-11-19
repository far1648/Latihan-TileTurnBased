using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Scriptable unit")]

public class ScriptableUnit : ScriptableObject
{
    public Faction Faction;
}

public enum Faction
{
    Hero = 0,
    Enemy = 1
}
