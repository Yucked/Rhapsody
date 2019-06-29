using System;
using System.Linq;

namespace Frostbyte.Extensions
{
    public sealed class EnumExtension<T> where T : struct
    {
        public static bool Ensure(T mainValue, params T[] values)
        {
            return values.Any(x => x.Equals(mainValue));
        }
    }
}