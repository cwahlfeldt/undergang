using System.Linq;
using Game.Components;
using Godot;

namespace Game
{
    public class RenderSystem : System
    {
        private readonly Node3D _boardContainer = new() { Name = "Board" };
        private readonly Node3D _unitContainer = new() { Name = "Units" };
        private readonly PackedScene _tileScene = ResourceLoader.Load<PackedScene>("res://src/Scenes/HexTile.tscn");

        public override void Initialize()
        {
            Entities.GetRootNode().AddChild(_boardContainer);
            Entities.GetRootNode().AddChild(_unitContainer);

            foreach (Entity entity in Entities.Query<Instance>())
            {
                if (entity.Has<Tile>())
                {
                    var tileScene = _tileScene.Instantiate<Node3D>();
                    var tileInstance = entity.Update(new Instance(tileScene));

                    _boardContainer.AddChild(tileInstance.Node);

                    tileInstance.Node.Position = HexGrid.HexToWorld(entity.Get<Coordinate>());
                    tileInstance.Node.Name = entity.Get<Name>();

                    if (!entity.Has<Traversable>())
                    {
                        tileInstance.Node.GetNode<MeshInstance3D>("Mesh").Visible = false;
                    }
                }
                SetupInputs(entity);

                if (entity.Has<Unit>())
                {
                    var unitScene = ResourceLoader.Load<PackedScene>($"res://src/Scenes/{entity.Get<Unit>()}.tscn");
                    var unitSceneInstance = unitScene.Instantiate<Node3D>();
                    var unitInstance = entity.Update(new Instance(unitSceneInstance));

                    _unitContainer.AddChild(unitInstance.Node);
                    unitInstance.Node.Position = HexGrid.HexToWorld(entity.Get<Coordinate>());
                    unitInstance.Node.Name = entity.Get<Name>();
                }
            }

            Events.OnGridReady(Entities.Query<Tile>());
        }

        public void SetupInputs(Entity entity)
        {
            if (!entity.Has<Tile>())
                return;

            if (entity.Get<Instance>().Node is Area3D tileBody)
            {
                tileBody.InputEvent += (camera, @event, position, normal, shapeIdx) =>
                {
                    if (@event is InputEventMouseButton mouseEvent &&
                        mouseEvent.ButtonIndex == MouseButton.Left &&
                        mouseEvent.Pressed)
                    {
                        // GD.Print("wtf");
                        Events.OnTileSelect(entity);
                        // Events.OnTileClick(tileComponent.Coord);
                    }
                };

                tileBody.MouseEntered += () =>
                {
                    Events.OnTileHover(entity);
                };

                tileBody.MouseExited += () =>
                {
                    Events.OnTileUnhover(entity);
                };
            }
        }

        // public override void Initialize()
        // {
        //     Entities.GetRootNode().AddChild(_boardContainer);
        //     Entities.GetRootNode().AddChild(_unitContainer);

        //     var tiles = Entities.GetTiles();

        //     foreach (var tile in tiles)
        //     {
        //         RenderTile(tile);
        //     }
        // }

        // private void RenderTile(Entity tile)
        // {
        //     var tileScene = ResourceLoader.Load<PackedScene>("res://src/Scenes/HexTile.tscn");
        //     var tileComponent = tile.Get<TileComponent>() with
        //     {
        //         Node = tileScene.Instantiate<Node3D>()
        //     };

        //     _boardContainer.AddChild(tileComponent.Node);

        //     if (tileComponent.Node is Area3D tileBody)
        //     {
        //         tileBody.InputEvent += (camera, @event, position, normal, shapeIdx) =>
        //         {
        //             if (@event is InputEventMouseButton mouseEvent &&
        //                 mouseEvent.ButtonIndex == MouseButton.Left &&
        //                 mouseEvent.Pressed)
        //             {
        //                 // GD.Print("wtf");
        //                 Events.OnTileSelect(tile);
        //                 // Events.OnTileClick(tileComponent.Coord);
        //             }
        //         };

        //         tileBody.MouseEntered += () =>
        //         {
        //             Events.OnTileHover(tile);
        //         };

        //         tileBody.MouseExited += () =>
        //         {
        //             Events.OnTileUnhover(tile);
        //         };
        //     }

        //     if (tileComponent.Type == TileType.Blocked)
        //     {
        //         tileComponent.Node.GetNode<MeshInstance3D>("Mesh").Visible = false;
        //     }

        //     tileComponent.Node.Name = tileComponent.Name;
        //     tileComponent.Node.Position = HexGrid.HexToWorld(tile.Get<TileComponent>().Coord);

        //     tile.Update(tileComponent);

        //     if (tile.Has<UnitComponent>())
        //     {
        //         RenderUnit(tile);
        //     }
        // }

        // private void RenderUnit(Entity tile)
        // {
        //     var unitScene = ResourceLoader.Load<PackedScene>($"res://src/Scenes/{tile.Get<UnitComponent>().Type}.tscn");
        //     var unitComponent = tile.Get<UnitComponent>() with
        //     {
        //         Node = unitScene.Instantiate<Node3D>()
        //     };

        //     _unitContainer.AddChild(unitComponent.Node);

        //     unitComponent.Node.Name = unitComponent.Name;
        //     unitComponent.Node.Position = HexGrid.HexToWorld(tile.Get<TileComponent>().Coord);

        //     tile.Update(unitComponent);
        // }
    }
}
