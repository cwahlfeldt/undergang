using Game.Components;
using Godot;

namespace Game
{
    public partial class GameManager : Node3D
    {
        private Systems _systems;

        public override void _Ready()
        {
            Events.Instance.TurnChanged += OnTurnChanged;

            _systems = new Systems(this);

            var entityManager = _systems.GetEntityManager();

            _systems.RegisterConcurrent<ComponentDebugSystem>();
            _systems.RegisterConcurrent<DebugSystem>();
            _systems.RegisterConcurrent<TileHighlightSystem>();

            _systems.Register<RenderSystem>();
            _systems.Register<TurnSystem>();
            _systems.Register<PlayerSystem>();
            _systems.Register<EnemySystem>();
            _systems.Register<RangeSystem>();
            _systems.Register<MovementSystem>();

            entityManager.CreateGrid(5);
            entityManager.CreatePlayer();
            entityManager.CreateEnemy(UnitType.Grunt);
            entityManager.CreateEnemy(UnitType.Grunt);
            entityManager.CreateEnemy(UnitType.Grunt);

            _systems.Initialize();
        }

        private async void OnTurnChanged(Entity entity)
        {
            GD.Print($"Turn changed to {entity.Get<Name>()}");
            await _systems.Update();
        }
    }
}
