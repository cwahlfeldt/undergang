using Godot;
using Game.Autoload;
using Game.Systems;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Game
{
    public partial class GameManager : Node3D
    {
        private EntityManager _entityManager;
        private GridManager _gridManager;
        private HexGridSystem _hexGridSystem;
        private PathFinderSystem _pathFinderSystem;
        private UnitSystem _unitSystem;
        private GridSystem _gridSystem;
        private TurnSystem _turnSystem;
        private UISystem _uiSystem;
        private TileHighlightSystem _tileHighlightSystem;
        private bool _playerActionInProgress = false;

        public override void _Ready()
        {
            SetupEventHandlers();

            _entityManager = new EntityManager(this);
            _gridManager = new GridManager(_entityManager);
            _gridSystem = new GridSystem(_entityManager);
            _hexGridSystem = new HexGridSystem(_entityManager, _gridSystem);
            _pathFinderSystem = new PathFinderSystem(_gridSystem);
            _unitSystem = new UnitSystem(_gridSystem);

            _turnSystem = new TurnSystem(_gridSystem);
            // _debugSystem = new DebugSystem(_entityManager);
            _tileHighlightSystem = new TileHighlightSystem(_gridSystem);

            _unitSystem.CreatePlayer(Config.PlayerStart);
            _unitSystem.CreateGrunt(_hexGridSystem.GetRandomFloorTile());
            _unitSystem.CreateGrunt(_hexGridSystem.GetRandomFloorTile());
            _unitSystem.CreateGrunt(_hexGridSystem.GetRandomFloorTile());

            _uiSystem = new UISystem(_entityManager);
            _turnSystem.StartCombat();
        }

        private void SetupEventHandlers()
        {
            EventBus.Instance.TurnChanged += OnTurnChanged;
            EventBus.Instance.TileSelect += OnTileSelect;
            EventBus.Instance.UnitDefeated += OnUnitDefeated;
        }

        private async void OnTileSelect(Entity tile)
        {
            if (_playerActionInProgress) return;

            var player = _entityManager.GetPlayer();
            var path = GetPathToTile(player, tile);

            if (path.Count > 0)
            {
                _playerActionInProgress = true;
                await ProcessPlayerTurn(player, path);
                _playerActionInProgress = false;
                _turnSystem.EndTurn();
            }
        }

        private List<Vector3I> GetPathToTile(Entity player, Entity tile)
        {
            var playerMoveRange = player.Get<MoveRangeComponent>().MoveRange;
            var currentPos = player.Get<HexCoordComponent>().Coord;
            var targetPos = tile.Get<HexCoordComponent>().Coord;
            return _pathFinderSystem.FindPath(currentPos, targetPos, playerMoveRange);
        }

        private async Task ProcessPlayerTurn(Entity player, List<Vector3I> path)
        {
            var initialAttackableEnemies = GetAttackableEnemies(player.Get<HexCoordComponent>().Coord);

            await _unitSystem.MoveUnit(player, path);

            HandlePlayerAttacks(player, initialAttackableEnemies, path.Last());
            HandleEnemyCounterattacks(player);
        }

        private List<Entity> GetAttackableEnemies(Vector3I position)
        {
            return _entityManager.GetUnitsInRange(position, 1)
                .Where(entity =>
                    entity.Has<UnitTypeComponent>() &&
                    entity.Get<UnitTypeComponent>().UnitType == UnitType.Enemy)
                .ToList();
        }

        private void HandlePlayerAttacks(Entity player, List<Entity> initialEnemies, Vector3I finalPosition)
        {
            var newAttackableTiles = _entityManager.GetTilesInRange(finalPosition, 1);
            foreach (var enemy in initialEnemies)
            {
                var enemyTile = _entityManager.GetEntityByCoord(enemy.Get<HexCoordComponent>().Coord);
                if (newAttackableTiles.Contains(enemyTile))
                {
                    _unitSystem.AttackUnit(player, enemy);
                }
            }
        }

        private void HandleEnemyCounterattacks(Entity player)
        {
            foreach (var enemy in _entityManager.GetEnemies())
            {
                var enemyRange = _entityManager.GetTilesInRange(enemy.Get<HexCoordComponent>().Coord, 1);
                var playerTile = _entityManager.GetEntityByCoord(player.Get<HexCoordComponent>().Coord);

                if (enemyRange.Contains(playerTile))
                {
                    _unitSystem.AttackUnit(enemy, player);
                    _uiSystem.RemoveHeart();
                }
            }
        }

        private async void OnTurnChanged(Entity unit)
        {
            if (unit.Get<UnitTypeComponent>().UnitType == UnitType.Enemy)
            {
                await ProcessEnemyTurn(unit);
            }
        }

        private async Task ProcessEnemyTurn(Entity enemy)
        {
            var player = _entityManager.GetPlayer();
            if (player == null) return;

            if (!IsPlayerInAttackRange(enemy, player))
            {
                await MoveEnemyTowardsPlayer(enemy, player);
            }

            _turnSystem.EndTurn();
        }

        private bool IsPlayerInAttackRange(Entity enemy, Entity player)
        {
            var attackableTiles = _entityManager.GetTilesInRange(enemy.Get<HexCoordComponent>().Coord, 1);
            var playerTile = _entityManager.GetEntityByCoord(player.Get<HexCoordComponent>().Coord);
            return attackableTiles.Contains(playerTile);
        }

        private async Task MoveEnemyTowardsPlayer(Entity enemy, Entity player)
        {
            var path = _pathFinderSystem.FindPath(
                enemy.Get<HexCoordComponent>().Coord,
                player.Get<HexCoordComponent>().Coord,
                enemy.Get<MoveRangeComponent>().MoveRange
            );

            if (path.Count > 0)
            {
                await _unitSystem.MoveUnit(enemy, path);
            }
        }

        private void OnUnitDefeated(Entity unit)
        {
            if (unit.Get<UnitTypeComponent>().UnitType == UnitType.Player)
            {
                HandlePlayerDefeat(unit);
            }
            else
            {
                HandleEnemyDefeat(unit);
            }
        }

        private void HandlePlayerDefeat(Entity player)
        {
            GD.Print("Game Over - Player Died!");
            _turnSystem.RemoveUnit(player);
            _entityManager.RemoveEntity(player);
        }

        private void HandleEnemyDefeat(Entity enemy)
        {
            _turnSystem.RemoveUnit(enemy);
            _entityManager.RemoveEntity(enemy);

            if (!_entityManager.GetEnemies().Any())
            {
                GD.Print("KILL EM ALL!");
            }
        }
    }
}