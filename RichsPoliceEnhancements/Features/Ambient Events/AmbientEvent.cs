using System.Collections.Generic;
using Rage;
using System.Linq;
using System;
using RichsPoliceEnhancements.Features;

namespace RichsPoliceEnhancements
{
    internal enum EventType
    {
        None = 0,
        DrugDeal = 1,
        DriveBy = 2,
        CarJacking = 3,
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
        MentalHealth = 14,
        TrafficStopAssist = 15,
        OpenCarry = 16,
        CarVsAnimal = 17
    }

    internal class AmbientEvent
    {
        internal EventType EventType { get; private set; }
        internal List<EventPed> EventPeds { get; private set; } = new List<EventPed>();
        internal List<EventVehicle> EventVehicles { get; private set; } = new List<EventVehicle>();
        internal List<Blip> EventBlips { get; private set; } = new List<Blip>();

        internal AmbientEvent(EventType eventType)
        {
            AppDomain.CurrentDomain.DomainUnload += TerminationHandler;
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
                case EventType.DriveBy:
                    DriveByEventFunctions.BeginEvent(this);
                    break;
                case EventType.CarJacking:
                    CarJackingEventFunctions.BeginEvent(this);
                    break;
                case EventType.Assault:
                    AssaultEventFunctions.BeginEvent(this);
                    break;
            }
        }

        internal void Cleanup()
        {
            // Clean up EventBlips
            foreach (Blip blip in EventBlips.Where(b => b))
            {
                blip.Delete();
            }
            EventBlips.Clear();

            // Clean up EventPeds
            foreach (EventPed EP in EventPeds.Where(x => x.Ped))
            {
                foreach (Blip blip in EP.Ped.GetAttachedBlips().Where(b => b))
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

            AmbientEvents.SetEventActiveFalse();
            AppDomain.CurrentDomain.DomainUnload -= TerminationHandler;
        }

        private void TerminationHandler(object sender, EventArgs e)
        {
            Cleanup();
            Game.LogTrivial("[RPE Ambient Event]: Plugin terminated.");
        }
    }
}
