using Godot;

namespace Game
{
    public record TileComponent(
    Node3D Node,
    string Name,
    Vector3I Coord,
    TileType Type,
    int Index
);
}
