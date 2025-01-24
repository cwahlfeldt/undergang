using Godot;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Game.Components;

namespace Game
{
    public class RangeSystem : System
    {
        public override void Initialize()
        {
            Entities.Query<Player, Grunt>()
                .ForEach(e => e.Add(new RangeCircle()));
        }
    }
}
