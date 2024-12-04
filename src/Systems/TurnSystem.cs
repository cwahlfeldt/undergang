using System;
using System.Collections.Generic;
using System.Linq;
using Game.Autoload;

namespace Game.Systems
{
    public class TurnSystem(EntityManager entityManager)
    {
        private Queue<Entity> _turnQueue = new();
        private readonly EntityManager _entityManager = entityManager;
        public Entity CurrentUnit => _turnQueue.Count > 0 ? _turnQueue.Peek() : null;

        public void StartCombat()
        {
            _turnQueue.Clear();

            // Add player first
            var player = _entityManager.GetPlayer();
            if (player != null)
                _turnQueue.Enqueue(player);

            // Add all enemies
            foreach (var enemy in _entityManager.GetEnemies())
            {
                _turnQueue.Enqueue(enemy);
            }

            // Notify first turn
            if (CurrentUnit != null)
                EventBus.Instance.OnTurnChanged(CurrentUnit);
        }

        public void EndTurn()
        {
            if (_turnQueue.Count > 0)
            {
                var unit = _turnQueue.Dequeue();

                // Only add back to queue if unit still exists
                if (_entityManager.GetEntity(unit.Id) != null)
                    _turnQueue.Enqueue(unit);

                // Notify next turn if there are units left
                if (CurrentUnit != null)
                    EventBus.Instance.OnTurnChanged(CurrentUnit);
            }
        }

        public void RemoveUnit(Entity unit)
        {
            // Create new queue without the removed unit
            var newQueue = new Queue<Entity>();
            while (_turnQueue.Count > 0)
            {
                var queuedUnit = _turnQueue.Dequeue();
                if (queuedUnit.Id != unit.Id)
                    newQueue.Enqueue(queuedUnit);
            }
            _turnQueue = newQueue;
        }

        public bool IsUnitTurn(Entity unit)
        {
            return CurrentUnit?.Id == unit.Id;
        }
    }
}