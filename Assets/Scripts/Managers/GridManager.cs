using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    
    [SerializeField] private MapData map;
    public MapData Map => map;
    private int width, height;

    private readonly List<Tile> heroSpawnTiles = new List<Tile>();
    private readonly List<Tile> enemySpawnTiles = new List<Tile>();

    public IReadOnlyList<Tile> HeroSpawnTiles => heroSpawnTiles;
    public IReadOnlyList<Tile> EnemySpawnTiles => enemySpawnTiles;

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
        var parsed = map != null ? MapParser.Parse(map) : null;

        if (parsed == null) return;

        width = parsed.Width;
        height = parsed.Height;

        tiles = new Dictionary<Vector2, Tile>();
        heroSpawnTiles.Clear();
        enemySpawnTiles.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var prefab = parsed.Mountain[x, y] ? mountainTile : grassTile;

                var spawnedTile = Instantiate(prefab, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";

                spawnedTile.Init(x, y);

                tiles[new Vector2(x, y)] = spawnedTile;
            }
        }

        foreach (var p in parsed.HeroSpawns)
        {
            heroSpawnTiles.Add(tiles[new Vector2(p.x, p.y)]);
        }

        foreach (var p in parsed.EnemySpawns)
        {
            enemySpawnTiles.Add(tiles[new Vector2(p.x, p.y)]);
        }

        cam.transform.position = new Vector3((float)width/2 -0.5f, (float)height/2 - 0.5f, -10);

        GameManager.Instance.ChangeState(GameState.SpawnHeroes);
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

    public List<Tile> GetTilesNear(IEnumerable<BaseUnit> centers, int radius)
    {
        var result = new List<Tile>();

        var origin = centers
            .Where(c => c != null && c.OccupiedTile != null)
            .Select(c => c.OccupiedTile)
            .ToList();

        foreach(var tile  in tiles.Values)
        {
            if (!tile.Walkable) continue;

            if (origin.Any(o => o.DistanceTo(tile) <= radius)) result.Add(tile);
        }

        return result;
    }

    public bool CanBreak(Tile target)
    {
        if (target == null || !target.Walkable) return false;

        Tile start = null;
        int total = 0;

        foreach (var tile in tiles.Values)
        {
            if (tile == target || !tile.IsTerrainWalkable) continue;

            total++;
            if (start == null) start = tile;
        }

        var visited = new HashSet<Tile> { start };
        var queue = new Queue<Tile>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var next in GetNeighbours(current))
            {
                if (next == target || visited.Contains(next) || !next.IsTerrainWalkable) continue;

                visited.Add(next);
                queue.Enqueue(next);
            }
        }

        return visited.Count == total;
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