using Godot;

namespace Game.Components
{
    // markers
    public readonly record struct Tile;
    public readonly record struct Traversable;
    public readonly record struct Untraversable;
    public readonly record struct Player;
    public readonly record struct Enemy;
    public readonly record struct Grunt;
    public readonly record struct Sniper;

    // base components
    public record struct Name(string Value) { public static implicit operator string(Name name) => name.Value; }
    public record struct TileIndex(int Value) { public static implicit operator int(TileIndex index) => index.Value; }
    public record struct Unit(UnitType Value) { public static implicit operator UnitType(Unit type) => type.Value; }
    public record struct Instance(Node3D Value) { public static implicit operator Node3D(Instance node) => node.Value; }
    public record struct Health(int Value) { public static implicit operator int(Health health) => health.Value; }
    public record struct Coordinate(Vector3I Value) { public static implicit operator Vector3I(Coordinate coord) => coord.Value; }
    public record struct Damage(int Value) { public static implicit operator int(Damage damage) => damage.Value; }
    public record struct MoveRange(int Value) { public static implicit operator int(MoveRange range) => range.Value; }
    public record struct AttackRange(int Value) { public static implicit operator int(AttackRange range) => range.Value; }
}