using Rage;

namespace RichsPoliceEnhancements
{
    class EventVehicle
    {
        public Vehicle Vehicle { get; private set; }
        public string EventType { get; private set; }

        // Default
        public EventVehicle()
        {

        }

        public EventVehicle(string eventType, Vehicle vehicle)
        {
            EventType = eventType;
            Vehicle = vehicle;
        }
    }
}
