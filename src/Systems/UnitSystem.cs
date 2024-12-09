using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Autoload;
using Godot;

namespace Game.Systems
{
    public class UnitSystem
    {
        private readonly EntityManager _entityManager;
        private readonly GridSystem _spatialSystem;
        private readonly Node3D _unitContainer;

        public UnitSystem(EntityManager entityManager, GridSystem spatialSystem)
        {
            _entityManager = entityManager;
            _spatialSystem = spatialSystem;

            _unitContainer = new Node3D
            {
                Name = "Units"
            };
            _entityManager.GetRootNode().AddChild(_unitContainer);
        }

        public async Task MoveUnit(Entity entity, List<Vector3I> path)
        {
            if (!entity.Has<RenderComponent>() && !entity.Has<HexCoordComponent>())
            {
                GD.PrintErr("Entity does not have a RenderComponent or HexCoordComponent");
                return;
            }

            var oldCoord = entity.Get<HexCoordComponent>().Coord;
            var oldTile = _entityManager.GetAt(oldCoord);
            var oldOccupants = oldTile.Get<OccupantsComponent>().Occupants;

            // Create new occupants list without the moving entity
            var updatedOldOccupants = oldOccupants.Where(e => e.Id != entity.Id).ToList();
            oldTile.Update(new OccupantsComponent(updatedOldOccupants));

            var entityMoveRange = entity.Get<MoveRangeComponent>().MoveRange;
            var entityNode = entity.Get<RenderComponent>().Node3D;
            var locations = path.Select(HexGrid.HexToWorld).ToList();

            await AnimationManager.Instance.MoveThrough(entityNode, locations);

            var newCoord = path.Last();
            var newTile = _entityManager.GetAt(newCoord);
            var newOccupants = newTile.Get<OccupantsComponent>().Occupants;

            // Create new occupants list with the moving entity added
            var updatedNewOccupants = new List<Entity>(newOccupants) { entity };

            newTile.Update(new OccupantsComponent(updatedNewOccupants));
            entity.Update(new HexCoordComponent(newCoord));
        }

        public async Task MoveUnitTo(Entity entity, List<Vector3I> path)
        {
            var unitComponent = entity.Get<UnitComponent>();
            var tileComponent = entity.Get<TileComponent>();
            var locations = path.Select(HexGrid.HexToWorld).ToList();

            await AnimationManager.Instance.MoveThrough(unitComponent.Node, locations);
            // var from = entity;
            // var to = _entityManager.GetAtCoord(coord);
        }

        public bool IsInAttackRange(Entity attacker, Vector3I position)
        {
            if (!attacker.Has<AttackRangeComponent>() || !attacker.Has<HexCoordComponent>())
                return false;

            var attackerPos = attacker.Get<HexCoordComponent>().Coord;
            var attackRange = attacker.Get<AttackRangeComponent>().AttackRange;

            return HexGrid.GetDistance(attackerPos, position) <= attackRange;
        }

        public void AttackUnit(Entity attacker, Entity target)
        {
            var damage = attacker.Get<DamageComponent>().Damage;
            var targetHealth = target.Get<HealthComponent>().Health;
            var newHealth = targetHealth - damage;

            if (newHealth <= 0)
            {
                EventBus.Instance.OnUnitDefeated(target);
                return;
            }

            target.Update(new HealthComponent(newHealth));
        }

        public void SpwanPlayer(Vector3I coord)
        {
            var tile = _entityManager.GetAt(coord);
            tile.Add(new UnitComponent(
                new Node3D(),
                $"Player_{Guid.NewGuid()}",
                UnitType.Player,
                3,
                1
            ));
        }

        public void SpawnGrunt(Vector3I coord)
        {
            var tile = _entityManager.GetAt(coord);
            tile.Add(new UnitComponent(
                new Node3D(),
                $"Enemy_{Guid.NewGuid()}",
                UnitType.Grunt,
                1,
                1
            ));
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

            // add occupant to tile
            var tile = _entityManager.GetAt(hexCoord);
            tile.Update(new OccupantsComponent([entity]));
            var unitComponent = new UnitComponent(unit, name, unitType);
            tile.Add(unitComponent);

            // _spatialSystem.RegisterUnit(hexCoord, entity);

            return entity;
        }
    }
}
