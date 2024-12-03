using System.Collections.Generic;
using System.Linq;
using Godot;
using Game.Autoload;
using System;

namespace Game.Systems
{
    public class HexGridSystem
    {
        private const float HEX_SIZE = 1.05f;
        private readonly EntityManager _entityManager;
        private Node3D _hexGridContainer;
        private readonly PackedScene _hexTileScene = ResourceLoader.Load<PackedScene>("res://src/Scenes/HexTile.tscn");

        public HexGridSystem(EntityManager entityManager, int radius)
        {
            _entityManager = entityManager;
            CreateHexGrid(radius);
        }

        public static readonly Dictionary<string, Vector3I> Directions = new()
        {
            { "NorthWest", new Vector3I(-1, 0, 1) },
            { "North", new Vector3I(0, -1, 1) },
            { "NorthEast", new Vector3I(1, -1, 0) },
            { "SouthWest", new Vector3I(-1, 1, 0) },
            { "South", new Vector3I(0, 1, -1) },
            { "SouthEast", new Vector3I(1, 0, -1) }
        };

        public void CreateHexGrid(int radius, int blockedTilesAmt = 12)
        {
            _hexGridContainer = new Node3D
            {
                Name = "HexGrid"
            };
            _entityManager.GetRootNode().AddChild(_hexGridContainer);

            var gridEntity = new Entity(_entityManager.GetNextId());
            gridEntity.Add(new HexGridComponent(radius));
            gridEntity.Add(new NameComponent(_hexGridContainer.Name));
            gridEntity.Add(new RenderComponent(_hexGridContainer));

            // Generate the coordinates
            var coordinates = GenerateHexCoordinates(radius);
            var randBlockedTileIndices = GenerateRandomIntArray(blockedTilesAmt);

            // Create tile entities
            var index = 0;
            foreach (var coord in coordinates)
            {
                var tileType = randBlockedTileIndices.Contains(index) ? TileType.Blocked : TileType.Floor;
                CreateTileEntity(coord, index, tileType);
                index++;
            }

            _entityManager.AddEntity(gridEntity);
        }

        private void CreateTileEntity(Vector3I hexCoord, int index, TileType tileType)
        {
            var tileEntity = new Entity(_entityManager.GetNextId());
            var tileNode = _hexTileScene.Instantiate<Node3D>();
            _hexGridContainer.AddChild(tileNode);

            if (tileNode is Area3D tileBody)
            {
                tileBody.InputEvent += (camera, @event, position, normal, shapeIdx) =>
                {
                    if (@event is InputEventMouseButton mouseEvent &&
                        mouseEvent.ButtonIndex == MouseButton.Left &&
                        mouseEvent.Pressed)
                    {
                        EventBus.Instance.OnTileSelect(tileEntity);
                    }
                };
            }

            if (tileType == TileType.Blocked)
            {
                tileNode.GetNode<MeshInstance3D>("Mesh").Visible = false;
            }

            tileNode.Name = $"Tile {hexCoord} {index}";
            tileNode.Position = HexToWorld(hexCoord);
            tileEntity.Add(new RenderComponent(tileNode));
            tileEntity.Add(new HexCoordComponent(hexCoord));
            tileEntity.Add(new HexTileComponent(tileType, index));
            tileEntity.Add(new NameComponent(tileNode.Name));

            _entityManager.AddEntity(tileEntity);
        }

        public List<Entity> GetNeighborTiles(Vector3I center)
        {
            var neighbors = new List<Entity>();
            foreach (var direction in Directions.Values)
            {
                var neighborCoord = center + direction;
                var neighborTile = GetTileAtCoordinate(neighborCoord);
                if (neighborTile != null && neighborTile.Get<HexTileComponent>().Type != TileType.Blocked)
                {
                    neighbors.Add(neighborTile);
                }
            }
            return neighbors;
        }

        public Entity GetTileAtCoordinate(Vector3I hexCoord)
        {
            return _entityManager.GetEntities().Values
                .FirstOrDefault(e =>
                    e.Has<HexTileComponent>() &&
                    e.Has<HexCoordComponent>() &&
                    e.Get<HexCoordComponent>().HexCoord == hexCoord);
        }

        private List<Vector3I> GenerateHexCoordinates(int mapSize)
        {
            var coords = new List<Vector3I>();
            for (int q = -mapSize; q <= mapSize; q++)
            {
                int r1 = Mathf.Max(-mapSize, -q - mapSize);
                int r2 = Mathf.Min(mapSize, -q + mapSize);
                for (int r = r1; r <= r2; r++)
                {
                    int s = -q - r;
                    coords.Add(new Vector3I(q, r, s));
                }
            }

            coords.Sort((a, b) =>
                {
                    if (a.Y != b.Y)
                        return a.Y.CompareTo(b.Y);  // Changed to ascending Y
                    return a.X.CompareTo(b.X);      // Changed to ascending X
                });

            return coords;
        }

        public static Vector3 HexToWorld(Vector3I hexCoord)
        {
            float x = HEX_SIZE * (1.5f * hexCoord.X);
            float z = HEX_SIZE * (Mathf.Sqrt(3.0f) * (hexCoord.Y + hexCoord.X * 0.5f));
            return new Vector3(x, 0, z);
        }

        public Vector3I WorldToHex(Vector3 worldPos)
        {
            float q = (2.0f / 3.0f * worldPos.X) / HEX_SIZE;
            float r = (-1.0f / 3.0f * worldPos.X + Mathf.Sqrt(3.0f) / 3.0f * worldPos.Z) / HEX_SIZE;
            float s = -q - r;
            return RoundToHex(new Vector3(q, r, s));
        }

        private Vector3I RoundToHex(Vector3 fractional)
        {
            float q = Mathf.Round(fractional.X);
            float r = Mathf.Round(fractional.Y);
            float s = Mathf.Round(fractional.Z);

            float qDiff = Mathf.Abs(q - fractional.X);
            float rDiff = Mathf.Abs(r - fractional.Y);
            float sDiff = Mathf.Abs(s - fractional.Z);

            if (qDiff > rDiff && qDiff > sDiff)
                q = -r - s;
            else if (rDiff > sDiff)
                r = -q - s;
            else
                s = -q - r;

            return new Vector3I((int)q, (int)r, (int)s);
        }

        public int GetDistance(Vector3I a, Vector3I b)
        {
            var diff = a - b;
            return (Mathf.Abs(diff.X) + Mathf.Abs(diff.Y) + Mathf.Abs(diff.Z)) / 2;
        }

        private int[] GenerateRandomIntArray(int size)
        {
            Random rand = new();
            int[] array = new int[size];

            for (int i = 0; i < size; i++)
            {
                var randNum = rand.Next(20, 90);
                array[i] = randNum;
            }

            return array;
        }

    }
}