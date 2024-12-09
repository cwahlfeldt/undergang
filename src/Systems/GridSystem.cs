using Game.Autoload;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Systems
{
    public class GridSystem(EntityManager entityManager)
    {
        private readonly PackedScene _tileScene = ResourceLoader.Load<PackedScene>("res://src/Scenes/HexTile.tscn");
        private readonly Node3D _gridContainer;

        public void RenderGrid()
        {

        }
    }
}