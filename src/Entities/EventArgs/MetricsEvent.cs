using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Infos;

namespace Frostbyte.Entities.EventArgs
{
    public sealed class MetricsEvent : BaseEvent
    {
        public MetricsInfo Metrics { get; }

        public MetricsEvent(MetricsInfo metrics) : base(EventType.Metrics)
        {
            Metrics = metrics;
        }
    }
}