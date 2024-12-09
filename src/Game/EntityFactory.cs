using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Game;
using Game.Autoload;
using Godot;

namespace Game
{
    public class EntityFactory(EntityManager entityManager)
    {
        private readonly EntityManager _entityManager = entityManager;
        private readonly PackedScene _tileScene = ResourceLoader.Load<PackedScene>("res://src/Scenes/HexTile.tscn");
        public IEnumerable<Entity> CreateGrid(int radius, int blockedTilesAmt = 16)
        {
            // Generate the coordinates
            var coordinates = HexGrid.GenerateHexCoordinates(radius);
            var randBlockedTileIndices = Utils.GenerateRandomIntArray(blockedTilesAmt);

            // Create tile entities
            var entities = new List<Entity>();
            var index = 0;
            foreach (var coord in coordinates)
            {
                var tileType = randBlockedTileIndices.Contains(index) && coord != Config.PlayerStart ? TileType.Blocked : TileType.Floor;
                var entity = CreateTileEntity(coord, index, tileType);
                entities.Add(entity);
                index++;
            }

            return entities;
        }

        private Entity CreateTileEntity(Vector3I hexCoord, int index, TileType tileType)
        {
            var tileEntity = new Entity(_entityManager.GetNextId());

            tileEntity.Add(new TileComponent(
                new Node3D(),
                $"Tile {hexCoord} {index}",
                hexCoord,
                tileType,
                index
            ));

            _entityManager.AddEntity(tileEntity);
            return tileEntity;
        }

        public void SpawnPlayer(Vector3I coord)
        {
            var tile = _entityManager.GetAtCoord(coord);
            tile.Add(new UnitComponent(
                new Node3D(),
                $"Player {Guid.NewGuid()}",
                UnitType.Player,
                3,
                1
            ));
        }

        public void SpawnGrunt(Vector3I coord)
        {
            var tile = _entityManager.GetAtCoord(coord);
            tile.Add(new UnitComponent(
                new Node3D(),
                $"Enemy {Guid.NewGuid()}",
                UnitType.Grunt,
                1,
                1
            ));
        }

    }
}