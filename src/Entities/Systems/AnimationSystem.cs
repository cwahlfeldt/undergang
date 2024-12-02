namespace Undergang.Entities.Systems;

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Godot;
using Undergang.Autoload;
using Undergang.Game;

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
        var rangedPath = path.Take(entityMoveRange + 1);
        var entityNode = entity.Get<RenderComponent>().Node3D;
        var locations = rangedPath.Select(HexGrid.HexToWorld).ToList();

        await AnimationManager.Instance.MoveThrough(entityNode, locations);

        entity.Update(new HexCoordComponent(rangedPath.Last()));
    }
}
