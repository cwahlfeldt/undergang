using Godot;

public partial class Enemy : Unit
{
    // [Export] public float MoveSpeed = 4.0f;
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

    public void TakeTurn(Vector3 playerPosition)
    {
        // Simple AI: Move towards player
        var directionToPlayer = (playerPosition - Position).Normalized();
        _targetPosition = Position + directionToPlayer * 2.0f; // Move 2 units towards player
        _isMoving = true;
    }
}
