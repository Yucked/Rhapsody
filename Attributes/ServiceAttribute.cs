using Microsoft.Extensions.DependencyInjection;
using System;

namespace Frostbyte.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ServiceAttribute : Attribute
    {
        public ServiceLifetime Lifetime { get; }
        public Type BaseType { get; }

        public ServiceAttribute(ServiceLifetime lifetime)
        {
            Lifetime = lifetime;
        }

        public ServiceAttribute(ServiceLifetime lifetime, Type baseType)
        {
            Lifetime = lifetime;
            BaseType = baseType;
        }
    }
}