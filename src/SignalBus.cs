using Godot;

public partial class SignalBus : Node
{
    [Signal] public delegate void TileClickedEventHandler(HexTile tile);
    [Signal] public delegate void TurnChangedEventHandler(Unit unit);

    public static SignalBus Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }
}

