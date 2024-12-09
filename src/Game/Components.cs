using System.Collections.Generic;
using Godot;

namespace Game
{
    public record UnitComponent(
        Node3D Node,
        string Name,
        UnitType Type,
        int Health = 1,
        int Damage = 1,
        int MoveRange = 1,
        int AttackRange = 1,
        AnimationState AnimationState = AnimationState.Spawn
    );

    public record TileComponent(
        Node3D Node,
        string Name,
        Vector3I Coord,
        TileType Type,
        int Index
    );

    public record HexGridComponent(Dictionary<Vector3I, Entity> Grid);
    public record NameComponent(string Name);
    public record HexCoordComponent(Vector3I Coord);
    public record UnitTypeComponent(UnitType UnitType);
    public record AttackRangeComponent(int AttackRange);
    public record MoveRangeComponent(int MoveRange);
    public record HealthComponent(int Health);
    public record DamageComponent(int Damage);
    public record HexTileComponent(TileType Type, int Index);
    public record OccupantsComponent(IEnumerable<Entity> Occupants);
    public record RenderComponent(Node3D Node3D);
    public record SelectedComponent(bool Selected);
}