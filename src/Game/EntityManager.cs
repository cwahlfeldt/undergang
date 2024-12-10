using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
// using Godot.DependencyInjection.Attributes;
using Godot.DependencyInjection.Services.Input;

namespace Game
{
    public partial class EntityManager(Node3D rootNode) : Node3D
    {
        private readonly Dictionary<int, Entity> _entities = [];
        private readonly Dictionary<Vector3I, Entity> _tiles = [];
        private readonly Dictionary<Vector3I, Entity> _units = [];
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

        public void RemoveEntity(Entity entity) =>
            _entities.Remove(entity.Id);

        public Entity GetEntity(int id) => _entities[id];

        public (UnitComponent unit, Vector3I coord, Entity entity) GetPlayer()
        {
            var entity = Query<UnitComponent>().FirstOrDefault(e =>
                e.Get<UnitComponent>().Type == UnitType.Player);

            return entity != null ? (
                unit: entity.Get<UnitComponent>(),
                coord: entity.Get<TileComponent>().Coord,
                entity
            ) : default;
        }

        public IEnumerable<Entity> GetEnemies() =>
            Query<UnitComponent>()
                .Where(e => e.Get<UnitComponent>().Type == UnitType.Grunt);

        public Entity GetRandTileEntity()
        {
            var rand = new Random();
            var entities = Query<TileComponent>()
                .Where(e =>
                    !e.Has<UnitComponent>() &&
                    !HexGrid.GetHexesInRange(Config.PlayerStart, 3).Contains(e.Get<TileComponent>().Coord) &&
                    e.Get<TileComponent>().Type != TileType.Blocked);

            return entities
                .ElementAtOrDefault(rand.Next(0, entities.Count()));
        }

        public IEnumerable<Entity> GetTiles() =>
            Query<TileComponent>();

        public Entity GetAt(Vector3I coord) =>
            GetTiles().FirstOrDefault(e => e.Get<TileComponent>().Coord == coord);

        public IEnumerable<Entity> GetTilesInRange(Vector3I coord, int range) =>
            GetTiles()
                .Where(e =>
                    HexGrid.GetHexesInRange(coord, range).Contains(e.Get<TileComponent>().Coord) &&
                    e.Get<TileComponent>().Coord != coord);

        public IEnumerable<Entity> Query<T1>() =>
            _entities.Values.Where(e =>
                e.Has<T1>());

        public IEnumerable<Entity> Query<T1, T2>() =>
            _entities.Values.Where(e =>
                e.Has<T1>() &&
                e.Has<T2>());
    }
}