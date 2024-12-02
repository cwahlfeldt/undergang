namespace Undergang.Game;

using System.Collections.Generic;
using Godot;

public partial class HexGrid
{
    private const float HEX_SIZE = 1.05f;
    public Dictionary<Vector3I, HexTile> Tiles = [];
    private Node3D _rootNode;

    public static readonly Dictionary<string, Vector3I> Directions = new()
    {
        { "NorthWest", new Vector3I(-1, 0, 1) },
        { "North", new Vector3I(0, -1, 1) },
        { "NorthEast", new Vector3I(1, -1, 0) },
        { "SouthWest", new Vector3I(-1, 1, 0) },
        { "South", new Vector3I(0, 1, -1) },
        { "SouthEast", new Vector3I(1, 0, -1) }
    };


    public HexGrid(int radius, Node3D parent)
    {
        _rootNode = new Node3D();
        parent.AddChild(_rootNode);
        GenerateHexagonalGrid(radius);
    }

    private void GenerateHexagonalGrid(int radius)
    {
        PackedScene hexTileScene = ResourceLoader.Load<PackedScene>("res://src/Components/Grid/HexTile.tscn");
        var coordinates = GenerateHexCoordinates(radius);

        foreach (var coord in coordinates)
        {
            Vector3 worldPos = HexToWorld(coord);
            HexTile tile = hexTileScene.Instantiate<HexTile>();
            _rootNode.AddChild(tile);
            tile.Position = worldPos;
            tile.SetHexCoordinate(coord);
            Tiles[coord] = tile;
        }
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
                return b.Y.CompareTo(a.Y);
            return b.X.CompareTo(a.X);
        });
        return coords;
    }

    public List<Vector3I> GetNeighbors(Vector3I center)
    {
        var neighbors = new List<Vector3I>();
        foreach (var dir in Directions.Values)
        {
            neighbors.Add(center + dir);
        }
        return neighbors.FindAll(hex => Tiles.ContainsKey(hex));
    }

    public static Vector3 HexToWorld(Vector3I hexCoord)
    {
        float x = HEX_SIZE * (1.5f * hexCoord.X);
        float z = HEX_SIZE * (Mathf.Sqrt(3.0f) * (hexCoord.Y + hexCoord.X * 0.5f));
        return new Vector3(x, 0, z);
    }

    public Vector3I WorldToHex(Vector3 worldPos)
    {
        // Convert world position to fractional hex coordinates
        float q = (2.0f / 3.0f * worldPos.X) / HEX_SIZE;
        float r = (-1.0f / 3.0f * worldPos.X + Mathf.Sqrt(3.0f) / 3.0f * worldPos.Z) / HEX_SIZE;
        float s = -q - r;

        // Round to the nearest hex
        return RoundToHex(new Vector3(q, r, s));
    }


    public int GetDistance(Vector3I a, Vector3I b)
    {
        var diff = a - b;
        return (Mathf.Abs(diff.X) + Mathf.Abs(diff.Y) + Mathf.Abs(diff.Z)) / 2;
    }

    public List<Vector3I> GetRing(Vector3I center, int radius)
    {
        var results = new List<Vector3I>();
        if (radius <= 0) return results;

        // Start at the top-right hex of the ring
        var hex = center + new Vector3I(radius, -radius, 0);

        // Move in each of the 6 directions
        foreach (var direction in Directions.Values)
        {
            // Move radius times in each direction
            for (int i = 0; i < radius; i++)
            {
                results.Add(hex);
                hex += direction;
            }
        }
        return results.FindAll(hex => Tiles.ContainsKey(hex));
    }


    private Vector3I RoundToHex(Vector3 fractional)
    {
        // Round the fractional hex coordinates
        float q = Mathf.Round(fractional.X);
        float r = Mathf.Round(fractional.Y);
        float s = Mathf.Round(fractional.Z);

        // Calculate the differences
        float qDiff = Mathf.Abs(q - fractional.X);
        float rDiff = Mathf.Abs(r - fractional.Y);
        float sDiff = Mathf.Abs(s - fractional.Z);

        // Adjust the rounded values based on the differences
        if (qDiff > rDiff && qDiff > sDiff)
        {
            q = -r - s;
        }
        else if (rDiff > sDiff)
        {
            r = -q - s;
        }
        else
        {
            s = -q - r;
        }

        return new Vector3I((int)q, (int)r, (int)s);
    }

    public HexTile GetTile(Vector3I coord)
    {
        return Tiles.TryGetValue(coord, out var tile) ? tile : null;
    }
}