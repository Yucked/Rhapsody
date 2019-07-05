using System;
using System.Collections.Generic;
using Frostbyte.Extensions;

namespace Frostbyte
{
    public sealed class Singleton
    {
        private static readonly Dictionary<string, object> Instances
            = new Dictionary<string, object>();

        public static T Of<T>() where T : class
        {
            var type = typeof(T);
            if (Instances.TryGetValue(type.Name, out var instance))
                return instance.TryCast<T>();

            instance = Instances[type.Name];
            return instance.TryCast<T>();
        }

        public static T Of<T>(Type type)
        {
            if (Instances.TryGetValue(type.Name, out var instance))
                return instance.TryCast<T>();

            instance = Instances[type.Name];
            return instance.TryCast<T>();
        }

        public static T Of<T>(object obj)
        {
            var type = obj.GetType();
            if (Instances.TryGetValue(type.Name, out var instance))
                return instance.TryCast<T>();

            instance = Instances[type.Name];
            return instance.TryCast<T>();
        }

        public static void Add<T>()
        {
            var type = typeof(T);
            if (Instances.ContainsKey(type.Name))
                return;

            var instance = Activator.CreateInstance<T>();
            Instances.TryAdd(type.Name, instance);
        }

        public static void Add(Type type)
        {
            if (Instances.ContainsKey(type.Name))
                return;

            var obj = Activator.CreateInstance(type);
            Instances.TryAdd(type.Name, obj);
        }

        public static void Add<T>(object obj)
        {
            var type = obj.GetType();
            if (Instances.ContainsKey(type.Name))
                return;

            obj ??= Activator.CreateInstance<T>();
            Instances.TryAdd(type.Name, obj);
        }

        public static void Update<T>(object obj)
        {
            var type = obj.GetType();
            if (!Instances.TryGetValue(type.Name, out var value))
                return;

            value = obj;
        }
    }
}