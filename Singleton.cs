using Frostbyte.Extensions;
using System;
using System.Collections.Generic;

namespace Frostbyte
{
    public sealed class Singleton
    {
        private static readonly Dictionary<string, object> _instances
            = new Dictionary<string, object>();

        public static T Of<T>() where T : class
        {
            var type = typeof(T);
            if (_instances.TryGetValue(type.Name, out var instance))
                return instance.TryCast<T>();

            instance = _instances[type.Name];
            return instance.TryCast<T>();
        }

        public static T Of<T>(Type type)
        {
            if (_instances.TryGetValue(type.Name, out var instance))
                return instance.TryCast<T>();

            instance = _instances[type.Name];
            return instance.TryCast<T>();
        }

        public static T Of<T>(object obj)
        {
            var type = obj.GetType();
            if (_instances.TryGetValue(type.Name, out var instance))
                return instance.TryCast<T>();

            instance = _instances[type.Name];
            return instance.TryCast<T>();
        }

        public static void Add<T>()
        {
            var type = typeof(T);
            if (_instances.ContainsKey(type.Name))
                return;

            var instance = Activator.CreateInstance<T>();
            _instances.TryAdd(type.Name, instance);
        }

        public static void Add(Type type)
        {
            if (_instances.ContainsKey(type.Name))
                return;

            var obj = Activator.CreateInstance(type);
            _instances.TryAdd(type.Name, obj);
        }

        public static void Add<T>(object obj)
        {
            var type = obj.GetType();
            if (_instances.ContainsKey(type.Name))
                return;

            obj ??= Activator.CreateInstance<T>();
            _instances.TryAdd(type.Name, obj);
        }

        public static void Update<T>(object obj)
        {
            var type = obj.GetType();
            if (!_instances.TryGetValue(type.Name, out var value))
                return;

            value = obj;
        }
    }
}