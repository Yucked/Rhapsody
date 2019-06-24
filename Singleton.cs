using Frostbyte.Extensions;
using System;
using System.Collections.Generic;

namespace Frostbyte
{
    public sealed class Singleton
    {
        private static readonly Dictionary<Type, object> _instances
            = new Dictionary<Type, object>();

        public static T Of<T>() where T : class
        {
            if (_instances.TryGetValue(typeof(T), out var instance))
                return instance.TryCast<T>();

            Add<T>();
            instance = _instances[typeof(T)];

            return instance.TryCast<T>();
        }

        public static T Of<T>(object obj)
        {
            if (_instances.TryGetValue(obj.GetType(), out var instance))
                return instance.TryCast<T>();

            Add<T>(obj);
            instance = _instances[typeof(T)];

            return instance.TryCast<T>();
        }

        public static void Add<T>()
        {
            if (_instances.ContainsKey(typeof(T)))
                return;

            var instance = Activator.CreateInstance<T>();
            _instances.TryAdd(typeof(T), instance);
        }

        public static void Add(Type type)
        {
            if (_instances.ContainsKey(type))
                return;

            var obj = Activator.CreateInstance(type);
            _instances.TryAdd(type, obj);
        }

        public static void Add<T>(object obj)
        {
            if (_instances.ContainsKey(typeof(T)))
                return;

            if (obj is null)
                obj = Activator.CreateInstance<T>();

            _instances.TryAdd(typeof(T), obj);
        }

        public static void Update<T>(object obj)
        {
            if (!_instances.TryGetValue(typeof(T), out var value))
                return;

            value = obj;
        }
    }
}