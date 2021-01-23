using Rage;
using LSPD_First_Response.Mod.API;
using System.Collections.Generic;
using System;
using System.Linq;

namespace RichsPoliceEnhancements.Features
{
    internal class SuspectStamina
    {
        internal static void Main()
        {
            var pursuitPeds = new List<Ped>();
            var timeLastChecked = Game.GameTime;
            LHandle pursuit = null;

            Game.LogTrivial($"[RPE Suspect Stamina]: Suspect stamina started.");
            while (true)
            {
                if (pursuit != null && Functions.GetActivePursuit() == null)
                {
                    Game.LogTrivial($"[RPE Suspect Stamina]: Setting pursuit as null.");
                    pursuit = null;
                    pursuitPeds.Clear();
                }
                if (pursuit == null && Functions.GetActivePursuit() != null && pursuitPeds.Count == 0)
                {
                    Game.LogTrivial($"[RPE Suspect Stamina]: Assigning active pursuit to pursuit handle.");
                    pursuit = Functions.GetActivePursuit();
                    pursuitPeds = GetPursuitPeds();
                    pursuitPeds.Remove(Game.LocalPlayer.Character);
                    AssignStamina(pursuitPeds);
                }

                if(pursuit != null & pursuitPeds.Count > 0)
                {
                    if (Math.Abs(Game.GameTime - timeLastChecked) >= 500)
                    {
                        AdjustStamina(pursuitPeds);
                        timeLastChecked = Game.GameTime;
                    }
                }
                GameFiber.Yield();
            }
        }

        private static List<Ped> GetPursuitPeds() => Functions.GetPursuitPeds(Functions.GetActivePursuit()).Where(x => x && x.IsAlive && x != Game.LocalPlayer.Character && !Functions.IsPedACop(x)).ToList();

        private static void AssignStamina(List<Ped> pursuitPeds)
        {
            foreach (Ped ped in pursuitPeds)
            {
                ped.Metadata.Stamina = 100;
                Rage.Native.NativeFunction.Natives.SET_ENTITY_MAX_SPEED(ped, 6f);
                Game.LogTrivial($"[RPE Suspect Stamina]: Stamina is {ped.Metadata.Stamina}");
            }
        }

        private static void AdjustStamina(List<Ped> pursuitPeds)
        {
            foreach (Ped ped in pursuitPeds.Where(p => p && p.IsOnFoot && !Functions.IsPedArrested(p)))
            {
                //Game.LogTrivial($"[RPE Suspect Stamina]: Trying to drain stamina for {ped.Model.Name}");
                if (ped.Speed > 3f && ped.Metadata.Stamina > 0)
                {
                    ped.Metadata.Stamina -= 1;
                    //Game.LogTrivial($"[RPE Suspect Stamina]: {ped.Model.Name} stamina is now {ped.Metadata.Stamina} (speed: {ped.Speed})");
                }
                else if ((ped.IsWalking || ped.IsStopped || ped.IsStill || ped.Speed <= 1f) && ped.Metadata.Stamina < 100)
                {
                    ped.Metadata.Stamina += 1;
                    //Game.LogTrivial($"[RPE Suspect Stamina]: {ped.Model.Name} stamina is now {ped.Metadata.Stamina} (speed: {ped.Speed})");
                }

                if (ped.Metadata.Stamina > 75)
                {
                    AdjustPedSpeed(ped, 6f);
                }
                else if (ped.Metadata.Stamina <= 75 && ped.Metadata.Stamina > 50)
                {
                    AdjustPedSpeed(ped, 5f);

                    // move_injured_generic  run
                }
                else if (ped.Metadata.Stamina <= 50 && ped.Metadata.Stamina > 25)
                {
                    AdjustPedSpeed(ped, 4f);
                }
                else if (ped.Metadata.Stamina <= 25)
                {
                    AdjustPedSpeed(ped, 3f);

                    // Chance to stop and catch breath
                    // move_injured_generic runtowalk
                }

                if (Settings.CanTripDuringFootPursuit)
                {
                    TryToTripPed(ped);
                }
            }
        }

        private static void AdjustPedSpeed(Ped ped, float speed)
        {
            //Game.LogTrivial($"[RPE Suspect Stamina]: {ped.Model.Name} stamina is {ped.Metadata.Stamina}");
            Rage.Native.NativeFunction.Natives.SET_ENTITY_MAX_SPEED(ped, speed);
        }

        private static void TryToTripPed(Ped ped)
        {
            if (new Random().Next(100) <= Settings.TripChance)
            {
                Game.LogTrivial($"[RPE Suspect Stamina]: Suspect tripped");
                ped.IsRagdoll = true;
                GameFiber.Sleep(500);
                if (!ped)
                {
                    return;
                }
                ped.IsRagdoll = false;
            }
        }
    }
}
