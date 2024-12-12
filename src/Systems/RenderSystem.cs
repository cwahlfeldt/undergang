using Godot;

namespace Game
{
    public class RenderSystem : System
    {
        private readonly Node3D _boardContainer = new() { Name = "Board" };
        private readonly Node3D _unitContainer = new() { Name = "Units" };

        public override void Initialize()
        {
            Entities.GetRootNode().AddChild(_boardContainer);
            Entities.GetRootNode().AddChild(_unitContainer);

            var tiles = Entities.GetTiles();

            foreach (var tile in tiles)
            {
                RenderTile(tile);
            }
        }

        private void RenderTile(Entity tile)
        {
            var tileScene = ResourceLoader.Load<PackedScene>("res://src/Scenes/HexTile.tscn");
            var tileComponent = tile.Get<TileComponent>() with
            {
                Node = tileScene.Instantiate<Node3D>()
            };

            _boardContainer.AddChild(tileComponent.Node);

            if (tileComponent.Node is Area3D tileBody)
            {
                tileBody.InputEvent += (camera, @event, position, normal, shapeIdx) =>
                {
                    if (@event is InputEventMouseButton mouseEvent &&
                        mouseEvent.ButtonIndex == MouseButton.Left &&
                        mouseEvent.Pressed)
                    {
                        // GD.Print("wtf");
                        Events.OnTileSelect(tile);
                        Events.OnTileClick(tileComponent.Coord);
                    }
                };

                tileBody.MouseEntered += () =>
                {
                    Events.OnTileHover(tile);
                };

                tileBody.MouseExited += () =>
                {
                    Events.OnTileUnhover(tile);
                };
            }

            if (tileComponent.Type == TileType.Blocked)
            {
                tileComponent.Node.GetNode<MeshInstance3D>("Mesh").Visible = false;
            }

            tileComponent.Node.Name = tileComponent.Name;
            tileComponent.Node.Position = HexGrid.HexToWorld(tile.Get<TileComponent>().Coord);

            tile.Update(tileComponent);

            if (tile.Has<UnitComponent>())
            {
                RenderUnit(tile);
            }
        }

        private void RenderUnit(Entity tile)
        {
            var unitScene = ResourceLoader.Load<PackedScene>($"res://src/Scenes/{tile.Get<UnitComponent>().Type}.tscn");
            var unitComponent = tile.Get<UnitComponent>() with
            {
                Node = unitScene.Instantiate<Node3D>()
            };

            _unitContainer.AddChild(unitComponent.Node);

            unitComponent.Node.Name = unitComponent.Name;
            unitComponent.Node.Position = HexGrid.HexToWorld(tile.Get<TileComponent>().Coord);

            tile.Update(unitComponent);
        }
    }
}
