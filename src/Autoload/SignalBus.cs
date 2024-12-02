using System;
using Godot;
using Undergang.Entities;

public partial class SignalBus : Node
{
    [Signal] public delegate void TileClickedEventHandler(HexTile tile);
    public event Action<Entity> TurnChangedEventHandler;

    public static SignalBus Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }
}

