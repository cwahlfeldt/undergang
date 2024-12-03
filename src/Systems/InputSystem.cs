using System.Collections.Generic;
using Godot;
using Game.Autoload;

namespace Game.Systems
{
    public class InputSystem
    {
        private readonly EntityManager _entityManager;
        private readonly HexGridSystem _hexGridSystem;
        private Entity _selectedTile;
        private readonly HashSet<Entity> _highlightedTiles = new();
        private readonly ShaderMaterial _highlightMaterial;
        private readonly ShaderMaterial _selectedMaterial;
        private readonly ShaderMaterial _defaultMaterial;

        public InputSystem(EntityManager entityManager, HexGridSystem hexGridSystem)
        {
            _entityManager = entityManager;
            _hexGridSystem = hexGridSystem;

            // Load shader materials
            _highlightMaterial = ResourceLoader.Load<ShaderMaterial>("res://assets/materials/HexTileHighlight.tres");
            _selectedMaterial = ResourceLoader.Load<ShaderMaterial>("res://assets/materials/HexTileSelected.tres");
            _defaultMaterial = ResourceLoader.Load<ShaderMaterial>("res://assets/materials/HexTileBase.tres");

            // Subscribe to events
            EventBus.Instance.TileSelect += OnTileSelect;
            EventBus.Instance.TileHover += OnTileHover;
            EventBus.Instance.TileUnhover += OnTileUnhover;
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

        private void SelectTile(Entity tile)
        {
            ClearSelection();
            _selectedTile = tile;
            SetTileMaterial(_selectedTile, _selectedMaterial);

            // Highlight neighboring tiles
            var neighbors = _hexGridSystem.GetNeighborTiles(tile.Get<HexCoordComponent>().HexCoord);
            foreach (var neighbor in neighbors)
            {
                if (neighbor != _selectedTile)
                {
                    _highlightedTiles.Add(neighbor);
                    SetTileMaterial(neighbor, _highlightMaterial);
                }
            }

            EventBus.Instance.OnTileSelect(tile);
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

        private void SetTileMaterial(Entity tile, ShaderMaterial material)
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