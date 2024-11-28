// GameManager.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Godot;
public partial class GameManager : Node3D
{
    [Export] public PackedScene PlayerScene = ResourceLoader.Load<PackedScene>("res://src/Components/Units/Player/Player.tscn");
    [Export] public PackedScene EnemyScene = ResourceLoader.Load<PackedScene>("res://src/Components/Units/Enemies/Enemy.tscn");

    private HexGrid _grid;
    private PathFinder _pathFinder;
    private TurnManager _turnManager;
    private Player _player;
    private List<Enemy> _enemies = new();
    private StateManager _stateManager;

    public override void _Ready()
    {
        // Connect signals
        SignalBus.Instance.TileClicked += OnTileClicked;
        SignalBus.Instance.TurnChanged += OnTurnChanged;

        // Initialize grid first since other systems depend on it
        _grid = new HexGrid(5, this);

        // Initialize systems
        _pathFinder = new PathFinder(_grid.Tiles);
        _turnManager = new TurnManager();

        // Spawn units
        SpawnPlayer(new Vector3I(0, 4, -4));
        SpawnEnemy(new Vector3I(-2, 1, 3));
        SpawnEnemy(new Vector3I(2, -3, 1));

        // Start combat
        _turnManager.StartCombat(_player, _enemies);
        _stateManager = new StateManager(this);
    }

    private void OnTileClicked(HexTile tile)
    {
        if (!_stateManager.CanProcessInput()) return;
        // if (!_turnManager.IsUnitTurn(_player)) return;

        var targetHex = _grid.WorldToHex(tile.Position);
        var currentPos = _grid.WorldToHex(_player.Position);
        var path = _pathFinder.FindPath(currentPos, targetHex);

        if (path.Count > 0)
        {
            var movePos = path[1];

            // Check if this move would kill the player
            foreach (var enemy in _enemies)
            {
                var enemyPos = _grid.WorldToHex(enemy.Position);
                bool wasAdjacent = _grid.GetDistance(currentPos, enemyPos) == 1;
                bool willBeAdjacent = _grid.GetDistance(movePos, enemyPos) == 1;

                // If we're moving adjacent to an enemy from a non-adjacent position, player dies
                if (!wasAdjacent && willBeAdjacent)
                {
                    MovePlayer(movePos);
                    _player.QueueFree();
                    GameOver();
                    return;
                }
            }

            // If we're still alive, check for enemy kills
            var killedEnemies = _enemies.Where(enemy =>
                _grid.GetDistance(movePos, _grid.WorldToHex(enemy.Position)) == 1).ToList();

            MovePlayer(movePos);

            foreach (var enemy in killedEnemies)
            {
                _enemies.Remove(enemy);
                _turnManager.RemoveUnit(enemy);
                enemy.QueueFree();
            }

            if (_enemies.Count == 0)
            {
                Victory();
                return;
            }

            _turnManager.EndTurn();

        }
    }
    private void OnTurnChanged(Unit unit)
    {
        GD.Print($"Turn changed to: {unit.Name}");

        // Add delay before processing the next turn
        var timer = GetTree().CreateTimer(0.3f);
        timer.Timeout += () =>
        {
            if (unit is Enemy enemy)
            {
                ProcessEnemyTurn(enemy);
                _turnManager.EndTurn();
            }
        };
    }

    public void ProcessEnemyTurn(Enemy enemy)
    {
        var playerPos = _grid.WorldToHex(_player.Position);
        var currentEnemyPos = _grid.WorldToHex(enemy.Position);

        // Move enemy toward player
        var path = _pathFinder.FindPath(currentEnemyPos, playerPos);
        if (path.Count > 0)
        {
            enemy.Position = _grid.HexToWorld(path[1]);
        }
    }

    public void GameOver()
    {
        GD.Print("Game Over - Player Died!");
        // Add any game over logic here
    }

    public void Victory()
    {
        GD.Print("KILL EM ALL!");
        // Add any game over logic here
    }

    public void MovePlayer(Vector3I hexCoord)
    {
        var worldPos = _grid.HexToWorld(hexCoord);
        _player.Position = worldPos;
    }

    public void SpawnPlayer(Vector3I hexCoord)
    {
        _player = PlayerScene.Instantiate<Player>();
        AddChild(_player);
        _player.Position = _grid.HexToWorld(hexCoord);
    }

    public void SpawnEnemy(Vector3I hexCoord)
    {
        var enemy = EnemyScene.Instantiate<Enemy>();
        AddChild(enemy);
        enemy.Position = _grid.HexToWorld(hexCoord);
        _enemies.Add(enemy);
    }
}