using Godot;
using Game.Autoload;
using Game.Systems;
using System.Threading.Tasks;
using System.Linq;

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
                var enemies = _entityManager.GetEnemies();

                // Note which enemies are in our attack range before moving
                var initialAttackableEnemies = enemies.Where(enemy =>
                    _unitSystem.IsInAttackRange(player, enemy.Get<HexCoordComponent>().Coord)).ToList();

                _playerActionInProgress = true;
                await _unitSystem.MoveUnit(player, path);

                // Kill enemies that were attackable before and are still in range
                foreach (var enemy in initialAttackableEnemies)
                {
                    if (_unitSystem.IsInAttackRange(player, enemy.Get<HexCoordComponent>().Coord))
                    {
                        _turnSystem.RemoveUnit(enemy);
                        _entityManager.RemoveEntity(enemy);
                    }
                }

                if (!_entityManager.GetEnemies().Any())
                {
                    Victory();
                    return;
                }

                _turnSystem.EndTurn();
                _playerActionInProgress = false;
            }
        }

        public async Task ProcessEnemyTurn(Entity enemy)
        {
            var player = _entityManager.GetPlayer();
            if (player == null) return;

            var playerPos = player.Get<HexCoordComponent>().Coord;

            // Check if player is in attack range at start of turn
            if (_unitSystem.IsInAttackRange(enemy, playerPos))
            {
                _turnSystem.RemoveUnit(player);
                _entityManager.RemoveEntity(player);
                GameOver();
                return;
            }

            // If can't kill player, move towards them
            var currentEnemyPos = enemy.Get<HexCoordComponent>().Coord;
            var enemyMoveRange = enemy.Get<MoveRangeComponent>().MoveRange;
            var path = _pathFinderSystem.FindPath(currentEnemyPos, playerPos, enemyMoveRange);

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