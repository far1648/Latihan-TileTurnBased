using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParsedMap
{
    public int Width;
    public int Height;
    public bool[,] Mountain;
    public List<Vector2Int> HeroSpawns = new List<Vector2Int>();
    public List<Vector2Int> EnemySpawns = new List<Vector2Int>();
}

public static class MapParser
{
    public static ParsedMap Parse(MapData data)
    {
        var rows = data.layout
            .Replace("\r", "")
            .Split('\n')
            .Select(r => r.Trim())
            .Where(r => r.Length >0)
            .ToList();

            if (rows.Count == 0)
        {
            return null;
        }

        var map = new ParsedMap
        {
            Width = rows.Max(r => r.Length),
            Height = rows.Count
        };
        map.Mountain = new bool[map.Width, map.Height];

        for (int row = 0; row < map.Height; row++)
        {
            string line = rows[row];
            int y = map.Height - 1 - row;

            for (int x = 0; x < map.Width; x++)
            {
                char c = x < line.Length ? line[x] : 'M';

                switch (char.ToUpper(c))
                {
                    case 'E':
                        break;
                    case 'M':
                        map.Mountain[x, y] = true;
                        break;
                    case 'H': 
                        map.HeroSpawns.Add(new Vector2Int(x, y));
                        break;
                    case 'F':
                        map.EnemySpawns.Add(new Vector2Int(x, y));
                        break;
                    default:
                        break;
                }
            }
        }

        return map;
    }

}