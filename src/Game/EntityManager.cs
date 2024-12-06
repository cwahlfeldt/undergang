using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Game
{
    public class EntityManager(Node3D rootNode)
    {
        private readonly Dictionary<int, Entity> _entities = [];
        private int _nextId = 0;
        private readonly Node3D _rootNode = rootNode;

        public int GetNextId()
        {
            return _nextId++;
        }

        public void AddEntity(Entity entity)
        {
            _entities[entity.Id] = entity;
        }

        public Dictionary<int, Entity> GetEntities()
        {
            return _entities;
        }

        public Node3D GetRootNode()
        {
            return _rootNode;
        }

        public void RemoveEntity(Entity entity)
        {
            // First remove from entity manager
            _entities.Remove(entity.Id);

            // Then safely remove visual component if it exists
            if (entity.Has<RenderComponent>())
            {
                var node = entity.Get<RenderComponent>().Node3D;
                if (node != null && Node.IsInstanceValid(node))
                {
                    node.QueueFree();
                }
            }
        }

        public Entity GetEntity(int id) => _entities[id];

        public IEnumerable<Entity> GetEnemies() =>
            Query<UnitTypeComponent>()
                .Where(e => e.Get<UnitTypeComponent>().UnitType == UnitType.Enemy);

        public Entity GetPlayer() =>
            Query<UnitTypeComponent>().FirstOrDefault(e =>
                e.Get<UnitTypeComponent>().UnitType == UnitType.Player);

        public IEnumerable<Entity> GetTiles() =>
            Query<HexTileComponent, HexCoordComponent>();

        public Entity GetEntityByCoord(Vector3I hexCoord) =>
            GetTiles().FirstOrDefault(e => e.Get<HexCoordComponent>().Coord == hexCoord);

        public IEnumerable<Entity> GetTilesInRange(Vector3I coord, int range) =>
            GetTiles()
                .Where(e =>
                    HexGrid.GetHexesInRange(coord, range).Contains(e.Get<HexCoordComponent>().Coord) &&
                    e.Get<HexCoordComponent>().Coord != coord);

        public IEnumerable<Entity> GetTraversableTiles() =>
            GetTiles()
                .Where(e =>
                    e.Get<HexTileComponent>().Type == TileType.Floor &&
                    !HexGrid.GetHexesInRange(Config.PlayerStart, 3).Contains(e.Get<HexCoordComponent>().Coord));

        public IEnumerable<Entity> GetTraversableTilesWithoutOccupants() =>
            GetTraversableTiles()
                .Where(e => !e.Get<OccupantsComponent>().Occupants.Any());

        public IEnumerable<Entity> Query<T1>() =>
            _entities.Values.Where(e =>
                e.Has<T1>());

        public IEnumerable<Entity> Query<T1, T2>() =>
            _entities.Values.Where(e =>
                e.Has<T1>() &&
                e.Has<T2>());

        public IEnumerable<Entity> Query<T1, T2, T3>() =>
            _entities.Values.Where(e =>
                e.Has<T1>() &&
                e.Has<T2>() &&
                e.Has<T3>());

        public IEnumerable<Entity> Query<T1, T2, T3, T4>() =>
            _entities.Values.Where(e =>
                e.Has<T1>() &&
                e.Has<T2>() &&
                e.Has<T3>() &&
                e.Has<T4>());

        public IEnumerable<Entity> Query<T1, T2, T3, T4, T5>() =>
            _entities.Values.Where(e =>
                e.Has<T1>() &&
                e.Has<T2>() &&
                e.Has<T3>() &&
                e.Has<T4>() &&
                e.Has<T5>());
    }
}