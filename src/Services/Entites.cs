using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Game.Components;
using Godot;

namespace Game
{
    public partial class Entities(Node3D rootNode) : Node3D, ISystem
    {
        private readonly Dictionary<int, Entity> _entities = [];
        private int _nextId = 0;
        private readonly Node3D _rootNode = rootNode;

        public int GetNextId()
        {
            return _nextId++;
        }

        public Entity AddEntity(Entity entity)
        {
            return _entities[entity.Id] = entity;
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
            Query<Unit, Enemy>();

        public Entity GetRandomTileEntity()
        {
            var rand = new Random();
            var entitiesAwayFromPlayer = Query<Coordinate>()
                .Where(e =>
                    !e.Has<Unit>() &&
                    !e.Has<Untraversable>() &&
                    !HexGrid.GetHexesInRange(Config.PlayerStart, 3).Contains(e.Get<Coordinate>()));

            return entitiesAwayFromPlayer
            .ElementAtOrDefault(rand.Next(0, entitiesAwayFromPlayer.Count()));
        }

        public IEnumerable<Entity> GetTiles() =>
            Query<Tile>();

        public Entity GetAt(Vector3I coord) =>
            Query<Tile>().FirstOrDefault(e => e.Get<Coordinate>() == coord);

        public bool IsTileOccupied(Vector3I coord)
        {
            var tile = GetAt(coord);
            return tile != null && tile.Has<Unit>();
        }

        public IEnumerable<Entity> GetTilesInRange(Vector3I coord, int range) =>
            GetTiles()
                .Where(e =>
                    HexGrid.GetHexesInRange(coord, range).Contains(e.Get<TileComponent>().Coord) &&
                    e.Get<TileComponent>().Coord != coord).ToList();

        public IEnumerable<Entity> Query<T1>() =>
            _entities.Values.Where(e =>
                e.Has<T1>()).ToList();
        public IEnumerable<Entity> Query<T1, T2>() =>
            _entities.Values.Where(e =>
                e.Has<T1>() &&
                e.Has<T2>()).ToList();
        public IEnumerable<Entity> Query<T1, T2, T3>() =>
            _entities.Values.Where(e =>
                e.Has<T1>() &&
                e.Has<T2>() &&
                e.Has<T3>())
                .ToList();
        public IEnumerable<Entity> Query<T1, T2, T3, T4>() =>
            _entities.Values.Where(e =>
                e.Has<T1>() &&
                e.Has<T2>() &&
                e.Has<T3>() &&
                e.Has<T4>())
                .ToList();

        // public IEnumerable<Entity> CreateGrid(int radius, int blockedTilesAmt = 16)
        // {
        //     // Generate the coordinates
        //     var coordinates = HexGrid.GenerateHexCoordinates(radius);
        //     var randBlockedTileIndices = Utils.GenerateRandomIntArray(blockedTilesAmt);

        //     // Create tile entities
        //     var entities = new List<Entity>();
        //     var index = 0;
        //     foreach (var coord in coordinates)
        //     {
        //         var tileType = randBlockedTileIndices.Contains(index) && coord != Config.PlayerStart ? TileType.Blocked : TileType.Floor;
        //         var entity = CreateTileEntity(coord, index, tileType);
        //         entities.Add(entity);
        //         index++;
        //     }

        //     return entities;
        // }

        // private Entity CreateTileEntity(Vector3I hexCoord, int index, TileType tileType)
        // {
        //     var tileEntity = new Entity(GetNextId());

        //     tileEntity.Add(new TileComponent(
        //         new Node3D(),
        //         $"Tile {hexCoord} {index}",
        //         hexCoord,
        //         tileType,
        //         index
        //     ));

        //     AddEntity(tileEntity);
        //     return tileEntity;
        // }

        // public void SpawnPlayer()
        // {
        //     var tile = GetAt(Config.PlayerStart);
        //     tile.Add(new UnitComponent(
        //         Node: new Node3D(),
        //         Name: $"Player {Guid.NewGuid()}",
        //         Type: UnitType.Player,
        //         Health: 3,
        //         MoveRange: 1
        //     ));
        // }

        public IEnumerable<Entity> CreateGrid(int mapSize = 5, int blockedTilesAmt = 16)
        {
            var randBlockedTileIndices = Utils.GenerateRandomIntArray(blockedTilesAmt);
            return HexGrid.GenerateHexCoordinates(mapSize)
                .Select((coord, i) =>
                {
                    var tile = CreateTile(coord, i);

                    if (randBlockedTileIndices.Contains(i) && coord != Config.PlayerStart)
                        tile.Add(new Untraversable());
                    else
                        tile.Add(new Traversable());

                    return tile;
                }).ToList();
        }

        public Entity CreateTile(Vector3I coord, int index = 0)
        {
            var tile = AddEntity(new Entity(GetNextId()));

            tile.Add(new Name($"Tile {coord}"));
            tile.Add(new Tile());
            tile.Add(new Instance(new Node3D()));
            tile.Add(new Coordinate(coord));
            tile.Add(new TileIndex(index));

            return tile;
        }

        public Entity CreatePlayer()
        {
            var player = AddEntity(new Entity(GetNextId()));

            player.Add(new Name("Player"));
            player.Add(new Player());
            player.Add(new Unit(UnitType.Player));
            player.Add(new Instance(new Node3D()));
            player.Add(new Coordinate(Config.PlayerStart));
            player.Add(new Damage(1));
            player.Add(new Health(3));
            player.Add(new MoveRange(1));
            player.Add(new AttackRange(1));

            return player;
        }

        public Entity CreateEnemy(UnitType unitType)
        {
            var enemy = AddEntity(new Entity(GetNextId()));

            enemy.Add(new Name("Enemy"));
            enemy.Add(new Enemy());
            enemy.Add(new Unit(unitType));
            enemy.Add(new Instance(new Node3D()));
            enemy.Add(new Coordinate(GetRandomTileEntity().Get<Coordinate>()));
            enemy.Add(new Damage(1));
            enemy.Add(new Health(1));
            enemy.Add(new MoveRange(1));
            enemy.Add(new AttackRange(1));

            return enemy;
        }
    }
}
