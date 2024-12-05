using System;
using Godot;

namespace Game.Autoload
{
    public partial class EventBus : Node
    {
        public event Action<Entity> UnitDefeated;
        public event Action<Entity> TurnChanged;
        public event Action<Entity> TileSelect;
        public event Action<Entity> TileHover;
        public event Action<Entity> TileUnhover;
        public event Action<Entity> UnitHover;
        public event Action<Entity> UnitUnhover;
        public event Action<Type, object> OnComponentChanged;

        public static EventBus Instance { get; private set; }

        public override void _Ready()
        {
            Instance = this;
        }

        public void OnUnitDefeated(Entity unit)
        {
            UnitDefeated?.Invoke(unit);
        }


        public void OnTileSelect(Entity tile)
        {
            TileSelect?.Invoke(tile);
        }

        public void OnTurnChanged(Entity tile)
        {
            TurnChanged?.Invoke(tile);
        }

        public void OnTileHover(Entity tile)
        {
            TileHover?.Invoke(tile);
        }

        public void OnTileUnhover(Entity tile)
        {
            TileUnhover?.Invoke(tile);
        }

        public void OnUnitHover(Entity tile)
        {
            UnitHover?.Invoke(tile);
        }

        public void OnUnitUnhover(Entity tile)
        {
            UnitUnhover?.Invoke(tile);
        }
    }
}
