using System;
using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Godot;

namespace Game.Systems
{
    public class TurnSystem
    {
        private Queue<Entity> _turnQueue = new();
        private readonly EntityManager _entityManager;
        public Entity CurrentUnit => _turnQueue.Count > 0 ? _turnQueue.Peek() : null;

        public TurnSystem(EntityManager entityManager)
        {
            _entityManager = entityManager;
            EventBus.Instance.UnitDefeated += OnUnitDefeated;
        }

        public void StartCombat()
        {
            RefreshTurnQueue();
        }

        private void RefreshTurnQueue()
        {
            _turnQueue.Clear();
            var player = _entityManager.GetPlayer();
            if (player != null) _turnQueue.Enqueue(player);

            foreach (var enemy in _entityManager.GetEnemies())
                _turnQueue.Enqueue(enemy);

            if (CurrentUnit != null)
                EventBus.Instance.OnTurnChanged(CurrentUnit);
        }

        public void EndTurn()
        {
            if (_turnQueue.Count > 0)
            {
                _turnQueue.Dequeue();

                if (_turnQueue.Count == 0)
                    RefreshTurnQueue();

                if (CurrentUnit != null)
                    EventBus.Instance.OnTurnChanged(CurrentUnit);
            }
        }

        public void RemoveUnit(Entity unit) =>
            _turnQueue = new Queue<Entity>(_turnQueue.Where(u => u.Id != unit.Id));

        public bool IsUnitTurn(Entity unit) =>
            CurrentUnit?.Id == unit.Id;

        public bool OnlyPlayerRemains() =>
            _turnQueue.Count == 1 && _turnQueue.Peek().Get<UnitTypeComponent>().UnitType == UnitType.Player;

        private void OnUnitDefeated(Entity unit) => RemoveUnit(unit);
    }
}