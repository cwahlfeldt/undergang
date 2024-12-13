using Godot;

namespace Game
{
    public record NodeComponent(
        Node3D Node,
        PackedScene Scene,
        Resource Resource
    );
}
