using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    [SerializeField] private int width, height;

    [SerializeField] private Tile grassTile, mountainTile;

    [SerializeField] private Camera cam;

    private Dictionary<Vector2, Tile> tiles;

    private static readonly Vector2[] Directions = 
        { Vector2.up, Vector2.down, Vector2.left, Vector2.right } ;

    void Awake()
    {
        Instance = this;
    }

    public void GenerateGrid()
    {
        tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var randomTile = Random.Range(0, 6) == 3 ? mountainTile : grassTile;

                var spawnedTile = Instantiate(randomTile, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";

                spawnedTile.Init(x, y);

                tiles[new Vector2(x, y)] = spawnedTile;
            }
        }

        cam.transform.position = new Vector3((float)width/2 -0.5f, (float)height/2 - 0.5f, -10);

        GameManager.Instance.ChangeState(GameState.SpawnHeroes);
    }

    public Tile GetHeroSpawnTile()
    {
        return tiles
            .Where(t=>t.Key.x < width/2 && t.Value.Walkable)
            .OrderBy(t=>Random.value)
            .Select(t => t.Value)
            .FirstOrDefault();
    }
    public Tile GetEnemySpawnTile()
    {
        return tiles
            .Where(t=>t.Key.x > width/2 && t.Value.Walkable)
            .OrderBy(t=>Random.value).Select(t => t.Value)
            .FirstOrDefault();
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (tiles.TryGetValue(pos, out var tile)) 
        {
            return tile;
        }
        return null;
    }

    public IEnumerable<Tile> GetAllTiles()
    {
        return tiles.Values;
    }

    private IEnumerable<Tile> GetNeighbours(Tile tile)
    {
        Vector2 pos = tile.transform.position;
        
        foreach (var dir in Directions)
        {
            var neighbour = GetTileAtPosition(pos + dir);
            if (neighbour != null) yield return neighbour;
        }
    }

    public Dictionary<Tile, Tile> FindReachable(Tile start, int maxSteps)
    {
        var cameFrom = new Dictionary<Tile, Tile> { [start] = null };
        var distance = new Dictionary<Tile, int> { [start] = 0 };
        var queue = new Queue<Tile>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (distance[current] >= maxSteps) continue;

            foreach (var next in GetNeighbours(current))
            {
                if (cameFrom.ContainsKey(next) || !next.Walkable) continue;

                cameFrom[next] = current;
                distance[next] = distance[current] + 1;
                queue.Enqueue(next);
            }
        }

        return cameFrom;
    }

    public Dictionary<Tile, int> FindDistanceMap(Tile source)
    {
        var distance = new Dictionary<Tile, int>() {[source] = 0};
        var queue = new Queue<Tile>();
        queue.Enqueue(source);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var next in GetNeighbours(current))
            {
                if (distance.ContainsKey(next) || !next.IsTerrainWalkable) continue;

                distance[next] = distance[current] + 1;
                queue.Enqueue(next);
            }
        } 

        return distance;
    }

    public List<Vector3> BuildPath(Dictionary<Tile, Tile> cameFrom, Tile target)
    {
        var path = new List<Vector3>();

        for (var tile = target; cameFrom[tile] != null; tile = cameFrom[tile])
            path.Add(tile.transform.position);
        
        path.Reverse();
        return path;
    }
}