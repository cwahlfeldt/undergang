
// Player.cs
using Godot;

public partial class Player : Unit
{
    // [Export] public float MoveSpeed = 5.0f;
    private Vector3 _targetPosition;
    private bool _isMoving;

    public override void _Ready()
    {
        _targetPosition = Position;
    }

    public override void _Process(double delta)
    {
        if (_isMoving)
        {
            Position = Position.MoveToward(_targetPosition, MoveSpeed * (float)delta);
            _isMoving = Position != _targetPosition;
        }
    }

    public void MoveTo(Vector3 position)
    {
        _targetPosition = position;
        _isMoving = true;
    }
}
