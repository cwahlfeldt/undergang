using Game.Components;
using Godot;

namespace Game
{
    public partial class GameManager : Node3D
    {
        private Systems _systems;

        public override void _Ready()
        {
            _systems = new Systems(this);

            var entityManager = _systems.GetEntityManager();
            entityManager.SpawnGrid(5);
            entityManager.CreatePlayer();
            entityManager.CreateEnemy(UnitType.Grunt);
            entityManager.CreateEnemy(UnitType.Grunt);
            entityManager.CreateEnemy(UnitType.Grunt);

            _systems.Register<RenderSystem>();

            // _systems.Register<PathFinderSystem>();
            // _systems.Register<TurnSystem>();
            // _systems.Register<MovementSystem>();
            // _systems.Register<UISystem>();
            // _systems.Register<TileHighlightSystem>();
            // _systems.Register<DebugSystem>();
            // _systems.Register<CombatSystem>();



            _systems.Initialize();
        }
    }
}
