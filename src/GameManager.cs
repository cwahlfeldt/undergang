using Godot;
using Game.Autoload;
using Game.Systems;

namespace Game
{
    public partial class GameManager : Node3D
    {
        private EntityManager _entityManager;
        private HexGridSystem _hexGridSystem;
        private PathFinderSystem _pathFinderSystem;
        private UnitSystem _unitSystem;
        private TurnSystem _turnSystem;
        // private CombatSystem _combatSystem;
        private AnimationSystem _animationSystem;
        private DebugSystem _debugSystem;

        public override void _Ready()
        {
            _entityManager = new EntityManager(this);
            _hexGridSystem = new HexGridSystem(_entityManager, 5);
            _pathFinderSystem = new PathFinderSystem(_entityManager, _hexGridSystem);
            _unitSystem = new UnitSystem(_entityManager);
            // _combatSystem = new CombatSystem(_entityManager, _hexGridSystem);
            _animationSystem = new AnimationSystem();
            _turnSystem = new TurnSystem(_entityManager);

            var player = _unitSystem.CreatePlayer(new Vector3I(0, 4, -4));
            _unitSystem.CreateGrunt(new Vector3I(-2, 1, 3));
            _unitSystem.CreateGrunt(new Vector3I(2, -3, 1));

            _turnSystem.OnTurnChanged += OnTurnChanged;
            _turnSystem.StartCombat();

            EventBus.Instance.TileSelect += OnTileClicked;
            _debugSystem = new DebugSystem(_entityManager);
        }

        private async void OnTileClicked(Entity tile)
        {
            var player = _entityManager.GetPlayer();
            var playerMoveRange = player.Get<MoveRangeComponent>().MoveRange;
            var tileCoord = tile.Get<HexCoordComponent>().HexCoord;
            // GD.Print(tileCoord);

            var targetHex = _hexGridSystem.WorldToHex(tileCoord);
            var currentPos = player.Get<HexCoordComponent>().HexCoord;
            var path = _pathFinderSystem.FindPath(currentPos, tileCoord, playerMoveRange);
            // GD.Print(path.ToString());

            if (path.Count > 0)
            {
                // Move player
                await _animationSystem.MoveEntity(player, path);

                // Check if player would die from this move
                // if (_combatSystem.WouldDieFromMove(player, currentPos, targetHex))
                // {
                //     _turnSystem.RemoveUnit(player);
                //     _entityManager.RemoveEntity(player);
                //     GameOver();
                //     return;
                // }

                // Check for killed enemies
                // var killedEnemies = _combatSystem.GetKillableEnemies(player, currentPos, targetHex);
                // foreach (var enemy in killedEnemies)
                // {
                //     _turnSystem.RemoveUnit(enemy);
                //     _entityManager.RemoveEntity(enemy);
                // }

                if (_entityManager.GetEnemies().Count == 0)
                {
                    Victory();
                    return;
                }

                _turnSystem.EndTurn();
            }
        }

        private void OnTurnChanged(Entity unit)
        {
            if (unit.Get<UnitTypeComponent>().UnitType == UnitType.Grunt)
            {
                ProcessEnemyTurn(unit);
            }
        }

        public async void ProcessEnemyTurn(Entity enemy)
        {
            var player = _entityManager.GetPlayer();
            var playerPos = player.Get<HexCoordComponent>().HexCoord;
            var currentEnemyPos = enemy.Get<HexCoordComponent>().HexCoord;
            var enemyMoveRange = enemy.Get<MoveRangeComponent>().MoveRange;

            var path = _pathFinderSystem.FindPath(currentEnemyPos, playerPos, enemyMoveRange);

            if (path.Count > 0)
            {

                await _animationSystem.MoveEntity(enemy, path);
                _turnSystem.EndTurn();
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