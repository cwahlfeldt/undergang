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
        private EntityManager _entityManager;
        private EntityFactory _entityFactory;
        private RenderSystem _renderSystem;
        private PathFinderSystem _pathFinderSystem;
        private TurnSystem _turnSystem;
        private MovementSystem _movementSystem;
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

            _renderSystem.RenderBoard();
            _pathFinderSystem.SetupPathfinding();
            _turnSystem.StartCombat();
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

            var (coord, player) = _entityManager.GetPlayerP();
            if (!_turnSystem.IsUnitTurn(player))
                return;

            var path = _pathFinderSystem.FindPath(
                coord,
                entity.Get<TileComponent>().Coord,
                player.Get<UnitComponent>().MoveRange
            );

            if (path.Count > 0)
            {
                _playerActionInProgress = true;
                await ProcessPlayerTurn(player, path);
                _playerActionInProgress = false;
                _turnSystem.EndTurn();
            }
        }

        public void AttackUnit(Entity attacker, Entity target)
        {
            var attackerComponent = attacker.Get<UnitComponent>();
            var targetComponent = target.Get<UnitComponent>();
            var damageDone = targetComponent.Health - attackerComponent.Damage;

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
            var attackableTiles = _entityManager.GetAllInRange(playerCoord, 1);
            foreach (var enemy in initialEnemies)
            {
                if (attackableTiles.Contains(enemy))
                {
                    AttackUnit(_entityManager.GetPlayer(), enemy);
                }
            }
        }

        private void HandleEnemyCounterattacks(Vector3I playerCoord)
        {
            var enemyCoords = _entityManager.GetEnemies().Where(tile =>
                {
                    var unit = tile.Get<UnitComponent>();
                    return unit != null && unit.Type == UnitType.Enemy;
                })
                .Select(tile => tile.Get<TileComponent>().Coord)
                .ToList();

            foreach (var enemyCoord in enemyCoords)
            {
                var enemyRange = _entityManager.GetAllInRange(enemyCoord, 1);
                var playerTile = _entityManager.GetAt(playerCoord);

                if (enemyRange.Contains(playerTile))
                {
                    AttackUnit(_entityManager.GetAtCoord(enemyCoord), playerTile);
                }
            }
        }
        private List<Entity> GetAttackableEnemies(Vector3I position)
        {
            return _entityManager.GetAllInRange(position, 1)
                .Where(entity => entity.Has<UnitComponent>() &&
                    entity.Get<UnitComponent>().Type == UnitType.Grunt)
                .ToList();
        }

        private async void OnTurnChanged(Entity entity)
        {
            if (!entity.Has<UnitComponent>()) return;
            GD.Print($"turn is {entity.Get<UnitComponent>().Name}");
            var (coord, player) = _entityManager.GetPlayerP();
            if (!_turnSystem.IsUnitTurn(player))
            {
                await ProcessEnemyTurn(entity);
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

        private async Task MoveEnemyTowardsPlayer(Entity enemy, Entity player)
        {
            var path = _pathFinderSystem.FindPath(
                enemy.Get<TileComponent>().Coord,
                player.Get<TileComponent>().Coord,
                enemy.Get<UnitComponent>().MoveRange
            );

            if (path.Count > 0)
            {
                await _movementSystem.MoveUnitTo(enemy, path);
            }
        }

        private bool IsPlayerInAttackRange(Entity enemy, Entity player)
        {
            var attackableTiles = _entityManager.GetAllInRange(
                enemy.Get<TileComponent>().Coord,
                enemy.Get<UnitComponent>().AttackRange
            );

            return attackableTiles.Contains(player);
        }

    }
}

