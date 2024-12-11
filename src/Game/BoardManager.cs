using Godot;
using Game.Autoload;
using Game.Systems;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Game
{
    public partial class BoardManager : Node3D
    {
        private bool _enablePathfinderDebug = true;
        [Export]
        public bool EnablePathfinderDebug
        {
            get => _enablePathfinderDebug;
            set
            {
                _debugSystem.TogglePathfindingDebug();
                _enablePathfinderDebug = value;
            }
        }

        [Export] public bool _enableHexCoordDebug = false;

        private EntityManager _entityManager;
        private EntityFactory _entityFactory;
        private RenderSystem _renderSystem;
        private PathFinderSystem _pathFinderSystem;
        private TurnSystem _turnSystem;
        private MovementSystem _movementSystem;
        private UISystem _uiSystem;
        private DebugSystem _debugSystem;
        private TileHighlightSystem _tileHighlightSystem;
        private bool _playerActionInProgress = false;

        public override void _Ready()
        {
            SetupEventHandlers();

            _entityManager = new EntityManager(this);
            _entityFactory = new EntityFactory(_entityManager);
            _renderSystem = new RenderSystem(_entityManager);
            _pathFinderSystem = new PathFinderSystem(_entityManager);
            _turnSystem = new TurnSystem(_entityManager);
            _movementSystem = new MovementSystem(_entityManager);

            _entityFactory.CreateGrid(5);
            _entityFactory.SpawnPlayer(Config.PlayerStart);
            _entityFactory.SpawnGrunt(_entityManager.GetRandTileEntity().Get<TileComponent>().Coord);
            _entityFactory.SpawnGrunt(_entityManager.GetRandTileEntity().Get<TileComponent>().Coord);
            _entityFactory.SpawnGrunt(_entityManager.GetRandTileEntity().Get<TileComponent>().Coord);

            _uiSystem = new UISystem(_entityManager);

            _renderSystem.RenderBoard();
            _pathFinderSystem.SetupPathfinding();
            _turnSystem.StartCombat();

            _debugSystem = new DebugSystem(_entityManager, _pathFinderSystem);
            if (_enablePathfinderDebug)
                _debugSystem.TogglePathfindingDebug();

            _tileHighlightSystem = new TileHighlightSystem(_entityManager, _pathFinderSystem);
        }

        private void SetupEventHandlers()
        {
            EventBus.Instance.TurnChanged += OnTurnChanged;
            EventBus.Instance.TileSelect += OnTileSelect;
        }

        private async void OnTileSelect(Entity entity)
        {
            if (_playerActionInProgress)
                return;

            var player = _entityManager.GetPlayer();
            if (entity.Get<TileComponent>().Coord == player.coord)
                return;
            if (!_turnSystem.IsUnitTurn(player.entity))
                return;

            var path = _pathFinderSystem.FindPath(
                player.coord,
                entity.Get<TileComponent>().Coord,
                player.unit.MoveRange
            );

            if (path.Count > 0)
            {
                _playerActionInProgress = true;
                await ProcessPlayerTurn(player.entity, path);
                _playerActionInProgress = false;
                _turnSystem.EndTurn();
            }
        }

        public void AttackUnit(Entity attacker, Entity target)
        {
            var attackerComponent = attacker.Get<UnitComponent>();
            var targetComponent = target.Get<UnitComponent>();
            var damageDone = targetComponent.Health - attackerComponent.Damage;

            if (target.Get<UnitComponent>().Type == UnitType.Player)
                _uiSystem.RemoveHeart();

            if (damageDone <= 0)
            {
                _turnSystem.RemoveUnit(target);
                targetComponent.Node.QueueFree();
                target.Remove(targetComponent);
                return;
            }

            target.Update(targetComponent with { Health = damageDone });
        }

        private async Task ProcessPlayerTurn(Entity player, List<Vector3I> path)
        {
            var startCoord = player.Get<TileComponent>().Coord;
            var initialAttackableEnemies = GetAttackableEnemies(startCoord);

            await _movementSystem.MoveUnitTo(player, path);

            var finalCoord = path.Last();

            HandlePlayerAttacks(finalCoord, initialAttackableEnemies);
            HandleEnemyCounterattacks(finalCoord);
        }

        private void HandlePlayerAttacks(Vector3I playerCoord, List<Entity> initialEnemies)
        {
            var (player, coord, entity) = _entityManager.GetPlayer();
            var attackableTiles = _entityManager.GetTilesInRange(playerCoord, 1);

            foreach (var enemy in initialEnemies)
            {
                if (attackableTiles.Contains(enemy))
                {
                    AttackUnit(entity, enemy);
                }
            }
        }

        private void HandleEnemyCounterattacks(Vector3I playerCoord)
        {
            var enemyCoords = _entityManager.GetEnemies()
                .Select(tile => tile.Get<TileComponent>().Coord);

            foreach (var enemyCoord in enemyCoords)
            {
                var enemyRange = _entityManager.GetTilesInRange(enemyCoord, 1);
                var playerTile = _entityManager.GetAt(playerCoord);

                if (enemyRange.Contains(playerTile))
                {
                    AttackUnit(_entityManager.GetAt(enemyCoord), _entityManager.GetAt(playerCoord));
                }
            }
        }

        private List<Entity> GetAttackableEnemies(Vector3I position)
        {
            return _entityManager.GetTilesInRange(position, 1)
                .Where(entity => entity.Has<UnitComponent>() &&
                    entity.Get<UnitComponent>().Type == UnitType.Grunt)
                .ToList();
        }

        private async void OnTurnChanged(Entity entity)
        {
            if (!entity.Has<UnitComponent>())
                return;

            GD.Print($"turn is {entity.Get<UnitComponent>().Name}");

            var player = _entityManager.GetPlayer();

            if (player.entity != null && !_turnSystem.IsUnitTurn(player.entity))
                await ProcessEnemyTurn(entity);
        }

        private async Task ProcessEnemyTurn(Entity enemy)
        {
            var (_, _, player) = _entityManager.GetPlayer();

            if (player == null)
                return;

            if (!IsPlayerInAttackRange(enemy, player))
                await MoveEnemyTowardsPlayer(enemy, player);

            _turnSystem.EndTurn();
        }

        private async Task MoveEnemyTowardsPlayer(Entity enemy, Entity player)
        {
            var path = _pathFinderSystem.FindPath(
                enemy.Get<TileComponent>().Coord,
                player.Get<TileComponent>().Coord,
                enemy.Get<UnitComponent>().MoveRange
            );

            if (path.Count > 0)
                await _movementSystem.MoveUnitTo(enemy, path);
        }

        private bool IsPlayerInAttackRange(Entity enemy, Entity player)
        {
            var attackableTiles = _entityManager.GetTilesInRange(
                enemy.Get<TileComponent>().Coord,
                enemy.Get<UnitComponent>().AttackRange
            );

            return attackableTiles.Contains(player);
        }

    }
}
