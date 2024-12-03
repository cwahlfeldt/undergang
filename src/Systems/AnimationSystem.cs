using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Game.Autoload;

namespace Game.Systems
{
    public class AnimationSystem
    {
        public async Task MoveEntity(Entity entity, List<Vector3I> path)
        {
            if (!entity.Has<RenderComponent>() && !entity.Has<HexCoordComponent>())
            {
                GD.PrintErr("Entity does not have a RenderComponent or HexCoordComponent");
                return;
            }

            // GD.Print("wtf");
            var entityMoveRange = entity.Get<MoveRangeComponent>().MoveRange;
            // var rangedPath = path.Take(entityMoveRange + 1);
            var entityNode = entity.Get<RenderComponent>().Node3D;
            var locations = path.Select(HexGridSystem.HexToWorld).ToList();

            await AnimationManager.Instance.MoveThrough(entityNode, locations);

            entity.Update(new HexCoordComponent(path.Last()));
        }
    }
}