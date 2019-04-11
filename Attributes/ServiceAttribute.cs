using Microsoft.Extensions.DependencyInjection;
using System;

namespace Frostbyte.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ServiceAttribute : Attribute
    {
        public ServiceLifetime Lifetime { get; set; }

        public Type[] InjectableTypes { get; set; }

        public ServiceAttribute(ServiceLifetime lifetime)
        {
            Lifetime = lifetime;
        }

        public ServiceAttribute(ServiceLifetime lifetime, params Type[] injectableTypes)
        {
            Lifetime = lifetime;
            InjectableTypes = injectableTypes;
        }
    }
}