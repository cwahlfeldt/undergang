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
        private AnimationSystem _animationSystem;
        private DebugSystem _debugSystem;
        private bool _playerActionInProgress = false;

        public override void _Ready()
        {
            _entityManager = new EntityManager(this);
            _hexGridSystem = new HexGridSystem(_entityManager, 5);
            _pathFinderSystem = new PathFinderSystem(_entityManager, _hexGridSystem);
            _unitSystem = new UnitSystem(_entityManager);
            _animationSystem = new AnimationSystem();
            _turnSystem = new TurnSystem(_entityManager);

            var player = _unitSystem.CreatePlayer(new Vector3I(0, 4, -4));
            _unitSystem.CreateGrunt(new Vector3I(-1, 0, 1));
            _unitSystem.CreateGrunt(new Vector3I(2, -3, 1));

            _turnSystem.OnTurnChanged += OnTurnChanged;
            _turnSystem.StartCombat();

            EventBus.Instance.TileSelect += OnTileClicked;
            _debugSystem = new DebugSystem(_entityManager);
        }

        private async void OnTileClicked(Entity tile)
        {
            if (_playerActionInProgress)
                return;
            var player = _entityManager.GetPlayer();
            var playerMoveRange = player.Get<MoveRangeComponent>().MoveRange;
            var tileCoord = tile.Get<HexCoordComponent>().HexCoord;
            var targetHex = _hexGridSystem.WorldToHex(tileCoord);
            var currentPos = player.Get<HexCoordComponent>().HexCoord;
            var path = _pathFinderSystem.FindPath(currentPos, tileCoord, playerMoveRange);

            if (path.Count > 0)
            {
                var movePos = path[playerMoveRange];
                var enemies = _entityManager.GetEnemies();

                _playerActionInProgress = true; // Set the flag to true
                await _animationSystem.MoveEntity(player, path);
                _playerActionInProgress = false;

                // Check if this move would kill the player
                foreach (var enemy in enemies)
                {
                    var enemyPos = enemy.Get<HexCoordComponent>().HexCoord;
                    bool wasAdjacent = _hexGridSystem.GetDistance(currentPos, enemyPos) == 1;
                    bool willBeAdjacent = _hexGridSystem.GetDistance(movePos, enemyPos) == 1;

                    // If we're moving adjacent to an enemy from a non-adjacent position, player dies
                    if (!wasAdjacent && willBeAdjacent)
                    {
                        _turnSystem.RemoveUnit(player);
                        _entityManager.RemoveEntity(player);
                        GameOver();
                        return;
                    }
                }

                // If we're still alive, check for enemy kills
                var killedEnemies = enemies.Where(enemy =>
                    _hexGridSystem.GetDistance(movePos, enemy.Get<HexCoordComponent>().HexCoord) == 1);

                foreach (var enemy in killedEnemies)
                {
                    _turnSystem.RemoveUnit(enemy);
                    _entityManager.RemoveEntity(enemy);
                }

                if (enemies.Count == 0)
                {
                    Victory();
                    return;
                }

                _turnSystem.EndTurn();

            }
        }

        private async void OnTurnChanged(Entity unit)
        {
            if (unit.Get<UnitTypeComponent>().UnitType == UnitType.Grunt)
            {
                await ProcessEnemyTurn(unit);
            }
        }

        public async Task ProcessEnemyTurn(Entity enemy)
        {
            var player = _entityManager.GetPlayer();
            var playerPos = player.Get<HexCoordComponent>().HexCoord;
            var currentEnemyPos = enemy.Get<HexCoordComponent>().HexCoord;
            var enemyMoveRange = enemy.Get<MoveRangeComponent>().MoveRange;

            var path = _pathFinderSystem.FindPath(currentEnemyPos, playerPos, enemyMoveRange);

            if (path.Count > 0)
            {

                await _animationSystem.MoveEntity(enemy, path);

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