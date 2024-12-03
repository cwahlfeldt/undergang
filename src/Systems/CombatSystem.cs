using System.Collections.Generic;
using System.Linq;
using Godot;
using Game;

namespace Game.Systems
{
    public class CombatSystem(EntityManager entities, HexGridSystem grid)
    {
        private readonly EntityManager _entities = entities;
        private readonly HexGridSystem _grid = grid;

        public bool WouldDieFromMove(Entity unit, Vector3I from, Vector3I to)
        {
            if (!unit.Has<UnitTypeComponent>() || unit.Get<UnitTypeComponent>().UnitType != UnitType.Player)
                return false;

            var enemies = _entities.GetEnemies();
            foreach (var enemy in enemies)
            {
                var enemyPos = enemy.Get<HexCoordComponent>().HexCoord;
                var attackRange = enemy.Get<AttackRangeComponent>().AttackRange;

                // If player moves from outside range to inside range, they die
                bool wasInRange = _grid.GetDistance(from, enemyPos) <= attackRange;
                bool willBeInRange = _grid.GetDistance(to, enemyPos) <= attackRange;

                if (!wasInRange && willBeInRange)
                    return true;
            }
            return false;
        }

        public List<Entity> GetKillableEnemies(Entity unit, Vector3I from, Vector3I to)
        {
            if (!unit.Has<UnitTypeComponent>() || unit.Get<UnitTypeComponent>().UnitType != UnitType.Player)
                return new List<Entity>();

            var attackRange = unit.Get<AttackRangeComponent>().AttackRange;
            return _entities.GetEnemies()
                .Where(enemy =>
                {
                    var enemyPos = enemy.Get<HexCoordComponent>().HexCoord;
                    return _grid.GetDistance(to, enemyPos) <= attackRange;
                })
                .ToList();
        }
    }
}