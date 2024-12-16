using System;
using System.Collections.Generic;

namespace Game
{
    public class Entity(int id)
    {
        public int Id { get; } = id;
        private readonly Dictionary<Type, object> _components = [];

        public bool Has<T>() => _components.ContainsKey(typeof(T));

        public void Add<T>(T component)
        {
            _components[typeof(T)] = component;
            Events.Instance.OnComponentChanged(Id, typeof(T), component);
        }

        public T Update<T>(T newComponent)
        {
            var result = (T)(_components[typeof(T)] = newComponent);
            Events.Instance.OnComponentChanged(Id, typeof(T), newComponent);
            return result;
        }

        public void Remove<T>()
        {
            var type = typeof(T);
            if (_components.ContainsKey(type))
            {
                _components.Remove(type);
                Events.Instance.OnComponentChanged(Id, typeof(T), null);
            }
        }

        public T Get<T>()
        {
            var type = typeof(T);
            return _components.TryGetValue(type, out var component)
                ? (T)component
                : default;
        }
    }
}
