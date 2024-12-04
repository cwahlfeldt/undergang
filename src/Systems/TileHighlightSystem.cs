using System.Collections.Generic;
using Godot;
using Game.Autoload;
using System.Threading.Tasks;
using System;

namespace Game.Systems
{
    public class TileHighlightSystem
    {
        private readonly EntityManager _entityManager;
        private readonly HashSet<Entity> _highlightedTiles = new();
        private readonly StandardMaterial3D _highlightMaterial;
        private readonly StandardMaterial3D _selectedMaterial;
        private readonly StandardMaterial3D _defaultMaterial;
        private Entity _selectedTile;

        public TileHighlightSystem(EntityManager entityManager)
        {
            _entityManager = entityManager;

            // Load shader materials
            _highlightMaterial = ResourceLoader.Load<StandardMaterial3D>("res://assets/materials/HexTileHighlight.tres");
            _selectedMaterial = ResourceLoader.Load<StandardMaterial3D>("res://assets/materials/HexTileSelect.tres");
            _defaultMaterial = ResourceLoader.Load<StandardMaterial3D>("res://assets/materials/HexTileBase.tres");

            // Subscribe to events
            EventBus.Instance.TileSelect += OnTileSelect;
            EventBus.Instance.TileHover += OnTileHover;
            EventBus.Instance.TileUnhover += OnTileUnhover;
            EventBus.Instance.TurnChanged += OnTurnChanged;
        }

        private void OnTurnChanged(Entity unit)
        {
            if (unit.Get<UnitTypeComponent>().UnitType == UnitType.Player)
            {
                SelectMoveRangeTiles(unit);
            }
        }

        private void OnTileHover(Entity tile)
        {
            if (tile != null &&
                tile.Get<HexTileComponent>().Type != TileType.Blocked &&
                tile != _selectedTile &&
                !_highlightedTiles.Contains(tile))
            {
                SetTileMaterial(tile, _highlightMaterial);
            }
        }

        private void OnTileUnhover(Entity tile)
        {
            if (tile != null &&
                tile != _selectedTile &&
                !_highlightedTiles.Contains(tile))
            {
                SetTileMaterial(tile, _defaultMaterial);
            }
        }

        private void OnTileSelect(Entity tile)
        {
            if (tile.Get<HexTileComponent>().Type != TileType.Blocked)
            {
                SelectTile(tile);
            }
        }

        public async void SelectTile(Entity entity)
        {
            ClearSelection();
            _selectedTile = entity;
            SetTileMaterial(_selectedTile, _selectedMaterial);
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            ClearSelection();
        }

        private void SelectMoveRangeTiles(Entity entity)
        {
            ClearSelection();
            var moveRangeMat = ResourceLoader.Load<StandardMaterial3D>("res://assets/materials/HexTileMoveRange.tres");

            // Highlight neighboring tiles
            var rangedTiles = HexGrid.GetTilesInRange(entity.Get<HexCoordComponent>().Coord, entity.Get<MoveRangeComponent>().MoveRange);
            foreach (var tile in rangedTiles)
            {
                var e = _entityManager.GetEntityByHexCoord(tile);
                if (e != _selectedTile)
                {
                    _highlightedTiles.Add(e);
                    SetTileMaterial(e, moveRangeMat);
                }
            }
        }

        private void ClearSelection()
        {
            if (_selectedTile != null)
            {
                SetTileMaterial(_selectedTile, _defaultMaterial);
                _selectedTile = null;
            }

            foreach (var tile in _highlightedTiles)
            {
                SetTileMaterial(tile, _defaultMaterial);
            }
            _highlightedTiles.Clear();
        }

        private void SetTileMaterial(Entity tile, StandardMaterial3D material)
        {
            var renderComponent = tile.Get<RenderComponent>();
            if (renderComponent?.Node3D is Node3D node)
            {
                var mesh = node.GetNode<MeshInstance3D>("Mesh");
                if (mesh != null)
                {
                    mesh.MaterialOverride = material;
                }
            }
        }

        public void Cleanup()
        {
            EventBus.Instance.TileSelect -= OnTileSelect;
            EventBus.Instance.TileHover -= OnTileHover;
            EventBus.Instance.TileUnhover -= OnTileUnhover;
            ClearSelection();
        }
    }
}