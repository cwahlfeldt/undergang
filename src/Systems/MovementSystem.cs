using System.Linq;
using System.Threading.Tasks;
using Game.Components;
using Godot;

namespace Game
{
    public class MovementSystem : System
    {
        public override async Task Update()
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

                await Tweener.MoveThrough(mover.Get<Instance>().Node, locations);

                mover.Update(new Coordinate(path.Last()));
                mover.Remove<Movement>();

                if (mover.Has<CurrentTurn>())
                {
                    Events.UnitActionComplete(mover);
                }

            }
        }
    }
}
