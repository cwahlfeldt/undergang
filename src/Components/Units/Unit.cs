
// Player.cs
using Godot;

public partial class Unit : Node3D
{
    [Export] public float MoveSpeed = 5.0f;
    private Vector3 _targetPosition;
    private bool _isMoving;

    public override void _Ready()
    {
        _targetPosition = Position;
    }

    public void MoveTo(Vector3 position)
    {
        _targetPosition = position;
        _isMoving = true;
    }
}
