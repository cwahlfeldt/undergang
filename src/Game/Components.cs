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
}