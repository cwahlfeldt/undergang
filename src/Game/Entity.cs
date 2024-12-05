using System;
using System.Collections.Generic;

namespace Game
{
    public class Entity(int id)
    {
        public int Id { get; } = id;
        private readonly Dictionary<Type, object> _components = [];

        public T Get<T>() => (T)_components[typeof(T)];
        public bool Has<T>() => _components.ContainsKey(typeof(T));
        public void Add<T>(T component) => _components[typeof(T)] = component;
        public void Update<T>(T newComponent) => _components[typeof(T)] = newComponent;
    }
}