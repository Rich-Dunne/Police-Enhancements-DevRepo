using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;
using RichsPoliceEnhancements.Utils;

namespace RichsPoliceEnhancements
{
    class CarJackingEventFunctions
    {
        internal static void BeginEvent(AmbientEvent @event)
        {
            var attempts = 1;
            var jackers = new List<Ped>();
            var victims = new List<Ped>();
            Game.LogTrivial($"[RPE Ambient Event]: Starting CarJacking event.");

            LoopToFindEventPeds();

            if (@event.EventPeds.Count < 2)
            {
                Game.LogTrivial($"[RPE Ambient Event]: Jacker or victim is null.  Ending event.");
                return;
            }

            GameFiber DriveByInteractionFiber = new GameFiber(() => EventProcess(@event), "RPE CarJacking Interaction Fiber");
            DriveByInteractionFiber.Start();

            List<Ped> NearbyPeds() => World.GetAllPeds().Where(p => PedIsRelevant(p)).ToList();

            void LoopToFindEventPeds()
            {
                while (@event.EventPeds.Count < 2 && attempts <= 100 && !Functions.IsCalloutRunning() && Functions.GetActivePursuit() == null)
                {
                    jackers = FindJackers();
                    victims = FindVictims();
                    FindEventPedPair();
                    if (@event.EventPeds.Count == 2)
                    {
                        Game.LogTrivial($"Success on attempt {attempts}");
                        break;
                    }
                    @event.EventPeds.Clear();
                    jackers.Clear();
                    victims.Clear();
                    attempts++;
                    GameFiber.Sleep(500);
                }
            }

            bool PedIsRelevant(Ped ped)
            {
                if (ped && ped.IsAlive && ped.Position.DistanceTo(Game.LocalPlayer.Character.Position) < 100f && !ped.IsPlayer && !ped.IsInjured && !ped.Model.Name.Contains("A_C") && ped.RelationshipGroup != RelationshipGroup.Cop && ped.RelationshipGroup != "UBCOP" && ped.IsAmbient())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            List<Ped> FindJackers()
            {
                return NearbyPeds().Where(x => x.IsOnFoot).ToList();
            }

            List<Ped> FindVictims()
            {
                return NearbyPeds().Where(p => !jackers.Contains(p) && jackers.Any(x => Math.Abs(x.Position.Z - p.Position.Z) <= 3f) && p.CurrentVehicle && p.Speed <= 1f).ToList();
            }

            void FindEventPedPair()
            {
                // If jacker is on foot and within 15f of any ped from victims, assign jacker and that victim ped as event peds
                var jacker = jackers.FirstOrDefault(x => victims.Any(y => y.DistanceTo2D(x) <= 15f));
                if (!jacker)
                {
                    //Game.LogTrivial($"[RPE Ambient Event]: No jackers found with a suitable victim nearby.");
                    @event.Cleanup();
                    return;
                }
                var victim = victims.FirstOrDefault(x => x.DistanceTo2D(jacker) <= 15f);
                if (!victim)
                {
                    //Game.LogTrivial($"[RPE Ambient Event]: No victim found within range of the jacker.");
                    @event.Cleanup();
                    return;
                }

                new EventPed(@event, jacker, Role.PrimarySuspect, true);
                new EventPed(@event, victim, Role.Victim, false);
            }
        }

        private static void EventProcess(AmbientEvent @event)
        {
            var jacker = @event.EventPeds.FirstOrDefault(x => x.Role == Role.PrimarySuspect);
            var victim = @event.EventPeds.FirstOrDefault(x => x.Role == Role.Victim);

            Game.LogTrivial($"[RPE Ambient Event]: Running CarJacking interaction.");

            if (jacker == null || victim == null || !jacker.Ped || !victim.Ped)
            {
                Game.LogTrivial($"[RPE Ambient Event]: Jacker or victim is null.  Ending event.");
                @event.Cleanup();
                return;
            }

            jacker.Ped.Tasks.Clear();
            jacker.Ped.Tasks.EnterVehicle(victim.Ped.CurrentVehicle, -1, -1, 5f, EnterVehicleFlags.AllowJacking).WaitForCompletion();

            while(EventPedsAreValid() && victim.Ped.LastVehicle && !jacker.Ped.IsInVehicle(victim.Ped.LastVehicle, false))
            {
                CheckPlayerDistanceToJacker(@event, jacker.Ped);
                CheckEventPedsDistance();
                CheckJackerTaskStatus();
                GameFiber.Yield();
            }

            if(Settings.EventBlips && jacker.Blip)
            {
                jacker.Blip.Alpha = 100;
            }
            Game.LogTrivial($"[RPE Ambient Event]: Jacker is in the vehicle and driving away.");
            jacker.Ped.Tasks.CruiseWithVehicle(30f, VehicleDrivingFlags.Emergency);
            EndEvent(@event);

            bool EventPedsAreValid()
            {
                if (jacker == null || victim == null || !jacker.Ped || !victim.Ped)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Jacker or victim is null");
                    @event.Cleanup();
                    return false;
                }
                if (!jacker.Ped.IsAlive || !victim.Ped.IsAlive)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Jacker or victim is dead.");
                    @event.Cleanup();
                    return false;
                }
                return true;
            }

            void CheckJackerTaskStatus()
            {
                if (jacker.Ped.Tasks.CurrentTaskStatus == TaskStatus.NoTask)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Jacker [{jacker.Ped.Model}, {jacker.Ped.Handle}] has no task.  Reassiging task.");
                    jacker.Ped.Tasks.EnterVehicle(victim.Ped.CurrentVehicle, -1, -1, 5f, EnterVehicleFlags.AllowJacking);
                }
            }

            void CheckEventPedsDistance()
            {
                if (jacker.Ped.DistanceTo2D(victim.Ped) > 20f)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Victim is too far from jacker.  Ending event.");
                    @event.Cleanup();
                    return;
                }
            }
        }

        private static void EndEvent(AmbientEvent @event)
        {
            Game.LogTrivial($"[RPE Ambient Event]: In EndEvent.");
            var jacker = @event.EventPeds.FirstOrDefault(x => x.Role == Role.PrimarySuspect);
            var victim = @event.EventPeds.FirstOrDefault(x => x.Role == Role.Victim);
            var oldDistance = Game.LocalPlayer.Character.DistanceTo2D(jacker.Ped);

            while (true)
            {
                if (!jacker.Ped || !victim.Ped || !jacker.Ped.IsAlive || !victim.Ped.IsAlive || Functions.IsPedGettingArrested(jacker.Ped))
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Jacker or victim is null or dead, or jacker is arrested.  Ending event.");
                    @event.Cleanup();
                    return;
                }

                if (Game.LocalPlayer.Character.DistanceTo2D(jacker.Ped) > 150f)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player is too far away.  Ending event.");
                    @event.Cleanup();
                    return;
                }

                if (Functions.GetActivePursuit() != null && Functions.IsPedInPursuit(jacker.Ped))
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player is in pursuit of jacker.  Ending event.");
                    @event.Cleanup();
                    return;
                }

                if(Settings.EventBlips && jacker.Blip)
                {
                    if (Math.Abs(Game.LocalPlayer.Character.DistanceTo2D(jacker.Ped) - oldDistance) > 0.15 && Game.LocalPlayer.Character.DistanceTo2D(jacker.Ped) > oldDistance && jacker.Blip.Alpha > 0f)
                    {
                        jacker.Blip.Alpha -= 0.001f;
                    }
                    else if (Math.Abs(Game.LocalPlayer.Character.DistanceTo2D(jacker.Ped) - oldDistance) > 0.15 && Game.LocalPlayer.Character.DistanceTo2D(jacker.Ped) < oldDistance && jacker.Blip.Alpha < 1.0f)
                    {
                        jacker.Blip.Alpha += 0.01f;
                    }
                    oldDistance = Game.LocalPlayer.Character.DistanceTo2D(jacker.Ped);
                }

                GameFiber.Yield();
            }
        }

        private static void CheckPlayerDistanceToJacker(AmbientEvent @event, Ped jacker)
        {
            if (Game.LocalPlayer.Character.DistanceTo2D(jacker) > 150f)
            {
                Game.LogTrivial($"[RPE Ambient Event]: Player is too far away.  Ending event.");
                @event.Cleanup();
                return;
            }
        }
    }
}
