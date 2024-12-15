namespace Game
{
    public class SystemDependencies(Entities entities, PathFinder pathFinder, Events events, Tweener Tweener, Systems systems)
    {
        public Entities Entities { get; } = entities;
        public PathFinder PathFinder { get; } = pathFinder;
        public Tweener Tweener { get; } = Tweener;
        public Events Events { get; } = events;
        public Systems Systems { get; } = systems;
    }
}
