namespace Undergang.Game;

using System;
using System.Collections.Generic;
using System.Linq;
using Undergang.Entities;

public partial class TurnManager
{
    public event Action<Entity> OnTurnChanged;
    private Queue<Entity> _turnQueue = new();
    public Entity CurrentUnit => _turnQueue.Count > 0 ? _turnQueue.Peek() : null;

    public void StartCombat(Entity player, List<Entity> enemies)
    {
        _turnQueue.Clear();

        // Add all units to queue
        _turnQueue.Enqueue(player);
        foreach (var enemy in enemies)
            _turnQueue.Enqueue(enemy);

        // Notify first turn
        OnTurnChanged?.Invoke(CurrentUnit);
    }

    public void EndTurn()
    {
        if (_turnQueue.Count > 0)
        {
            var unit = _turnQueue.Dequeue();
            _turnQueue.Enqueue(unit); // Put at end of queue
            OnTurnChanged?.Invoke(CurrentUnit);
        }
    }

    public void RemoveUnit(Entity unit)
    {
        // Create new queue without the removed unit
        _turnQueue = new Queue<Entity>(_turnQueue.Where(u => u != unit));
    }

    public bool IsUnitTurn(Entity unit)
    {
        return CurrentUnit == unit;
    }
}