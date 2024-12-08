using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Autoload;
using Godot;

namespace Game.Systems
{
    public class UnitFactory
    {
        private readonly EntityManager _entityManager;
        private readonly GridManager _gridManager;
        private readonly Node3D _unitContainer;

        public UnitFactory(EntityManager entityManager, GridManager gridManager)
        {
            _entityManager = entityManager;
            _gridManager = gridManager;

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

            _gridManager.(entity);
            return entity;
        }

        public Entity CreateGrunt(Vector3I hexCoord)
        {
            var scene = ResourceLoader.Load<PackedScene>("res://src/Scenes/Enemy.tscn");
            var entity = CreateUnit(UnitType.Grunt, hexCoord, scene);
            entity.Add(new UnitTypeComponent(UnitType.Enemy));

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
            entity.Add(new DamageComponent(1));

            // add node to scene
            var unit = unitScene.Instantiate<Node3D>();
            _unitContainer.AddChild(unit);
            entity.Add(new RenderComponent(unit));
            entity.Get<RenderComponent>().Node3D.Name = name;
            unit.Position = HexGrid.HexToWorld(hexCoord);

            _gridManager.RegisterUnit(hexCoord, entity);

            return entity;
        }
    }
}
