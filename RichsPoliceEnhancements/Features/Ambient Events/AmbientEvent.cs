using System.Collections.Generic;
using Rage;
using System.Linq;

namespace RichsPoliceEnhancements
{
    internal enum EventType
    {
        DrugDeal = 0,
        DriveBy = 1,
        CarJacking = 2,
        Assault = 4,
        RoadRage = 5,
        PublicIntoxication = 6,
        DUI = 7,
        Prostitution = 8,
        Protest = 9,
        SuspiciousCircumstances = 10,
        CriminalMischief = 11,
        OfficerAmbush = 12,
        CitizenAssist = 13,
        MentalHealth = 14
    }

    internal class AmbientEvent
    {
        internal EventType EventType { get; private set; }
        internal List<EventPed> EventPeds { get; private set; } = new List<EventPed>();
        internal List<EventVehicle> EventVehicles { get; private set; } = new List<EventVehicle>();
        internal List<Blip> EventBlips { get; private set; } = new List<Blip>();

        internal AmbientEvent(EventType eventType)
        {
            EventType = eventType;
            RunEventFunctions(eventType);
        }

        private void RunEventFunctions(EventType eventType)
        {
            switch (eventType)
            {
                case EventType.DrugDeal:
                    DrugDealEventFunctions.BeginEvent(this);
                    break;
            }
        }

        internal void Cleanup()
        {
            // Clean up EventBlips
            foreach (Blip blip in EventBlips)
            {
                blip.Delete();
            }
            EventBlips.Clear();

            // Clean up EventPeds
            foreach (EventPed EP in EventPeds.Where(x => x.Ped))
            {
                foreach (Blip blip in EP.Ped.GetAttachedBlips())
                {
                    blip.Delete();
                }
                EP.Ped.BlockPermanentEvents = false;
                EP.Ped.Dismiss();
            }
            EventPeds.Clear();

            // Clean up EventVehicles
            //foreach (EventVehicle vehicle in EventVehicles.Where(x => x.))
            
            //    vehicle.IsPersistent = false;
            //}
            //EventVehicles.Clear();
        }
    }
}
