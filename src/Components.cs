namespace Game;

using Godot;

public enum UnitType { Player, Grunt }

public record NameComponent(string Name);
public record PositionComponent(Vector3 Position);
public record HexCoordComponent(Vector3I HexCoord);
public record UnitTypeComponent(UnitType UnitType);
public record AttackRangeComponent(int AttackRange);
public record MoveRangeComponent(int MoveRange);
public record HealthComponent(int Health);
public record HexTileComponent(TileType Type);
public record RenderComponent(Node3D Node3D);
public record HexGridComponent(int Radius);
public record InputReciever();