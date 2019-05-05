using Microsoft.Extensions.DependencyInjection;
using System;

namespace Frostbyte.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ServiceAttribute : Attribute
    {
        public ServiceAttribute(ServiceLifetime lifetime)
        {
            Lifetime = lifetime;
        }

        public ServiceAttribute(ServiceLifetime lifetime, params Type[] injectableTypes)
        {
            Lifetime = lifetime;
            InjectableTypes = injectableTypes;
        }

        public ServiceLifetime Lifetime { get; }
        public Type[] InjectableTypes { get; }
    }
}