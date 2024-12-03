using System;
using Godot;

namespace Game.Autoload
{
    public partial class EventBus : Node
    {
        // [Signal] public delegate void TileClickedEventHandler(HexTile tile);
        public event Action<Entity> TurnChanged;
        public event Action<Entity> TileSelect;

        public static EventBus Instance { get; private set; }

        public override void _Ready()
        {
            Instance = this;
        }

        public void OnTileSelect(Entity tile)
        {
            TileSelect?.Invoke(tile);
        }

        public void OnTurnChanged(Entity tile)
        {
            TurnChanged?.Invoke(tile);
        }
    }
}
