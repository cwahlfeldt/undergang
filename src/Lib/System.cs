using System.Threading.Tasks;

namespace Game
{
    public abstract class System : ISystem
    {
        protected Entities Entities { get; private set; }
        protected Events Events { get; private set; }
        protected Tweener Tweener { get; private set; }
        protected Systems Systems { get; private set; }

        internal void InjectDependencies(SystemDependencies dependencies)
        {
            Entities = dependencies.Entities;
            Events = dependencies.Events;
            Tweener = dependencies.Tweener;
            Systems = dependencies.Systems;
        }

        // Get other systems when needed
        protected T GetSystem<T>() where T : ISystem
        {
            return Systems.Get<T>();
        }

        public virtual void Initialize() { }
        public virtual async Task Update(Entity entity) { }
        public virtual async Task Process(float delta) { }
        public virtual void Cleanup() { }
    }
}
