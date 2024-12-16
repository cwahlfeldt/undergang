using System;
using System.Collections.Generic;
using System.Linq;
using Game.Components;

namespace Game
{
    public class TurnSystem : System
    {
        private Queue<Entity> _turnQueue = new();
        public Entity CurrentUnit => _turnQueue.Count > 0 ? _turnQueue.Peek() : null;

        public override void Initialize()
        {
            Events.UnitDefeated += OnUnitDefeated;
            Events.TurnEnd += OnTurnEnd;

            RefreshTurnQueue();
        }

        private void OnTurnEnd(Entity entity)
        {
            EndTurn();
        }

        private void RefreshTurnQueue()
        {
            _turnQueue.Clear();

            var player = Entities.Query<Player>().FirstOrDefault();

            if (player != null)
                _turnQueue.Enqueue(player);

            foreach (var enemy in Entities.Query<Enemy>())
                _turnQueue.Enqueue(enemy);

            if (CurrentUnit != null)
            {
                CurrentUnit.Add(new Active());
                Events.OnTurnChanged(CurrentUnit);
            }
        }

        public void EndTurn()
        {
            if (_turnQueue.Count > 0)
            {
                var lastEntity = _turnQueue.Dequeue();
                lastEntity.Remove<Active>();

                if (_turnQueue.Count == 0)
                    RefreshTurnQueue();

                if (CurrentUnit != null)
                    Events.OnTurnChanged(CurrentUnit);
            }
        }


        public void RemoveUnit(Entity unit) =>
            _turnQueue = new Queue<Entity>(_turnQueue.Where(u => u.Id != unit.Id));

        public bool IsUnitTurn(Entity unit) =>
            CurrentUnit?.Id == unit.Id;

        private void OnUnitDefeated(Entity unit) => RemoveUnit(unit);
    }
}
