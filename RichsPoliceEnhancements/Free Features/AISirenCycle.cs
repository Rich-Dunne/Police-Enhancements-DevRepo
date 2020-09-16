﻿using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;

namespace RichsPoliceEnhancements
{
    public class AISirenCycle
    {
        internal static void Main()
        {
            LHandle pursuit = null;
            List<Vehicle> pursuitVehicles = new List<Vehicle>();

            while (true)
            {
                if (Functions.GetActivePursuit() != null)
                {
                    pursuit = Functions.GetActivePursuit();

                    Game.LogTrivial("[RPE AI Siren Cycle]: Beginning pursuit cop collector");
                    GameFiber PursuitCopsCollectorFiber = new GameFiber(() => PursuitCopsCollector(pursuit, pursuitVehicles));
                    PursuitCopsCollectorFiber.Start();
                }

                if (pursuit != null && !Functions.IsPursuitStillRunning(pursuit))
                {
                    pursuit = null;
                    pursuitVehicles.Clear();
                }

                GameFiber.Yield();
            }
        }

        internal static void PursuitCopsCollector(LHandle pursuit, List<Vehicle> pursuitVehicles)
        {
            while (pursuit != null)
            {
                foreach (Vehicle veh in Game.LocalPlayer.Character.GetNearbyVehicles(16).Where(v => v && v.IsPoliceVehicle && v != Game.LocalPlayer.Character.LastVehicle && v != Game.LocalPlayer.Character.CurrentVehicle && v.HasDriver && Functions.IsPedInPursuit(v.Driver) && v.IsSirenOn && !pursuitVehicles.Contains(v)))
                {
                    Game.LogTrivial("[RPE AI Siren Cycle]: Added police vehicle to list for siren cycling");
                    pursuitVehicles.Add(veh);

                    Game.LogTrivial("[RPE AI Siren Cycle]: Starting vehicle's personal siren cycle fiber");
                    GameFiber AISirenCyclerFiber = new GameFiber(() => AISirenCycler(pursuit, veh));
                    AISirenCyclerFiber.Start();
                }
                GameFiber.Yield();
            }
        }

        internal static void AISirenCycler(LHandle pursuit, Vehicle policeVeh)
        {
            int randomSleepDuration = 10000;
            while (Functions.IsPursuitStillRunning(pursuit) && policeVeh)
            {
                randomSleepDuration = new Random().Next(10000, 20000);

                if (!policeVeh.HasDriver)
                {
                    Game.LogTrivial($"[RPE]: Police vehicle doesn't have a driver.  We'll keep looping in case they re-enter the vehicle.");
                    GameFiber.Yield();
                    continue;
                }

                if (Settings.EnableSilentBackup && Game.LocalPlayer.Character.LastVehicle && !Game.LocalPlayer.Character.LastVehicle.IsSirenOn)
                {
                    Game.LogTrivial($"[RPE]: SilentBackup is enabled and your vehicle's siren is off, so we don't need to cycle the AI's sirens.");
                    continue;
                }

                policeVeh.IsSirenOn = false;
                policeVeh.IsSirenSilent = true;
                GameFiber.Sleep(1);
                if (policeVeh)
                {
                    policeVeh.IsSirenOn = true;
                    policeVeh.IsSirenSilent = false;
                }
                else
                {
                    Game.LogTrivial($"[RPE]: Police vehicle is no longer valid");
                    return;
                }
            }
            GameFiber.Sleep(randomSleepDuration);
        }
    }
}
