using Frostbyte.Entities.Enums;
using System;

namespace Frostbyte.Entities
{
    public sealed class RatelimitBucket
    {
        public DateTimeOffset EntryDate { get; set; }
        public RestrictionType RestrictionType { get; set; }
    }
}