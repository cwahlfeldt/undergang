namespace Game
{
    public class SystemDependencies(Entities entities, Events events, Tweener Tweener, Systems systems)
    {
        public Entities Entities { get; } = entities;
        public Tweener Tweener { get; } = Tweener;
        public Events Events { get; } = events;
        public Systems Systems { get; } = systems;
    }
}
