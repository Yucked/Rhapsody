using System;

namespace Frostbyte.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RegisterServiceAttribute : Attribute
    {
        public Type BaseType { get; }

        public RegisterServiceAttribute(Type baseType)
        {
            BaseType = baseType;
        }

        public RegisterServiceAttribute() { }
    }
}