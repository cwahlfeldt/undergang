using Godot;

namespace Game.Systems
{
    public class UnitSystem
    {
        private readonly EntityManager _entityManager;
        private Node3D _unitContainer;

        public UnitSystem(EntityManager entityManager)
        {
            _entityManager = entityManager;
            _unitContainer = new Node3D
            {
                Name = "Units"
            };
            _entityManager.GetRootNode().AddChild(_unitContainer);
        }
        public Entity CreatePlayer(Vector3I hexCoord)
        {
            var scene = ResourceLoader.Load<PackedScene>("res://src/Scenes/Player.tscn");
            var entity = CreateUnit(UnitType.Player, hexCoord, scene);
            entity.Update(new HealthComponent(3));

            _entityManager.AddEntity(entity);
            return entity;
        }

        public Entity CreateGrunt(Vector3I hexCoord)
        {
            var scene = ResourceLoader.Load<PackedScene>("res://src/Scenes/Enemy.tscn");
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
            _unitContainer.AddChild(unit);
            unit.Position = HexGridSystem.HexToWorld(hexCoord);

            entity.Add(new RenderComponent(unit));
            entity.Get<RenderComponent>().Node3D.Name = name;

            return entity;
        }
    }
}
