using Godot;

namespace Game
{
    public partial class GameManager : Node3D
    {
        private Systems _systems;

        public override void _Ready()
        {
            _systems = new Systems(this);

            _systems.Register<RenderSystem>();
            _systems.Register<PathFinderSystem>();
            _systems.Register<TurnSystem>();
            _systems.Register<MovementSystem>();
            _systems.Register<UISystem>();
            _systems.Register<TileHighlightSystem>();
            _systems.Register<DebugSystem>();
            _systems.Register<CombatSystem>();

            _systems.GetEntityManager().CreateGrid(5);
            _systems.GetEntityManager().SpawnPlayer();
            _systems.GetEntityManager().SpawnGrunt();
            _systems.GetEntityManager().SpawnGrunt();
            _systems.GetEntityManager().SpawnGrunt();

            _systems.Initialize();
        }
    }
}
