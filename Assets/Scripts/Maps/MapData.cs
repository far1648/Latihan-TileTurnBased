using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Map", menuName = "Map data")]
public class MapData : ScriptableObject
{
    public string MapName = "Tutorial";
    [Tooltip("E = Rumput   M = Gunung   F = Food/Enemy   H = Hero")]
    [TextArea(6, 20)]
    public string layout = 
        "EEMEEMEE" +
        "MEEEFEEM" +
        "MEEEEEEM" +
        "MEEEEMEM" +
        "EEEEEEEE" +
        "EHEHEHEE";

    public List<ScriptableUnit> Heroes = new List<ScriptableUnit>();
    public List<ScriptableUnit> Enemies = new List<ScriptableUnit>();
}
