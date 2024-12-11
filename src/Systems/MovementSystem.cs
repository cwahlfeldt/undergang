using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Autoload;
using Godot;

namespace Game.Systems
{
    public class MovementSystem(EntityManager _entityManager)
    {
        public async Task<Entity> MoveUnitTo(Entity entity, List<Vector3I> path)
        {
            var unitComponent = entity.Get<UnitComponent>();
            var tileComponent = entity.Get<TileComponent>();
            var locations = path.Select(HexGrid.HexToWorld).ToList();

            await AnimationManager.Instance.MoveThrough(unitComponent.Node, locations);

            var fromCoord = path.First();
            var toCoord = path.Last();
            var toEntity = _entityManager.GetAt(toCoord);

            entity.Remove(unitComponent);
            toEntity.Add(unitComponent);

            EventBus.Instance.OnMoveCompleted(toEntity, fromCoord, toCoord);

            return toEntity;
        }
    }
}
