using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.EventArgs
{
    public class BaseEvent
    {
        public EventType EventType { get; }

        public BaseEvent(EventType eventType)
        {
            EventType = eventType;
        }
    }
}