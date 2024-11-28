using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class TurnManager
{
    private Queue<Node3D> _turnQueue = new();
    public Node3D CurrentUnit => _turnQueue.Count > 0 ? _turnQueue.Peek() : null;

    public void StartCombat(Player player, List<Enemy> enemies)
    {
        _turnQueue.Clear();

        // Add all units to queue
        _turnQueue.Enqueue(player);
        foreach (var enemy in enemies)
        {
            _turnQueue.Enqueue(enemy);
        }

        // Notify first turn
        SignalBus.Instance.EmitSignal(SignalBus.SignalName.TurnChanged, CurrentUnit);
    }

    public void EndTurn()
    {
        if (_turnQueue.Count > 0)
        {
            var unit = _turnQueue.Dequeue();
            _turnQueue.Enqueue(unit); // Put at end of queue
            SignalBus.Instance.EmitSignal(SignalBus.SignalName.TurnChanged, CurrentUnit);
        }
    }

    public void RemoveUnit(Node3D unit)
    {
        // Create new queue without the removed unit
        _turnQueue = new Queue<Node3D>(_turnQueue.Where(u => u != unit));
    }

    public bool IsUnitTurn(Node3D unit)
    {
        return CurrentUnit == unit;
    }
}