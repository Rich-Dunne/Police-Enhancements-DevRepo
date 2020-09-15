using Rage;

namespace RichsPoliceEnhancements
{
    public class EventPed
    {
        public string EventType { get; private set; }
        public Ped Ped { get; private set; }

        // Default
        public EventPed()
        {

        }

        public EventPed(string eventType, Ped ped)
        {
            EventType = eventType;
            Ped = ped;
        }
    }
}
