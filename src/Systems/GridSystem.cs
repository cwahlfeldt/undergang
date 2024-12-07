using Game.Autoload;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class GridSystem
    {
        private readonly Dictionary<Vector3I, Entity> _tiles = [];
        private readonly Dictionary<Vector3I, Entity> _units = [];
        private readonly EntityManager _entityManager;

        public GridSystem(EntityManager entityManager)
        {
            _entityManager = entityManager;
            Clear();
        }

        public Dictionary<Vector3I, Entity> GetTiles()
        {
            return _tiles;
        }

        public Dictionary<Vector3I, Entity> GetUnits()
        {
            return _units;
        }

        public void RegisterTile(Vector3I coord, Entity tile)
        {
            _tiles[coord] = tile;
        }

        public void RegisterUnit(Vector3I coord, Entity unit)
        {
            _units[coord] = unit;
        }

        public void MoveUnit(Vector3I from, Vector3I to)
        {
            if (_units.TryGetValue(from, out var unit))
            {
                _units.Remove(from);
                _units[to] = unit;
            }
        }

        public bool IsCoordOccupied(Vector3I coord)
        {
            return _units.ContainsKey(coord);
        }

        public Entity GetTileAt(Vector3I coord)
        {
            return _tiles.TryGetValue(coord, out var tile) ? tile : null;
        }

        public Entity GetUnitAt(Vector3I coord)
        {
            return _units.TryGetValue(coord, out var unit) ? unit : null;
        }

        public IEnumerable<Entity> GetUnitsInRange(Vector3I center, int range)
        {
            var hexesInRange = HexGrid.GetHexesInRange(center, range);
            return hexesInRange.Where(_units.ContainsKey)
                               .Select(coord => _units[coord]);
        }

        public IEnumerable<Entity> GetAdjacentUnits(Vector3I coord)
        {
            return HexGrid.GetHexesInRange(coord, 2)
                         .Where(c => _units.ContainsKey(c))
                         .Select(c => _units[c]);
        }

        public void RemoveUnit(Vector3I coord)
        {
            _units.Remove(coord);
        }

        public void RemoveTile(Vector3I coord)
        {
            _tiles.Remove(coord);
        }

        public void Clear()
        {
            _tiles.Clear();
            _units.Clear();
        }
    }
}