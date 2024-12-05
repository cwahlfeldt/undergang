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

        public IEnumerable<Entity> GetHexGrid() =>
            _entities.Values
                .Where(e => e.Has<HexTileComponent>())
                .ToList();

        public Entity GetEntity(int id) => _entities[id];

        public Entity GetEntityByHexCoord(Vector3I hexCoord) =>
            _entities.Values
                .Where(e => e.Has<HexCoordComponent>() &&
                            e.Has<RenderComponent>() &&
                            e.Get<HexCoordComponent>().Coord == hexCoord)
                            .FirstOrDefault();

        public int GetHexTileIndex(Vector3I hexCoord) =>
            _entities.Values
                .Where(e => e.Has<HexTileComponent>() &&
                            e.Get<HexCoordComponent>().Coord == hexCoord)
                            .FirstOrDefault().Get<HexTileComponent>().Index;

        public IEnumerable<Entity> GetEnemies() =>
            _entities.Values
                .Where(e => e.Has<UnitTypeComponent>() &&
                            e.Get<UnitTypeComponent>().UnitType == UnitType.Enemy)
                .ToList();

        public Entity GetPlayer() =>
            _entities.Values
                .FirstOrDefault(e => e.Has<UnitTypeComponent>() &&
                                     e.Get<UnitTypeComponent>().UnitType == UnitType.Player);
    }
}