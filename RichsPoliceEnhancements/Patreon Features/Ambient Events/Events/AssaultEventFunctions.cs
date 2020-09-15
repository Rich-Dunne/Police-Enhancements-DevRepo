using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;


namespace RichsPoliceEnhancements
{
    class AssaultEventFunctions
    {
        public static void RunEventFunctions(Ped player, List<Ped> nearbyPeds, List<EventPed> eventPeds, List<EventVehicle> eventVehicles, List<Blip> eventBlips)
        {
            Game.LogTrivial($"[Rich Ambiance] Starting Assault event.");
            if (FindSuspect(nearbyPeds, eventPeds, eventBlips) != null && FindVictim(nearbyPeds, eventPeds) != null)
            {
                AssaultInteraction(eventPeds, eventBlips);
            }
            else
            {
                Game.LogTrivial($"[Rich Ambiance] Suspect or victim returned null.  Ending event.");
            }
        }

        private static Ped FindSuspect(List<Ped> pedList, List<EventPed> eventPeds, List<Blip> eventBlips)
        {
            foreach (Ped p in pedList.Where(p => p.IsOnFoot))
            {
                // Ped Settings
                Ped suspect = p;
                suspect.IsPersistent = true;
                suspect.BlockPermanentEvents = true;

                // Blip Settings
                /*Blip suspectBlip = suspect.AttachBlip();
                suspectBlip.Sprite = BlipSprite.StrangersandFreaks;
                suspectBlip.Color = Color.Red;
                suspectBlip.Scale = 0.75f;
                eventBlips.Add(suspectBlip);*/

                eventPeds.Add(new EventPed("Assault", suspect));
                Game.LogTrivial($"[Rich Ambiance] Suspect found.");
                return suspect;
            }
            Game.LogTrivial($"[Rich Ambiance] No suitable suspect peds found.");
            return null;
        }

        private static Ped FindVictim(List<Ped> pedList, List<EventPed> eventPeds)
        {
            Ped suspect = eventPeds[0].Ped;
            Ped victim;
            List<Ped> sortedList = pedList.OrderBy(p => p.DistanceTo(suspect)).ToList();

            foreach (Ped p in sortedList)
            {
                if (p.IsOnFoot && p.DistanceTo(suspect) > 0 && p.DistanceTo(suspect) <= 15f)
                {
                    victim = p;
                    victim.IsPersistent = true;
                    victim.BlockPermanentEvents = true;
                    eventPeds.Add(new EventPed("Assault", victim));
                    Game.LogTrivial($"[Rich Ambiance] Victim found.");
                    return victim;
                }
            }
            suspect.IsPersistent = false;
            suspect.Tasks.Clear();
            suspect.Dismiss();

            Game.LogTrivial($"[Rich Ambiance] No victims found close enough to suspect.");
            return null;
        }

        private static void AssaultInteraction(List<EventPed> eventPeds, List<Blip> eventBlips)
        {
            Ped suspect = eventPeds[0].Ped;
            Ped victim = eventPeds[1].Ped;

            suspect.Tasks.FightAgainst(victim);

            Game.LogTrivial($"[Rich Ambiance] Waiting for suspect to attack victim.");
            while (!victim.HasBeenDamagedBy(suspect) && !AmbientEvent.PrematureEndCheck(eventPeds))
            {
                GameFiber.Yield();
            }

            if (Settings.EventBlips)
            {
                Game.LogTrivial($"[Rich Ambiance] Creating event blip.");
                Blip suspectBlip = suspect.AttachBlip();
                suspectBlip.Sprite = BlipSprite.StrangersandFreaks;
                suspectBlip.Color = Color.Red;
                suspectBlip.Scale = 0.75f;
                eventBlips.Add(suspectBlip);
            }

            if (new Random().Next(1) == 1)
            {
                Game.LogTrivial($"[Rich Ambiance] Victim is fighting suspect.");
                victim.Tasks.FightAgainst(suspect);
            }
            else
            {
                Game.LogTrivial($"[Rich Ambiance] Victim is fleeing.");
                victim.Tasks.ReactAndFlee(suspect);
            }

            Game.LogTrivial($"[Rich Ambiance] PrematureEndCheck looping.");
            while (!AmbientEvent.PrematureEndCheck(eventPeds))
            {
                GameFiber.Sleep(1000);
            }
        }
    }
}
