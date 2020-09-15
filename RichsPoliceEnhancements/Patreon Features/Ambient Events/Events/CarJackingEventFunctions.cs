using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;

namespace RichsPoliceEnhancements
{
    class CarJackingEventFunctions
    {
        public static void RunEventFunctions(Ped player, List<Ped> nearbyPeds, List<EventPed> eventPeds, List<EventVehicle> eventVehicles, List<Blip> eventBlips)
        {
            Game.LogTrivial($"[Rich Ambiance] Starting CarJacking event.");
            if (FindJacker(nearbyPeds, eventPeds) != null && FindTarget(nearbyPeds, eventPeds[0].Ped, eventVehicles) != null)
            {
                CarJackingInteraction(eventPeds, eventVehicles, eventBlips);
            }
            else
            {
                Game.LogTrivial($"[Rich Ambiance] Driver or target returned null.  Ending event.");
            }
        }

        // Having trouble finding a target vehicle near most peds.  Figure out additional checks
        private static Ped FindJacker(List<Ped> pedList, List<EventPed> eventPeds)
        {
            Ped carJacker;
            foreach (Ped p in pedList)
            {
                if (p.Exists() && p.IsValid() && p.IsAlive && p.IsOnFoot)
                {
                    //Game.LogTrivial($"Target relationship group: {p.RelationshipGroup.Name}");
                    // Ped Settings
                    carJacker = p;
                    carJacker.IsPersistent = true;
                    carJacker.BlockPermanentEvents = true;

                    // Blip Settings
                    //Blip blip = carJacker.AttachBlip();
                    //blip.Color = Color.Red;
                    //AmbientEvent.EventBlips.Add(blip);


                    eventPeds.Add(new EventPed("CarJacking", carJacker));
                    Game.LogTrivial($"[Rich Ambiance] Car jacker found.");
                    return carJacker;
                }
            }
            Game.LogTrivial($"Could not find car jacker.");
            return null;
        }

        private static Vehicle FindTarget(List<Ped> pedList, Ped carJacker, List<EventVehicle> eventVehicles) // Target can be ped or empty, locked car
        {
            foreach (Vehicle v in carJacker.GetNearbyVehicles(16))
            {
                if (v.Exists() && v.IsValid() && v.DistanceTo(carJacker) <= 20f && (v.IsStoppedAtTrafficLights || v.Speed == 0))
                {
                    // Vehicle settings
                    v.IsPersistent = true;

                    // Blip Settings
                    //Blip blip = v.AttachBlip();
                    //blip.Sprite = BlipSprite.GangVehicle;
                    //blip.Color = Color.LightBlue;
                    //AmbientEvent.EventBlips.Add(blip);

                    eventVehicles.Add(new EventVehicle("CarJacking", v));
                    Game.LogTrivial($"[Rich Ambiance] Target vehicle found.");
                    return v;
                }
            }

            Game.LogTrivial($"[Rich Ambiance] Could not find target vehicle.");
            return null;
        }

        private static void CarJackingInteraction(List<EventPed> eventPeds, List<EventVehicle> eventVehicles, List<Blip> eventBlips)
        {
            bool endEvent = false;
            Ped carJacker = eventPeds[0].Ped;
            Vehicle targetVehicle = eventVehicles[0].Vehicle;
            carJacker.Tasks.Clear();
            Game.LogTrivial($"Car jacker is entering vehicle.");
            if(targetVehicle.Exists() && targetVehicle.IsValid())
            {
                carJacker.Tasks.EnterVehicle(targetVehicle, -1, -1, 5f, EnterVehicleFlags.AllowJacking).WaitForCompletion();
            }
            else
            {
                endEvent = true;
                Game.LogTrivial($"Target vehicle is no longer valid.");
            }

            while(!carJacker.IsInVehicle(targetVehicle, false) && !endEvent)
            {
                if (AmbientEvent.PrematureEndCheck(eventPeds, eventVehicles))
                {
                    break;
                }
                GameFiber.Yield();
            }

            if(carJacker.IsInVehicle(targetVehicle, false) && !endEvent)
            {
                Game.LogTrivial($"Car jacker is in vehicle and driving away.");

                // Blip Settings
                if (Settings.EventBlips)
                {
                    Blip blip = carJacker.AttachBlip();
                    blip.Sprite = BlipSprite.StrangersandFreaks;
                    blip.Color = Color.Red;
                    eventBlips.Add(blip);
                }

                carJacker.Tasks.CruiseWithVehicle(30f, VehicleDrivingFlags.Emergency);
                carJacker.Dismiss();
            }

            // We run this loop so the event stays active until something happens.  Don't want to start another event while the player is still interacting with this one.
            while (!AmbientEvent.PrematureEndCheck(eventPeds, eventVehicles))
            {
                GameFiber.Yield();
            }
        }
    }
}
