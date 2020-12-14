using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;
using RichsPoliceEnhancements.Features;

namespace RichsPoliceEnhancements
{
    class AssaultEventFunctions
    {
        internal static void BeginEvent(AmbientEvent @event)
        {
            var attempts = 1;
            var suspects = new List<Ped>();
            var victims = new List<Ped>();
            Game.LogTrivial($"[RPE Ambient Event]: Starting Assault event.");

            LoopToFindEventPeds();

            if (@event.EventPeds.Count < 2)
            {
                Game.LogTrivial($"[RPE Ambient Event]: Unable to find suitable event peds after 100 attempts.  Ending event.");
                return;
            }

            GameFiber AssaultInteractionFiber = new GameFiber(() => EventProcess(@event), "RPE Assault Interaction Fiber");
            AssaultInteractionFiber.Start();

            void LoopToFindEventPeds()
            {
                while (@event.EventPeds.Count < 2 && attempts <= 100 && !Functions.IsCalloutRunning() && Functions.GetActivePursuit() == null)
                {
                    suspects = FindSuspects();
                    victims = FindVictims();
                    FindEventPedPair();
                    if (@event.EventPeds.Count == 2)
                    {
                        Game.LogTrivial($"Success on attempt {attempts}");
                        break;
                    }
                    @event.EventPeds.Clear();
                    suspects.Clear();
                    victims.Clear();
                    attempts++;
                    GameFiber.Sleep(500);
                }
            }

            List<Ped> NearbyPeds() => World.GetAllPeds().Where(p => PedIsRelevant(p)).ToList();

            bool PedIsRelevant(Ped ped)
            {
                if (ped && ped.IsAlive && ped.Position.DistanceTo(Game.LocalPlayer.Character.Position) < 100f && !ped.IsPlayer && !ped.IsInjured && !ped.Model.Name.Contains("A_C") && ped.RelationshipGroup != RelationshipGroup.Cop)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            List<Ped> FindSuspects()
            {
                return NearbyPeds().Where(p => p.IsOnFoot).ToList();
            }

            List<Ped> FindVictims()
            {
                return NearbyPeds().Where(p => p.IsOnFoot).ToList();
            }

            void FindEventPedPair()
            {
                // If suspect is within 10f of any ped from victims, assign driver and that victim ped as event peds
                var suspect = suspects.FirstOrDefault(x => victims.Any(y => y.DistanceTo2D(x) <= 10f));
                if (!suspect)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: No suspects found with a suitable victim nearby.");
                    @event.Cleanup();
                    return;
                }
                var victim = victims.FirstOrDefault(x => x != suspect && x.DistanceTo2D(suspect) <= 10f);
                if (!victim)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: No victim found within range of the suspect.");
                    @event.Cleanup();
                    return;
                }

                new EventPed(@event, suspect, Role.PrimarySuspect, true);
                new EventPed(@event, victim, Role.Victim, false);
            }
        }

        private static void EventProcess(AmbientEvent @event)
        {
            var suspect = @event.EventPeds.FirstOrDefault(x => x.Role == Role.PrimarySuspect);
            var victim = @event.EventPeds.FirstOrDefault(x => x.Role == Role.Victim);
            Game.LogTrivial($"[RPE Ambient Event]: Running Assault interaction.");

            suspect.Ped.Tasks.FightAgainst(victim.Ped);
            if (new Random().Next(1) == 1 || victim.Ped.Inventory.Weapons.Count > 0)
            {
                Game.LogTrivial($"[RPE Ambient Event]: Victim is fighting suspect.");
                victim.Ped.Tasks.FightAgainst(suspect.Ped);
            }
            else
            {
                Game.LogTrivial($"[RPE Ambient Event]: Victim is fleeing.");
                victim.Ped.Tasks.ReactAndFlee(suspect.Ped);
            }

            EndEvent(@event);
        }

        private static void EndEvent(AmbientEvent @event)
        {
            var suspect = @event.EventPeds.FirstOrDefault(x => x.Role == Role.PrimarySuspect);
            var victim = @event.EventPeds.FirstOrDefault(x => x.Role == Role.Victim);
            var oldDistance = Game.LocalPlayer.Character.DistanceTo2D(suspect.Ped);

            while (true)
            {
                if (!suspect.Ped || !victim.Ped || !suspect.Ped.IsAlive || !victim.Ped.IsAlive || Functions.IsPedGettingArrested(suspect.Ped))
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Suspect or victim is null or dead, or driver is arrested.  Ending event.");
                    @event.Cleanup();
                    return;
                }

                if (Game.LocalPlayer.Character.DistanceTo2D(suspect.Ped) > 150f)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player is too far away.  Ending event.");
                    @event.Cleanup();
                    return;
                }

                if(suspect.Ped.DistanceTo2D(victim.Ped) > 50f)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Victim got away from suspect.  Ending event.");
                    @event.Cleanup();
                    return;
                }

                if (Functions.GetActivePursuit() != null && Functions.IsPedInPursuit(suspect.Ped))
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player is in pursuit of suspect.  Ending event.");
                    @event.Cleanup();
                    return;
                }

                if (Math.Abs(Game.LocalPlayer.Character.DistanceTo2D(suspect.Ped) - oldDistance) > 0.15 && Game.LocalPlayer.Character.DistanceTo2D(suspect.Ped) > oldDistance && suspect.Blip.Alpha > 0f)
                {
                    suspect.Blip.Alpha -= 0.001f;
                }
                else if (Math.Abs(Game.LocalPlayer.Character.DistanceTo2D(suspect.Ped) - oldDistance) > 0.15 && Game.LocalPlayer.Character.DistanceTo2D(suspect.Ped) < oldDistance && suspect.Blip.Alpha < 1.0f)
                {
                    suspect.Blip.Alpha += 0.01f;
                }
                oldDistance = Game.LocalPlayer.Character.DistanceTo2D(suspect.Ped);

                GameFiber.Yield();
            }
        }
    }
}
