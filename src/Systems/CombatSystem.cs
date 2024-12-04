using System.Collections.Generic;
using System.Linq;
using Godot;
using Game;

namespace Game.Systems
{
    public class CombatSystem(EntityManager entities)
    {
        private readonly EntityManager _entities = entities;

        public bool IsInAttackRange(Entity attacker, Vector3I position)
        {
            if (!attacker.Has<AttackRangeComponent>() || !attacker.Has<HexCoordComponent>())
                return false;

            var attackerPos = attacker.Get<HexCoordComponent>().HexCoord;
            var attackRange = attacker.Get<AttackRangeComponent>().AttackRange;

            return HexGrid.GetDistance(attackerPos, position) <= attackRange;
        }
    }
}