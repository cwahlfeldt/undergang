using System;
using System.Collections.Generic;
using Godot;

namespace Game
{
    public partial class Systems
    {
        private readonly Dictionary<Type, ISystem> _systems = [];
        private readonly SystemDependencies _dependencies;
        private bool _initialized;

        public Systems(Node3D rootNode)
        {
            var entities = new Entities(rootNode);

            _dependencies = new SystemDependencies(
                entities,
                Events.Instance,
                Tweener.Instance,
                this
            );

            _systems[typeof(Entities)] = entities;
            _systems[typeof(Events)] = Events.Instance;
            _systems[typeof(Tweener)] = Tweener.Instance;
        }

        public T Register<T>() where T : System, new()
        {
            var system = new T();
            system.InjectDependencies(_dependencies);
            _systems[typeof(T)] = system;
            return system;
        }

        // private void Register(Entities entityManager)
        // {
        //     _systems[typeof(Entities)] = entityManager;
        // }
        // private void Register(Events events)
        // {
        //     _systems[typeof(Events)] = events;
        // }
        // private void Register(Tweener entityManager)
        // {
        //     _systems[typeof(Tweener)] = entityManager;
        // }
        // private void Register(Entities entityManager)
        // {
        //     _systems[typeof(Entities)] = entityManager;
        // }

        public Entities GetEntityManager()
        {
            return _dependencies.Entities;
        }

        public T Get<T>() where T : ISystem
        {
            if (!_systems.TryGetValue(typeof(T), out var system))
                throw new InvalidOperationException($"System of type {typeof(T).Name} not found.");

            return (T)system;
        }

        public void Initialize()
        {
            if (_initialized) return;

            foreach (var system in _systems.Values)
            {
                system.Initialize();
            }
            _initialized = true;
        }

        public void Update(Entity entity)
        {
            foreach (var system in _systems.Values)
            {
                if (system is System baseSystem)
                {
                    baseSystem.Update(entity);
                }
            }
        }

        public void Cleanup()
        {
            foreach (var system in _systems.Values)
            {
                if (system is System baseSystem)
                {
                    baseSystem.Cleanup();
                }
            }
            _systems.Clear();
            _initialized = false;
        }


        public IEnumerable<ISystem> GetAllSystems()
        {
            return _systems.Values;
        }

        public void RemoveSystem<T>() where T : ISystem
        {
            var type = typeof(T);
            if (_systems.TryGetValue(type, out var system))
            {
                system.Cleanup();
                _systems.Remove(type);
            }
        }
    }
}
