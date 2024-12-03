namespace Game;

using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PathFinder
{
    private readonly AStar3D _astar;
    private readonly Dictionary<Vector3I, HexTile> _tiles;

    public PathFinder(Dictionary<Vector3I, HexTile> tiles)
    {
        _astar = new AStar3D();
        _tiles = tiles;
        SetupPathfinding();
    }

    public void SetupPathfinding()
    {
        _astar.Clear();
        AddPoints();
        ConnectPoints();
    }

    private void AddPoints()
    {
        foreach (var kvp in _tiles)
        {
            var coord = kvp.Key;
            var tile = kvp.Value;
            // Only add walkable tiles
            if (tile.TileType != TileType.Wall && tile.TileType != TileType.Lava)
            {
                // Use a unique index based on the hex coordinates
                int index = GetIndexFromCoord(coord);
                _astar.AddPoint(index, tile.Position);
            }
        }
    }

    private void ConnectPoints()
    {
        foreach (var kvp in _tiles)
        {
            var coord = kvp.Key;
            int currentIndex = GetIndexFromCoord(coord);

            // Skip if this node wasn't added to the graph
            if (!_astar.HasPoint(currentIndex))
                continue;

            // Check all neighbors
            foreach (var dir in HexGridSystem.Directions.Values)
            {
                var neighborCoord = coord + dir;
                if (_tiles.ContainsKey(neighborCoord))
                {
                    int neighborIndex = GetIndexFromCoord(neighborCoord);
                    if (_astar.HasPoint(neighborIndex) && !_astar.ArePointsConnected(currentIndex, neighborIndex))
                    {
                        _astar.ConnectPoints(currentIndex, neighborIndex);
                    }
                }
            }
        }
    }

    public List<Vector3I> FindPath(Vector3I from, Vector3I to)
    {
        int fromIndex = GetIndexFromCoord(from);
        int toIndex = GetIndexFromCoord(to);

        if (!_astar.HasPoint(fromIndex) || !_astar.HasPoint(toIndex))
            return new List<Vector3I>();

        var path = _astar.GetPointPath(fromIndex, toIndex);
        return path.Select(worldPos => _tiles.First(t => t.Value.Position == worldPos).Key).ToList();
    }

    public List<Vector3I> GetReachableCoords(Vector3I start, int range)
    {
        var reachable = new List<Vector3I>();
        var visited = new HashSet<Vector3I>();
        var queue = new Queue<(Vector3I coord, int distance)>();

        queue.Enqueue((start, 0));
        visited.Add(start);

        while (queue.Count > 0)
        {
            var (current, distance) = queue.Dequeue();
            reachable.Add(current);

            if (distance >= range)
                continue;

            foreach (var dir in HexGrid.Directions.Values)
            {
                var neighborCoord = current + dir;
                if (_tiles.TryGetValue(neighborCoord, out var neighborTile) &&
                    !visited.Contains(neighborCoord) &&
                    neighborTile.TileType != TileType.Wall &&
                    neighborTile.TileType != TileType.Lava)
                {
                    visited.Add(neighborCoord);
                    queue.Enqueue((neighborCoord, distance + 1));
                }
            }
        }

        return reachable;
    }

    private int GetIndexFromCoord(Vector3I coord)
    {
        // Shift coordinates to ensure positive values
        const int OFFSET = 100; // Large enough to handle your grid size
        int x = coord.X + OFFSET;
        int y = coord.Y + OFFSET;
        int z = coord.Z + OFFSET;

        // Create unique positive index
        return x * 10000 + y * 100 + z;
    }
}