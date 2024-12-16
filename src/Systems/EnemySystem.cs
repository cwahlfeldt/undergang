using System.Linq;
using System.Threading.Tasks;
using Game.Components;

namespace Game
{
    public class EnemySystem : System
    {
        public override async Task Update()
        {
            var enemy = Entities.Query<Enemy, CurrentTurn>().FirstOrDefault();
            var player = Entities.Query<Player>().FirstOrDefault();

            if (enemy == null || player == null)
                return;

            // Only move if enemy is waiting for action
            if (!enemy.Has<WaitingForAction>() || enemy.Has<Movement>())
                return;

            enemy.Add(new Movement(
                enemy.Get<Coordinate>(),
                player.Get<Coordinate>()
            ));

            enemy.Remove<WaitingForAction>();
        }
    }
}