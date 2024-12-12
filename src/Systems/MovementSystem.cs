using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace Game
{
    public class MovementSystem : System
    {
        public async Task<Entity> MoveUnitTo(Entity entity, List<Vector3I> path)
        {
            var unitComponent = entity.Get<UnitComponent>();
            var tileComponent = entity.Get<TileComponent>();
            var locations = path.Select(HexGrid.HexToWorld).ToList();

            await Tweener.MoveThrough(unitComponent.Node, locations);

            var fromCoord = path.First();
            var toCoord = path.Last();
            var toEntity = Entities.GetAt(toCoord);

            entity.Remove(unitComponent);
            toEntity.Add(unitComponent);

            Events.OnMoveCompleted(toEntity, fromCoord, toCoord);

            return toEntity;
        }
    }
}
