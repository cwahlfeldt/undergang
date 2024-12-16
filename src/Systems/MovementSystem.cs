using System.Linq;
using System.Threading.Tasks;
using Game.Components;
using Godot;

namespace Game
{
    public class MovementSystem : System
    {
        public override async Task Update(Entity entity)
        {
            var moveEntites = Entities.Query<Movement>().ToList();
            GD.Print($"Found {moveEntites.Count} entities to move");

            if (!moveEntites.Any())
                return;

            foreach (var mover in moveEntites)
            {
                var (from, to) = mover.Get<Movement>();
                var path = PathFinder.FindPath(from, to, mover.Get<MoveRange>());
                var locations = path.Select(HexGrid.HexToWorld).ToList();

                GD.Print($"From: {from}, To: {to}");
                GD.Print($"Path length: {path.Count()}");
                GD.Print($"World locations: {string.Join(", ", locations)}");

                // This will now properly await on the main thread
                await Tweener.MoveThrough(mover.Get<Instance>().Node, locations);
                mover.Remove<Movement>();
                // GD.Print(mover.Has<Movement>());

            }
        }
    }
}
