using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;

namespace RichsPoliceEnhancements.Features
{
    public class AISirenCycle
    {
        internal static void Main()
        {
            LHandle pursuit = null;
            var pursuitVehicles = new List<Vehicle>();
            bool pursuitCopsCollectorStarted = false;

            while (true)
            {
                if (Functions.GetActivePursuit() != null && !pursuitCopsCollectorStarted)
                {
                    pursuit = Functions.GetActivePursuit();

                    Game.LogTrivial("[RPE AI Siren Cycle]: Beginning pursuit cop collector");
                    GameFiber PursuitCopsCollectorFiber = new GameFiber(() => PursuitCopsCollector(pursuit, pursuitVehicles), "Pursuit Cops Collector Fiber");
                    PursuitCopsCollectorFiber.Start();
                    pursuitCopsCollectorStarted = true;
                }

                if (pursuit != null && Functions.GetActivePursuit() == null)
                {
                    Game.LogTrivial("[RPE AI Siren Cycle]: Pursuit is no longer running");
                    pursuit = null;
                    pursuitVehicles.Clear();
                    pursuitCopsCollectorStarted = false;
                }

                GameFiber.Yield();
            }
        }

        private static void PursuitCopsCollector(LHandle pursuit, List<Vehicle> pursuitVehicles)
        {
            while (Functions.GetActivePursuit() != null)
            {
                foreach (Vehicle veh in Game.LocalPlayer.Character.GetNearbyVehicles(16).Where(v => v && v.IsPoliceVehicle && v != Game.LocalPlayer.Character.LastVehicle && v != Game.LocalPlayer.Character.CurrentVehicle && v.HasDriver && v.Driver.IsAlive && Functions.IsPedInPursuit(v.Driver) && v.IsSirenOn && !pursuitVehicles.Contains(v)))
                {
                    Game.LogTrivial("[RPE AI Siren Cycle]: Added police vehicle to list for siren cycling");
                    pursuitVehicles.Add(veh);

                    Game.LogTrivial("[RPE AI Siren Cycle]: Starting vehicle's personal siren cycle fiber");
                    GameFiber AISirenCyclerFiber = new GameFiber(() => AISirenCycler(pursuit, veh), "Siren Cycler Fiber");
                    AISirenCyclerFiber.Start();
                }
                GameFiber.Yield();
            }
        }

        private static void AISirenCycler(LHandle pursuit, Vehicle policeVeh)
        {
            Game.LogTrivial($"[RPE AI Siren Cycle]: In the siren cycler");
            Game.LogTrivial($"[RPE AI Siren Cycle]: IsPursuitStillRunning: {Functions.IsPursuitStillRunning(pursuit)}");
            policeVeh.IsSirenOn = false;
            policeVeh.IsSirenSilent = true;
            int randomSleepDuration;
            while (Functions.IsPursuitStillRunning(pursuit) && policeVeh)
            {
                randomSleepDuration = new Random().Next(10000, 20000);
                GameFiber.Sleep(randomSleepDuration);
                Game.LogTrivial($"[RPE AI Siren Cycle]: IsPursuitStillRunning: {Functions.IsPursuitStillRunning(pursuit)}");

                if (!policeVeh)
                {
                    Game.LogTrivial($"[RPE AI Siren Cycle]: Police vehicle is null.");
                    return;
                }

                if (!Functions.IsPursuitStillRunning(pursuit))
                {
                    Game.LogTrivial($"[RPE AI Siren Cycle]: Pursuit is over, exiting siren cycler.");
                    return;
                }

                if (!policeVeh.HasDriver || !policeVeh.Driver.IsAlive)
                {
                    Game.LogTrivial($"[RPE AI Siren Cycle]: Police vehicle doesn't have a live driver.  We'll keep looping in case the vehicle gets a new driver.");
                    GameFiber.Yield();
                    continue;
                }

                if (Settings.EnableSilentBackup && Game.LocalPlayer.Character.LastVehicle && !Game.LocalPlayer.Character.LastVehicle.IsSirenOn)
                {
                    Game.LogTrivial($"[RPE AI Siren Cycle]: SilentBackup is enabled and your vehicle's siren is off, so we don't need to cycle the AI's sirens.");
                    GameFiber.Yield();
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
                    Game.LogTrivial($"[RPE AI Siren Cycle]: Police vehicle is null");
                    return;
                }
            }
            Game.LogTrivial($"[RPE AI Siren Cycle]: Pursuit is over");
        }
    }
}
