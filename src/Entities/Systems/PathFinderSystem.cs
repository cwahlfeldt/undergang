using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Undergang.Entities.Systems
{
    public class PathFinderSystem
    {
        private readonly AStar3D _astar;
        private readonly EntityManager _entityManager;
        private readonly HexGridSystem _hexGridSystem;

        public PathFinderSystem(EntityManager entityManager, HexGridSystem hexGridSystem)
        {
            _astar = new AStar3D();
            _entityManager = entityManager;
            _hexGridSystem = hexGridSystem;
            SetupPathfinding();
        }

        // Initializes the pathfinding graph based on walkable tiles
        public void SetupPathfinding()
        {
            _astar.Clear();
            AddPoints();
            ConnectPoints();
        }

        // Adds walkable tiles to the pathfinding graph
        private void AddPoints()
        {
            var tiles = _entityManager.GetEntities().Values
                .Where(e => e.Has<HexTileComponent>() && e.Has<HexCoordComponent>());

            foreach (var tile in tiles)
            {
                var tileType = tile.Get<HexTileComponent>().Type;
                var coord = tile.Get<HexCoordComponent>().HexCoord;

                // Only add walkable tiles to the pathfinding graph
                if (tileType != TileType.Wall && tileType != TileType.Lava)
                {
                    int index = GetIndexFromCoord(coord);
                    var worldPos = tile.Get<RenderComponent>().Node3D.Position;
                    _astar.AddPoint(index, worldPos);
                }
            }
        }

        // Connects neighboring walkable tiles in the pathfinding graph
        private void ConnectPoints()
        {
            var tiles = _entityManager.GetEntities().Values
                .Where(e => e.Has<HexTileComponent>() && e.Has<HexCoordComponent>());

            foreach (var tile in tiles)
            {
                var coord = tile.Get<HexCoordComponent>().HexCoord;
                int currentIndex = GetIndexFromCoord(coord);

                // Skip if this node wasn't added to the graph
                if (!_astar.HasPoint(currentIndex))
                    continue;

                // Check all neighbors
                foreach (var dir in HexGridSystem.Directions.Values)
                {
                    var neighborCoord = coord + dir;
                    var neighborTile = _hexGridSystem.GetTileAtCoordinate(neighborCoord);

                    if (neighborTile != null)
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

        // Finds a path between two coordinates, returning a list of hex coordinates
        public List<Vector3I> FindPath(Vector3I from, Vector3I to)
        {
            int fromIndex = GetIndexFromCoord(from);
            int toIndex = GetIndexFromCoord(to);

            if (!_astar.HasPoint(fromIndex) || !_astar.HasPoint(toIndex))
                return new List<Vector3I>();

            var path = _astar.GetPointPath(fromIndex, toIndex);
            
            // Convert world positions back to hex coordinates
            return path.Select(worldPos => _hexGridSystem.WorldToHex(worldPos)).ToList();
        }

        // Gets all coordinates reachable within a certain range
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
                    var neighborTile = _hexGridSystem.GetTileAtCoordinate(neighborCoord);

                    if (neighborTile != null && 
                        !visited.Contains(neighborCoord) && 
                        neighborTile.Get<HexTileComponent>().Type != TileType.Wall && 
                        neighborTile.Get<HexTileComponent>().Type != TileType.Lava)
                    {
                        visited.Add(neighborCoord);
                        queue.Enqueue((neighborCoord, distance + 1));
                    }
                }
            }

            return reachable;
        }

        // Generates a unique index for each hex coordinate
        private int GetIndexFromCoord(Vector3I coord)
        {
            const int OFFSET = 100; // Large enough to handle your grid size
            int x = coord.X + OFFSET;
            int y = coord.Y + OFFSET;
            int z = coord.Z + OFFSET;

            // Create unique positive index
            return x * 10000 + y * 100 + z;
        }
    }
}