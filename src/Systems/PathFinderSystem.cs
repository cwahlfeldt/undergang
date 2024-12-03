using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Game.Systems
{
    public class PathFinderSystem
    {
        private readonly AStar3D _astar = new();
        private readonly EntityManager _entityManager;
        private readonly HexGridSystem _hexGridSystem;
        private Dictionary<Vector3I, Entity> _tiles;

        public PathFinderSystem(EntityManager entityManager, HexGridSystem hexGridSystem)
        {
            _entityManager = entityManager;
            _hexGridSystem = hexGridSystem;
            _tiles = new Dictionary<Vector3I, Entity>();
            SetupPathfinding();
        }

        public void SetupPathfinding()
        {
            _astar.Clear();
            // Get all hex tiles into our dictionary
            _tiles = _entityManager.GetHexGrid()
                .ToDictionary(
                    e => e.Get<HexCoordComponent>().HexCoord,
                    e => e
                );

            AddPoints();
            ConnectPoints();
        }

        private void AddPoints()
        {
            foreach (var (coord, tile) in _tiles)
            {
                if (tile.Get<HexTileComponent>().Type != TileType.Blocked)
                {
                    int index = tile.Get<HexTileComponent>().Index;
                    _astar.AddPoint(index, tile.Get<RenderComponent>().Node3D.Position);
                }
            }
        }

        private void ConnectPoints()
        {
            foreach (var (coord, tile) in _tiles)
            {
                int currentIndex = tile.Get<HexTileComponent>().Index;
                if (!_astar.HasPoint(currentIndex))
                    continue;

                foreach (var dir in HexGridSystem.Directions.Values)
                {
                    var neighborCoord = coord + dir;
                    if (_tiles.TryGetValue(neighborCoord, out var neighborTile))
                    {
                        int neighborIndex = neighborTile.Get<HexTileComponent>().Index;
                        if (_astar.HasPoint(neighborIndex) && !_astar.ArePointsConnected(currentIndex, neighborIndex))
                        {
                            _astar.ConnectPoints(currentIndex, neighborIndex);
                        }
                    }
                }
            }
        }

        public List<Vector3I> FindPath(Vector3I from, Vector3I to, int maxRange)
        {
            if (!_tiles.TryGetValue(from, out var fromTile) || !_tiles.TryGetValue(to, out var toTile))
                return [];

            int fromIndex = fromTile.Get<HexTileComponent>().Index;
            int toIndex = toTile.Get<HexTileComponent>().Index;

            if (!_astar.HasPoint(fromIndex) || !_astar.HasPoint(toIndex))
                return [];

            var path = _astar.GetPointPath(fromIndex, toIndex);
            if (path == null || path.Length == 0)
                return [];

            // Convert world positions back to coordinates
            var coordPath = path.Select(worldPos => _hexGridSystem.WorldToHex(worldPos)).ToList();

            // Limit by max range if specified
            if (maxRange > 0)
            {
                return coordPath.Take(maxRange + 1).ToList();
            }

            return coordPath;
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

                foreach (var dir in HexGridSystem.Directions.Values)
                {
                    var neighborCoord = current + dir;
                    if (_tiles.TryGetValue(neighborCoord, out var neighborTile) &&
                        !visited.Contains(neighborCoord) &&
                        neighborTile.Get<HexTileComponent>().Type != TileType.Blocked)
                    {
                        visited.Add(neighborCoord);
                        queue.Enqueue((neighborCoord, distance + 1));
                    }
                }
            }

            return reachable;
        }
    }
}