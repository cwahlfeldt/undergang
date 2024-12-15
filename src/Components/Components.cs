using System;
using System.Collections.Generic;
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
    public readonly record struct Attacker;
    public readonly record struct Target;
    public readonly record struct Active;

    // wack ass
    public record struct Instance(Node3D Node) { public static implicit operator Node3D(Instance node) => node.Node; }

    // base components
    public record struct Name(StringName Value) { public static implicit operator StringName(Name name) => name.Value; }
    public record struct TileIndex(int Value) { public static implicit operator int(TileIndex index) => index.Value; }
    public record struct Unit(UnitType Type)
    {
        public static implicit operator UnitType(Unit type) => type.Type;
        public override readonly string ToString() => Type.ToString();
    }
    public record struct Health(int Value) { public static implicit operator int(Health health) => health.Value; }
    public record struct Coordinate(Vector3I Value) { public static implicit operator Vector3I(Coordinate coord) => coord.Value; }
    public record struct Damage(int Value) { public static implicit operator int(Damage damage) => damage.Value; }
    public record struct MoveRange(int Value) { public static implicit operator int(MoveRange range) => range.Value; }
    public record struct AttackRange(int Value) { public static implicit operator int(AttackRange range) => range.Value; }

    public record struct Movement(Vector3I From, Vector3I To)
    {
        public static implicit operator (Vector3I, Vector3I)(Movement movement) =>
            (movement.From, movement.To);
    }

    public record struct Path(List<Vector3I> Value)
    {
        public static implicit operator List<Vector3I>(Path path) =>
           path.Value;
    }
}