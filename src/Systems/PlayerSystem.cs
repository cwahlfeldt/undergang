using System;
using System.Linq;
using System.Threading.Tasks;
using Game.Components;
using Godot;

namespace Game
{
    public class PlayerSystem : System
    {
        public override void Initialize()
        {
            Events.TileSelect += OnTileSelect;
            Events.OnUnitActionComplete += OnUnitActionComplete;
        }

        public override async Task Update()
        {
            var selectedTile = Entities.Query<SelectedTile>().FirstOrDefault();
            var player = Entities.Query<Player>().FirstOrDefault();

            if (selectedTile == null ||
                selectedTile.Get<Coordinate>() == player.Get<Coordinate>() ||
                player.Has<Movement>() ||
                !player.Has<WaitingForAction>())
                return;

            player.Add(new Movement(
                player.Get<Coordinate>(),
                selectedTile.Get<Coordinate>()
            ));

            player.Remove<WaitingForAction>();
        }

        private async void OnTileSelect(Entity entity)
        {
            ClearSelectedTiles();
            var player = Entities.Query<Player>().FirstOrDefault();

            if (!player.Has<WaitingForAction>() || player.Has<Movement>())
                return;

            if (entity.Has<Tile>() && !entity.Has<SelectedTile>() && entity.Has<Traversable>())
            {
                ClearSelectedTiles();
                entity.Add(new SelectedTile());
                await Systems.Update();
            }
        }

        private void OnUnitActionComplete(Entity _)
        {
            ClearSelectedTiles();
        }

        private void ClearSelectedTiles()
        {
            foreach (var tile in Entities.Query<SelectedTile>())
                tile.Remove<SelectedTile>();
        }

        public override void Cleanup()
        {
            Events.TileSelect -= OnTileSelect;
            Events.OnUnitActionComplete -= OnUnitActionComplete;
        }
    }
}