using Godot;
using Game.Autoload;
using Game.Systems;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Game
{
    public partial class GameManager : Node3D
    {
        private EntityManager _entityManager;
        private HexGridSystem _hexGridSystem;
        private PathFinderSystem _pathFinderSystem;
        private UnitSystem _unitSystem;
        private TurnSystem _turnSystem;
        private TileHighlightSystem _tileHighlightSystem;
        private DebugSystem _debugSystem;
        private bool _playerActionInProgress = false;

        public override void _Ready()
        {
            EventBus.Instance.TurnChanged += OnTurnChanged;
            EventBus.Instance.TileSelect += OnTileSelect;
            EventBus.Instance.UnitDefeated += OnUnitDefeated;

            _entityManager = new EntityManager(this);
            _hexGridSystem = new HexGridSystem(_entityManager, 5);
            _pathFinderSystem = new PathFinderSystem(_entityManager);
            _unitSystem = new UnitSystem(_entityManager);
            _turnSystem = new TurnSystem(_entityManager);
            _debugSystem = new DebugSystem(_entityManager);
            _tileHighlightSystem = new TileHighlightSystem(_entityManager);

            var player = _unitSystem.CreatePlayer(Config.PlayerStart);
            _unitSystem.CreateGrunt(_hexGridSystem.GetRandomFloorTile());
            _unitSystem.CreateGrunt(_hexGridSystem.GetRandomFloorTile());
            _unitSystem.CreateGrunt(_hexGridSystem.GetRandomFloorTile());

            _turnSystem.StartCombat();
        }


        private async void OnTileSelect(Entity tile)
        {
            if (_playerActionInProgress)
                return;

            var player = _entityManager.GetPlayer();
            var playerMoveRange = player.Get<MoveRangeComponent>().MoveRange;
            var tileCoord = tile.Get<HexCoordComponent>().Coord;
            var currentPos = player.Get<HexCoordComponent>().Coord;
            var path = _pathFinderSystem.FindPath(currentPos, tileCoord, playerMoveRange);

            if (path.Count > 0)
            {
                // Get the tiles we can currently attack from our position
                var currentAttackableTiles = _entityManager.GetTilesInRange(currentPos, 1);

                // Find enemies occupying those tiles before we move
                var initialAttackableEnemies = currentAttackableTiles
                    .SelectMany(tile => tile.Get<OccupantsComponent>().Occupants)
                    .Where(occupant =>
                        occupant.Has<UnitTypeComponent>() &&
                        occupant.Get<UnitTypeComponent>().UnitType == UnitType.Enemy)
                    .ToList();

                _playerActionInProgress = true;
                await _unitSystem.MoveUnit(player, path);

                // Get the tiles we can now attack from our new position
                var newAttackableTiles = _entityManager.GetTilesInRange(path.Last(), 1);

                // Kill enemies that were attackable before and are still in attackable tiles
                if (initialAttackableEnemies.Any())
                {
                    foreach (var enemy in initialAttackableEnemies)
                    {
                        // Check if enemy's current tile is in our new attack range
                        var enemyTile = _entityManager.GetEntityByCoord(
                            enemy.Get<HexCoordComponent>().Coord);

                        if (newAttackableTiles.Contains(enemyTile))
                        {
                            _unitSystem.AttackUnit(player, enemy);
                        }
                    }
                }

                _playerActionInProgress = false;
                _turnSystem.EndTurn();
            }
        }

        public async Task ProcessEnemyTurn(Entity enemy)
        {
            var player = _entityManager.GetPlayer();
            if (player == null) return;

            var enemyPos = enemy.Get<HexCoordComponent>().Coord;
            var attackableTiles = _entityManager.GetTilesInRange(enemyPos, 1);

            // Check if player is in any attackable tile
            var playerTile = _entityManager.GetEntityByCoord(
                player.Get<HexCoordComponent>().Coord);

            if (attackableTiles.Contains(playerTile))
            {
                _unitSystem.AttackUnit(enemy, player);
            }

            // If can't attack player, move towards them
            var playerPos = player.Get<HexCoordComponent>().Coord;
            var enemyMoveRange = enemy.Get<MoveRangeComponent>().MoveRange;
            var path = _pathFinderSystem.FindPath(enemyPos, playerPos, enemyMoveRange);

            if (path.Count > 0)
            {
                await _unitSystem.MoveUnit(enemy, path);
            }

            _turnSystem.EndTurn();
        }

        private async void OnTurnChanged(Entity unit)
        {
            GD.Print($"Turn changed to {unit.Get<NameComponent>().Name}");

            if (unit.Get<UnitTypeComponent>().UnitType == UnitType.Enemy)
            {
                await ProcessEnemyTurn(unit);
            }
        }

        private void OnUnitDefeated(Entity unit)
        {
            if (unit.Get<UnitTypeComponent>().UnitType == UnitType.Player)
            {
                GameOver();
                _turnSystem.RemoveUnit(unit);
                _entityManager.RemoveEntity(unit);
                return;
            }

            if (!_entityManager.GetEnemies().Any())
            {
                Victory();
                return;
            }

            _turnSystem.RemoveUnit(unit);
            _entityManager.RemoveEntity(unit);

        }

        public void GameOver()
        {
            GD.Print("Game Over - Player Died!");
        }

        public void Victory()
        {
            GD.Print("KILL EM ALL!");
        }
    }
}