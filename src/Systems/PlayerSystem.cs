using System.Linq;
using System.Threading.Tasks;
using Game.Components;

namespace Game
{
    public class PlayerSystem : System
    {
        private bool _playerActionInProgress = false;

        public override void Initialize()
        {
            Events.TileSelect += OnTileSelect;
        }

        public override async Task Update(Entity entity)
        {
            if (_playerActionInProgress)
                return;

            var player = Entities.Query<Player>().FirstOrDefault();

            if (!player.Has<Active>())
                return;

            if (entity.Get<Coordinate>() == player.Get<Coordinate>() &&
                !player.Has<Active>() &&
                !entity.Has<Tile>())
                return;

            _playerActionInProgress = true;

            player.Add(new Movement(
                player.Get<Coordinate>(),
                entity.Get<Coordinate>()
            ));

            // await Systems.Update(player);


            Events.EndTurn(player);
        }

        private async void OnTileSelect(Entity entity)
        {
            await Systems.Update(entity);
            _playerActionInProgress = false;
        }
    }
}