using Godot;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Game.Components;
using System.Text.RegularExpressions;

namespace Game
{
    public class RangeSystem : System
    {
        public override void Initialize()
        {
            Entities.Query<Unit>()
                .ToList()
                .ForEach(e =>
                {
                    if (e.Has<Grunt>() || e.Has<Player>())
                        e.Add(new RangeCircle());
                });
        }

        public override async Task Update()
        {
            UpdateRanges(Entities.Query<Unit>().ToList());
        }

        private void UpdateRanges(List<Entity> units)
        {
            units.ForEach(u =>
            {
                if (u.Has<RangeCircle>())
                {
                    var tilesInRange = GetRangeCircle(u.Get<Coordinate>()).ToList();

                }

            });
        }

        public static IEnumerable<Vector3I> GetRangeCircle(Vector3I center)
        {
            return HexGrid.Directions.Values.Select(dir => center + dir);
        }
    }
}
