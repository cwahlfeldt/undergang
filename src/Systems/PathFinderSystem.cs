using System.Collections.Generic;
using System.Linq;
using Godot;
using Game.Autoload;

namespace Game.Systems
{
    public class PathFinderSystem
    {
        private readonly AStar3D _astar = new();
        private readonly EntityManager _entityManager;
        private Dictionary<Vector3I, Entity> _tiles = [];

        public PathFinderSystem(EntityManager entityManager)
        {
            EventBus.Instance.MoveCompleted += OnMoveCompleted;
            _entityManager = entityManager;
            SetupPathfinding();
        }

        public void SetupPathfinding()
        {
            _astar.Clear();
            _tiles = _entityManager
                .GetTiles()
                .ToDictionary(
                    entity => entity.Get<TileComponent>().Coord,
                    entity => entity
                );

            AddPoints();
            ConnectPoints();
        }

        private void AddPoints()
        {
            foreach (var (coord, tile) in _tiles)
            {
                var tileComponent = tile.Get<TileComponent>();
                if (tileComponent.Type != TileType.Blocked)
                {
                    int index = tileComponent.Index;
                    _astar.AddPoint(index, tileComponent.Node.Position);
                }
            }
        }

        private void ConnectPoints()
        {
            foreach (var (coord, tile) in _tiles)
            {
                int currentIndex = tile.Get<TileComponent>().Index;
                if (!_astar.HasPoint(currentIndex))
                    continue;

                foreach (var dir in HexGrid.Directions.Values)
                {
                    var neighborCoord = coord + dir;
                    if (_tiles.TryGetValue(neighborCoord, out var neighborTile))
                    {
                        if (_entityManager.IsTileOccupied(neighborCoord))
                            continue;

                        int neighborIndex = neighborTile.Get<TileComponent>().Index;
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

            int fromIndex = fromTile.Get<TileComponent>().Index;
            int toIndex = toTile.Get<TileComponent>().Index;

            if (!_astar.HasPoint(fromIndex) || !_astar.HasPoint(toIndex))
                return [];

            var path = _astar.GetPointPath(fromIndex, toIndex);
            if (path == null || path.Length == 0)
                return [];

            var coordPath = path.Select(worldPos => HexGrid.WorldToHex(worldPos)).ToList();
            return maxRange > 0 ? coordPath.Take(maxRange + 1).ToList() : coordPath;
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
                        neighborTile.Get<TileComponent>().Type != TileType.Blocked &&
                        !_entityManager.IsTileOccupied(neighborCoord))
                    {
                        visited.Add(neighborCoord);
                        queue.Enqueue((neighborCoord, distance + 1));
                    }
                }
            }

            return reachable;
        }

        private void OnMoveCompleted(Entity entity, Vector3I fromCoord, Vector3I toCoord)
        {
            UpdateConnectionsForTile(fromCoord);
            UpdateConnectionsForTile(toCoord);
        }

        private void UpdateConnectionsForTile(Vector3I coord)
        {
            if (!_tiles.TryGetValue(coord, out var tile))
                return;

            int tileIndex = tile.Get<TileComponent>().Index;

            // Disconnect existing connections
            foreach (var dir in HexGrid.Directions.Values)
            {
                var neighborCoord = coord + dir;
                if (_tiles.TryGetValue(neighborCoord, out var neighborTile))
                {
                    int neighborIndex = neighborTile.Get<TileComponent>().Index;
                    if (_astar.ArePointsConnected(tileIndex, neighborIndex))
                    {
                        _astar.DisconnectPoints(tileIndex, neighborIndex);
                    }
                }
            }

            // Reconnect valid paths
            foreach (var dir in HexGrid.Directions.Values)
            {
                var neighborCoord = coord + dir;
                if (_tiles.TryGetValue(neighborCoord, out var neighborTile))
                {
                    if (_entityManager.IsTileOccupied(neighborCoord))
                        continue;

                    int neighborIndex = neighborTile.Get<TileComponent>().Index;
                    if (_astar.HasPoint(neighborIndex) && !_astar.ArePointsConnected(tileIndex, neighborIndex))
                    {
                        _astar.ConnectPoints(tileIndex, neighborIndex);
                    }
                }
            }
        }

        public bool HasConnection(Vector3I coord, Vector3I neighborCoord)
        {
            if (!_tiles.TryGetValue(coord, out var tile) || !_tiles.TryGetValue(neighborCoord, out var neighborTile))
                return false;

            int tileIndex = tile.Get<TileComponent>().Index;
            int neighborIndex = neighborTile.Get<TileComponent>().Index;

            return _astar.HasPoint(tileIndex) &&
                   _astar.HasPoint(neighborIndex) &&
                   _astar.ArePointsConnected(tileIndex, neighborIndex);
        }
    }
}
