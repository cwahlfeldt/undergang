using Godot;

namespace Game.Systems
{
    public class DebugSystem
    {
        private readonly EntityManager _entityManager;

        public DebugSystem(EntityManager entityManager)
        {
            _entityManager = entityManager;
            ShowHexCoordLabels();
        }

        private void ShowHexCoordLabels()
        {
            var debugNode = new Node3D
            {
                Name = "Debug System",
                Position = new Vector3(0.105f, -0.6f, 0.375f)
            };
            _entityManager.GetRootNode().AddChild(debugNode);

            foreach (var tile in _entityManager.GetHexGrid())
            {
                if (tile.Get<HexTileComponent>().Type == TileType.Blocked)
                    continue;

                var hexCoord = tile.Get<HexCoordComponent>().HexCoord;
                var labelPos = HexGrid.HexToWorld(hexCoord);

                var coordLabel = new Label3D
                {
                    Text = hexCoord.ToString(),
                    FontSize = 34,
                    PixelSize = 0.01f,
                    Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
                    Position = new Vector3(labelPos.X, 1.1f, labelPos.Z),
                    Modulate = Colors.Black,
                    Name = hexCoord.ToString()
                };
                debugNode.AddChild(coordLabel);
            }
        }
    }
}
