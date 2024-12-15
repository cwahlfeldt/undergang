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
            RefreshTurnQueue();
        }

        private async void RefreshTurnQueue()
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
                await Events.OnTurnChanged(CurrentUnit);
            }
        }

        public async void EndTurn()
        {
            if (_turnQueue.Count > 0)
            {
                var lastEntity = _turnQueue.Dequeue();
                lastEntity.Remove(lastEntity.Get<Active>());

                if (_turnQueue.Count == 0)
                    RefreshTurnQueue();

                if (CurrentUnit != null)
                    await Events.OnTurnChanged(CurrentUnit);
            }
        }

        public void RemoveUnit(Entity unit) =>
            _turnQueue = new Queue<Entity>(_turnQueue.Where(u => u.Id != unit.Id));

        public bool IsUnitTurn(Entity unit) =>
            CurrentUnit?.Id == unit.Id;

        private void OnUnitDefeated(Entity unit) => RemoveUnit(unit);
    }
}
