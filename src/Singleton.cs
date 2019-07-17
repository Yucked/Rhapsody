using System;
using System.Collections.Generic;

namespace Frostbyte
{
    public readonly struct Singleton
    {
        private static readonly Dictionary<string, object>
            Instances = new Dictionary<string, object>();

        public static T Of<T>()
        {
            var type = typeof(T);
            return Instances.TryGetValue(type.Name, out var obj)
                ? (T) obj
                : default;
        }

        public static T Of<T>(Type type)
        {
            return Instances.TryGetValue(type.Name, out var obj)
                ? (T) obj
                : default;
        }

        public static void Add<T>()
        {
            var type = typeof(T);

            if (Instances.ContainsKey(type.Name))
                return;

            var instance = Activator.CreateInstance<T>();
            Instances.TryAdd(type.Name, instance);
        }

        public static void Add<T>(T value)
        {
            var type = typeof(T);

            if (Instances.ContainsKey(type.Name))
                return;

            Instances.TryAdd(type.Name, value);
        }

        public static void Add(Type type)
        {
            if (Instances.ContainsKey(type.Name))
                return;

            Instances.TryAdd(type.Name, type);
        }
    }
}