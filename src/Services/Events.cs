using System;
using System.Threading.Tasks;
using Godot;

namespace Game
{
    public partial class Events : Node, ISystem
    {
        public event Action<Entity> UnitDefeated;
        public event Action<Entity, Vector3I, Vector3I> MoveCompleted;
        public event Action<Entity> TurnChanged;
        public event Action<Entity> TileSelect;
        public event Action<Vector3I> TileClick;
        public event Action<Entity> TileHover;
        public event Action<Entity> TileUnhover;
        public event Action<Entity> UnitHover;
        public event Action<Entity> UnitUnhover;
        public event Action<Type, object> ComponentChanged;

        public static Events Instance { get; private set; }

        public override void _Ready()
        {
            Instance = this;
        }

        public void OnMoveCompleted(Entity unit, Vector3I from, Vector3I to)
        {
            MoveCompleted?.Invoke(unit, from, to);
        }

        public void OnUnitDefeated(Entity unit)
        {
            UnitDefeated?.Invoke(unit);
        }

        public void OnComponentChanged(Type type, object obj)
        {
            ComponentChanged?.Invoke(type, obj);
        }

        public void OnTileSelect(Entity tile)
        {
            TileSelect?.Invoke(tile);
        }

        public void OnTileClick(Vector3I coord)
        {
            TileClick?.Invoke(coord);
        }

        public async Task OnTurnChanged(Entity tile)
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
