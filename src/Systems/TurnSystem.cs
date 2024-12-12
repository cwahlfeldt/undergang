using System.Collections.Generic;
using System.Linq;

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

        private void RefreshTurnQueue()
        {
            _turnQueue.Clear();
            var player = Entities.GetPlayer();
            if (player.entity != null) _turnQueue.Enqueue(player.entity);

            foreach (var enemy in Entities.GetEnemies())
                _turnQueue.Enqueue(enemy);

            if (CurrentUnit != null)
                Events.OnTurnChanged(CurrentUnit);
        }

        public void EndTurn()
        {
            if (_turnQueue.Count > 0)
            {
                _turnQueue.Dequeue();

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

        public bool OnlyPlayerRemains() =>
            _turnQueue.Count == 1 && _turnQueue.Peek().Get<UnitComponent>().Type == UnitType.Player;

        private void OnUnitDefeated(Entity unit) => RemoveUnit(unit);
    }
}
