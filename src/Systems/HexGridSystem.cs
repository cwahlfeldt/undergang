using System.Collections.Generic;
using System.Linq;
using Godot;
using Game.Autoload;
using System;

namespace Game.Systems
{
    public class HexGridSystem
    {
        private const float HEX_SIZE = 1.05f;
        private readonly EntityManager _entityManager;
        private Node3D _hexGridContainer;
        private readonly PackedScene _hexTileScene = ResourceLoader.Load<PackedScene>("res://src/Scenes/HexTile.tscn");

        public HexGridSystem(EntityManager entityManager, int radius)
        {
            _entityManager = entityManager;
            CreateHexGrid(radius);
        }

        public void CreateHexGrid(int radius, int blockedTilesAmt = 16)
        {
            _hexGridContainer = new Node3D
            {
                Name = "HexGrid"
            };
            _entityManager.GetRootNode().AddChild(_hexGridContainer);

            var gridEntity = new Entity(_entityManager.GetNextId());
            gridEntity.Add(new HexGridComponent(radius));
            gridEntity.Add(new NameComponent(_hexGridContainer.Name));
            gridEntity.Add(new RenderComponent(_hexGridContainer));

            // Generate the coordinates
            var coordinates = HexGrid.GenerateHexCoordinates(radius);
            var randBlockedTileIndices = Utils.GenerateRandomIntArray(blockedTilesAmt);

            // Create tile entities
            var index = 0;
            foreach (var coord in coordinates)
            {
                var tileType = randBlockedTileIndices.Contains(index) && coord != Config.PlayerStart ? TileType.Blocked : TileType.Floor;
                CreateTileEntity(coord, index, tileType);
                index++;
            }

            _entityManager.AddEntity(gridEntity);
        }

        private void CreateTileEntity(Vector3I hexCoord, int index, TileType tileType)
        {
            var tileEntity = new Entity(_entityManager.GetNextId());
            var tileNode = _hexTileScene.Instantiate<Node3D>();
            _hexGridContainer.AddChild(tileNode);

            if (tileNode is Area3D tileBody)
            {
                tileBody.InputEvent += (camera, @event, position, normal, shapeIdx) =>
                {
                    if (@event is InputEventMouseButton mouseEvent &&
                        mouseEvent.ButtonIndex == MouseButton.Left &&
                        mouseEvent.Pressed)
                    {
                        EventBus.Instance.OnTileSelect(tileEntity);
                    }
                };

                tileBody.MouseEntered += () =>
                {
                    EventBus.Instance.OnTileHover(tileEntity);
                };

                tileBody.MouseExited += () =>
                {
                    EventBus.Instance.OnTileUnhover(tileEntity);
                };
            }

            if (tileType == TileType.Blocked)
            {
                tileNode.GetNode<MeshInstance3D>("Mesh").Visible = false;
            }

            tileNode.Name = $"Tile {hexCoord} {index}";
            tileNode.Position = HexGrid.HexToWorld(hexCoord);
            tileEntity.Add(new RenderComponent(tileNode));
            tileEntity.Add(new HexCoordComponent(hexCoord));
            tileEntity.Add(new HexTileComponent(tileType, index));
            tileEntity.Add(new NameComponent(tileNode.Name));

            _entityManager.AddEntity(tileEntity);
        }

        public Entity GetTileAtCoordinate(Vector3I hexCoord)
        {
            return _entityManager.GetEntities().Values
                .FirstOrDefault(e =>
                    e.Has<HexTileComponent>() &&
                    e.Has<HexCoordComponent>() &&
                    e.Get<HexCoordComponent>().HexCoord == hexCoord);
        }
    }
}