using Godot;
using Undergang.Game;

namespace Undergang.Entities.Systems
{
    public class UnitSystem(EntityManager entityManager)
    {
        private readonly EntityManager _entityManager = entityManager;
        
        public Entity CreatePlayer(Vector3I hexCoord)
        {
            var scene = ResourceLoader.Load<PackedScene>("res://src/Components/Units/Player/Player.tscn");
            var entity = CreateUnit(UnitType.Player, hexCoord, scene);
            entity.Update(new HealthComponent(3));

            _entityManager.AddEntity(entity);
            return entity;
        }

        public Entity CreateGrunt(Vector3I hexCoord)
        {
            var scene = ResourceLoader.Load<PackedScene>("res://src/Components/Units/Enemies/Enemy.tscn");
            var entity = CreateUnit(UnitType.Grunt, hexCoord, scene);

            _entityManager.AddEntity(entity);
            return entity;
        }

        private Entity CreateUnit(UnitType unitType, Vector3I hexCoord, PackedScene unitScene)
        {
            var entity = new Entity(_entityManager.GetNextId());
            var name = $"{unitType}_{entity.Id}";

            entity.Add(new NameComponent(name));
            entity.Add(new HexCoordComponent(hexCoord));
            entity.Add(new AttackRangeComponent(1));
            entity.Add(new UnitTypeComponent(unitType));
            entity.Add(new MoveRangeComponent(1));
            entity.Add(new HealthComponent(1));

            var unit = unitScene.Instantiate<Node3D>();
            _entityManager.GetRootNode().AddChild(unit);
            unit.Position = HexGrid.HexToWorld(hexCoord);

            entity.Add(new RenderComponent(unit));
            entity.Get<RenderComponent>().Node3D.Name = name;

            return entity;
        }
    }
}
