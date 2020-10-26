using System;
using System.Collections.Generic;
using Rage;
using LSPD_First_Response.Mod.API;

namespace RichsPoliceEnhancements
{
    class AmbientEvent
    {
        //private bool EventActive;
        private static Ped Player; // Assign player's character to ped 'player'
        private string EventType;
        private static List<EventPed> EventPeds = new List<EventPed>();
        private static List<EventVehicle> EventVehicles = new List<EventVehicle>();
        private static List<Blip> EventBlips = new List<Blip>();

        public AmbientEvent(string eventType, Ped player)
        {
            EventType = eventType;
            Player = player;
            AppDomain.CurrentDomain.DomainUnload += MyTerminationHandler;

            switch (eventType)
            {
                case "DrugDeal":
                    DrugDealEventFunctions.RunEventFunctions(Player, NearbyPeds(), EventPeds, EventVehicles, EventBlips);
                    break;
                case "DriveBy":
                    DriveByEventFunctions.RunEventFunctions(Player, NearbyPeds(), EventPeds, EventVehicles, EventBlips);
                    break;
                case "CarJacking":
                    CarJackingEventFunctions.RunEventFunctions(Player, NearbyPeds(), EventPeds, EventVehicles, EventBlips);
                    break;
                case "Assault":
                    AssaultEventFunctions.RunEventFunctions(Player, NearbyPeds(), EventPeds, EventVehicles, EventBlips);
                    break;
                case "RoadRage":
                    //RoadRageFunctions.RunEventFunctions(Player, NearbyPeds());
                    break;
                case "PublicIntoxication":
                    //PublicIntoxicationFunctions.RunEventFunctions(Player, NearbyPeds());
                    break;
                case "DUI":
                    //DUIFunctions.RunEventFunctions(Player, NearbyPeds());
                    break;
                case "Prostitution":
                    //ProstitutionFunctions.RunEventFunctions(Player, NearbyPeds());
                    break;
                case "Protest":
                    //ProtestFunctions.RunEventFunctions(Player, NearbyPeds());
                    break;
                case "SuspiciousCircumstances":
                    //SuspiciousFunctions.RunEventFunctions(Player, NearbyPeds());
                    break;
                case "CriminalMischief":
                    //CriminalMischiefFunctions.RunEventFunctions(Player, NearbyPeds());
                    break;
                case "OfficerAmbush":
                    //OfficerAmbushFunctions.RunEventFunctions(Player, NearbyPeds());
                    break;
                case "CitizenAssist":
                    //CitizenAssistFunctions.RunEventFunctions(Player, NearbyPeds());
                    break;
                case "MentalHealth":
                    //MentalHealthFunctions.RunEventFunctions(Player, NearbyPeds());
                    break;
            }
            EventCleanup();
            Game.LogTrivial($"[Rich Ambiance] {eventType} is finished.");
            AppDomain.CurrentDomain.DomainUnload -= MyTerminationHandler;
        }

        private List<Ped> NearbyPeds()
        {
            List<Ped> pedList = new List<Ped>();
            foreach (Ped ped in World.GetAllPeds())
            {
                if (PedIsRelevant(ped))
                {
                    pedList.Add(ped);
                    //Game.LogTrivial($"[Rich Ambiance] ped added to the list");
                }
            }
            Game.LogTrivial("[Rich Ambiance] List complete.  Total peds: " + pedList.Count);
            return pedList;
        }

        private static bool PedIsRelevant(Ped ped)
        {
            // If the ped exists && if the ped is valid && if the ped is within 50m of player && if the ped is NOT the player && if ped is NOT injured && if ped model is not an animal && if ped is on foot
            if (PedIsValid(ped) && ped.IsAlive && ped.Position.DistanceTo(Game.LocalPlayer.Character.Position) < 100f && !ped.IsPlayer && !ped.IsInjured && !ped.Model.Name.Contains("A_C"))// && ped.IsOnFoot)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool PedIsValid(Ped ped)
        {
            if(ped.Exists() && ped.IsValid())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool PrematureEndCheck(List<EventPed> eventPeds)
        {
            foreach (EventPed EP in eventPeds)
            {
                if (!PedIsValid(EP.Ped))
                {
                    Game.LogTrivial($"[Rich Ambiance] An event ped is no longer valid");
                    return true;
                }
                if (PedIsValid(EP.Ped) && EP.Ped.DistanceTo(Player) > 150f)
                {
                    Game.LogTrivial($"An event ped is too far away");
                    return true;
                }
                if (!EP.Ped.IsAlive)
                {
                    Game.LogTrivial($"[Rich Ambiance] An event ped is dead.");
                    return true;
                }
            }
            return false;
        }

        public static bool PrematureEndCheck(List<EventPed> eventPeds, List<EventVehicle> eventVehicles)
        {
            foreach(EventPed EP in eventPeds)
            {
                if (!EP.Ped.Exists() || !EP.Ped.IsValid())
                {
                    Game.LogTrivial($"An event ped is no longer valid");
                    return true;
                }
                if(PedIsValid(EP.Ped) && EP.Ped.DistanceTo(Player) > 150f)
                {
                    Game.LogTrivial($"An event ped is too far away");
                    return true;
                }
                if (!EP.Ped.IsAlive)
                {
                    Game.LogTrivial($"An event ped is dead.");
                    return true;
                }
            }
            return false;
        }

        // Implement system to initiate pursuit?  LSPDFR default function doesn't appear to recognize these events (assault, driveby) as crimes
        private static void EventCleanup()
        {
            // Clean up EventBlips
            foreach (Blip blip in EventBlips)
            {
                if(blip.Exists() && blip.IsValid())
                {
                    blip.Delete();
                }
            }
            EventBlips.Clear();

            // Clean up EventPeds
            foreach (EventPed ped in EventPeds)
            {
                if (ped.Ped.Exists() && ped.Ped.IsValid())
                {
                    ped.Ped.IsPersistent = false;
                    ped.Ped.Tasks.Clear();
                    ped.Ped.Dismiss();
                }
            }
            EventPeds.Clear();

            // Clean up EventVehicles
            foreach (EventVehicle vehicle in EventVehicles)
            {
                if (vehicle.Vehicle.Exists() && vehicle.Vehicle.IsValid())
                {
                    vehicle.Vehicle.IsPersistent = false;
                }
            }
            EventVehicles.Clear();
        }

        private static void MyTerminationHandler(object sender, EventArgs e)
        {
            // Clean up EventBlips
            foreach (Blip blip in EventBlips)
            {
                blip.Delete();
            }
            EventBlips.Clear();

            // Clean up EventPeds
            foreach (EventPed ped in EventPeds)
            {
                if (ped.Ped.Exists() && ped.Ped.IsValid())
                {
                    ped.Ped.IsPersistent = false;
                    ped.Ped.Tasks.Clear();
                    ped.Ped.Dismiss();
                }
            }
            EventPeds.Clear();

            // Clean up EventVehicles
            foreach (EventVehicle vehicle in EventVehicles)
            {
                if (vehicle.Vehicle.Exists() && vehicle.Vehicle.IsValid())
                {
                    vehicle.Vehicle.IsPersistent = false;
                }
            }
            EventVehicles.Clear();

            Game.LogTrivial($"Rich Ambiance has been terminated.");
        }
    }
}
