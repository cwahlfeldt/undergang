using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Autoload;
using Godot;

namespace Game.Systems
{
    public class GameSystem(
        EntityManager entityManager,
        SpatialSystem spatialSystem,
        HexGridSystem hexGridSystem,
        UnitSystem unitSystem,
        PathFinderSystem pathFinderSystem,
        TurnSystem turnSystem
        )
    {
        private EntityManager _entityManager = entityManager;
        private HexGridSystem _hexGridSystem = hexGridSystem;
        private PathFinderSystem _pathFinderSystem = pathFinderSystem;
        private UnitSystem _unitSystem = unitSystem;
        private SpatialSystem _spatialSystem = spatialSystem;
        private TurnSystem _turnSystem = turnSystem;
        private bool _playerActionInProgress = false;

        public void StartGame()
        {
            EventBus.Instance.TurnChanged += OnTurnChanged;
            EventBus.Instance.TileSelect += OnTileSelect;
            EventBus.Instance.UnitDefeated += OnUnitDefeated;

            _turnSystem.StartCombat();
        }

        private async void OnTileSelect(Entity tile)
        {
            if (_playerActionInProgress)
                return;

            var player = _entityManager.GetPlayer();
            var moveRange = player.Get<MoveRangeComponent>().MoveRange;
            var targetCoord = tile.Get<HexCoordComponent>().Coord;
            var playerCoord = player.Get<HexCoordComponent>().Coord;

            var path = _pathFinderSystem.FindPath(playerCoord, targetCoord, moveRange);

            if (path.Count > 0)
            {
                _playerActionInProgress = true;

                // Get attackable enemies before moving
                var initialAttackableEnemies = GetAttackableEnemies(playerCoord);

                // Move the player
                await _unitSystem.MoveUnit(player, path);
                var newPosition = path.Last();

                // Get new attackable enemies after moving
                var finalAttackableEnemies = GetAttackableEnemies(newPosition);

                // Attack enemies that were attackable before and still are
                foreach (var enemy in initialAttackableEnemies)
                {
                    if (finalAttackableEnemies.Contains(enemy))
                    {
                        _unitSystem.AttackUnit(player, enemy);
                    }
                }

                _playerActionInProgress = false;
                _turnSystem.EndTurn();
            }
        }

        private IEnumerable<Entity> GetAttackableEnemies(Vector3I position)
        {
            return _spatialSystem.GetAdjacentUnits(position)
                .Where(unit =>
                    unit.Has<UnitTypeComponent>() &&
                    unit.Get<UnitTypeComponent>().UnitType == UnitType.Enemy)
                .ToList();
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
