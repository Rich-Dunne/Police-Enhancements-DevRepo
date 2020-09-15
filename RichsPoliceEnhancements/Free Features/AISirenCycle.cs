using System;
using System.Collections.Generic;

using Rage;
using LSPD_First_Response.Mod.API;

namespace RichsPoliceEnhancements
{
    // BUGS:
    // - 
    public class AISirenCycle
    {
        private static LHandle pursuit;
        public static void Main()
        {
            bool pursuitActive = false;
            List<Vehicle> pursuitVehicles = new List<Vehicle>();
            while (true)
            {
                int r = new Random().Next(10000, 20000);
                GameFiber.Yield();

                if (Functions.GetActivePursuit() != null && !pursuitActive)
                {
                    pursuitActive = true;
                    pursuit = Functions.GetActivePursuit();

                    // Fiber to constantly check for new pursuit peds
                    Game.LogTrivial("[RPE AI Siren Cycle]: Beginning pursuit cop compiler");
                    GameFiber PursuitCopsCompilerFiber = new GameFiber(() => PursuitCopsCompiler(pursuit, pursuitVehicles));
                    PursuitCopsCompilerFiber.Start();
                }

                if (pursuit != null && !Functions.IsPursuitStillRunning(pursuit))
                {
                    //Game.LogTrivial("[RPE AI Siren Cycle]: No active pursuit.");
                    pursuitActive = false;
                    pursuit = null;
                    pursuitVehicles.Clear();
                }
            }
        }

        public static void PursuitCopsCompiler(LHandle pursuit, List<Vehicle> pursuitVehicles)
        {
            while (Functions.GetActivePursuit() != null)
            {
                GameFiber.Yield();

                foreach (Vehicle veh in Game.LocalPlayer.Character.GetNearbyVehicles(16))
                {
                    //Game.LogTrivial("[RPE]: Looping for cops in pursuit");
                    if (veh.Exists() && veh.IsValid() && veh != Game.LocalPlayer.Character.LastVehicle && veh.IsPoliceVehicle && veh.HasDriver && veh.Driver != Game.LocalPlayer.Character && Functions.IsPedInPursuit(veh.Driver) && veh.IsSirenOn)
                    {
                        if (!pursuitVehicles.Contains(veh))
                        {
                            Game.LogTrivial("[RPE AI Siren Cycle]: Added police vehicle to list for siren cycling");
                            pursuitVehicles.Add(veh);

                            Game.LogTrivial("[RPE AI Siren Cycle]: Starting vehicle's personal siren cycle fiber");
                            GameFiber PersonalSirenCyclerFiber = new GameFiber(() => PersonalSirenCycler(pursuit, veh));
                            PersonalSirenCyclerFiber.Start();
                        }
                        else
                        {
                            //Game.LogTrivial("[RPE]: This unit is already in the list");
                        }
                    }
                }
            }
        }

        public static void PersonalSirenCycler(LHandle pursuit, Vehicle policeVeh)
        {
            bool silentBackup = false;
            if (RichsPoliceEnhancements.Settings.EnableSilentBackup)
            {
                GameFiber.StartNew(delegate
                {
                    while (Functions.IsPursuitStillRunning(pursuit) && policeVeh.Exists() && policeVeh.IsValid())
                    {
                        GameFiber.Yield();
                        if (Game.LocalPlayer.LastVehicle.Exists() && Game.LocalPlayer.LastVehicle.IsValid() && policeVeh.Exists() && policeVeh.IsValid() && policeVeh.HasDriver && policeVeh.DistanceTo(Game.LocalPlayer.Character.LastVehicle) <= 100f && !Game.LocalPlayer.Character.LastVehicle.IsSirenOn && policeVeh.IsSirenOn)
                        {
                            Game.LogTrivial("[RPE AI Siren Cycle]: Silencing nearby units.");
                            //Game.DisplaySubtitle("Silencing nearby units");
                            policeVeh.IsSirenOn = false;
                            policeVeh.IsSirenSilent = true;
                            silentBackup = true;
                        }
                        else if (Game.LocalPlayer.LastVehicle.Exists() && Game.LocalPlayer.LastVehicle.IsValid() && policeVeh.Exists() && policeVeh.IsValid() && policeVeh.HasDriver && policeVeh.DistanceTo(Game.LocalPlayer.Character.LastVehicle) <= 100f && Game.LocalPlayer.Character.LastVehicle.IsSirenOn && !policeVeh.IsSirenOn)
                        {
                            policeVeh.IsSirenOn = true;
                            policeVeh.IsSirenSilent = false;
                            silentBackup = false;
                        }
                    }
                });

                Game.LogTrivial("[RPE AI Siren Cycle]: In the silent backup loop");
                while (Functions.IsPursuitStillRunning(pursuit) && policeVeh.Exists() && policeVeh.IsValid())
                {
                    GameFiber.Yield();

                    /*if (Game.LocalPlayer.LastVehicle.Exists() && Game.LocalPlayer.LastVehicle.IsValid() && policeVeh.Exists() && policeVeh.IsValid() && policeVeh.HasDriver && policeVeh.DistanceTo(Game.LocalPlayer.Character.LastVehicle) <= 100f && !Game.LocalPlayer.Character.LastVehicle.IsSirenOn && policeVeh.IsSirenOn)
                    {
                        Game.LogTrivial("[RPE AI Siren Cycler]: Silencing nearby units.");
                        Game.DisplaySubtitle("Silencing nearby units");
                        policeVeh.IsSirenOn = false;
                        policeVeh.IsSirenSilent = true;
                        silentBackup = true;
                    }*/
                    if (!silentBackup) // else if
                    {
                        Game.LogTrivial("[RPE AI Siren Cycle]: Cycling sirens.");
                        policeVeh.IsSirenOn = true;
                        policeVeh.IsSirenSilent = false;
                        GameFiber.Sleep(10000);
                        if (Functions.IsPursuitStillRunning(pursuit) && policeVeh.IsValid() && policeVeh.Exists() && policeVeh.HasDriver && policeVeh.IsSirenOn)
                        {
                            policeVeh.IsSirenOn = false;
                            policeVeh.IsSirenSilent = true;

                            GameFiber.Sleep(1);

                            policeVeh.IsSirenOn = true;
                            policeVeh.IsSirenSilent = false;
                        }
                    }
                }
            }
            else
            {
                Game.LogTrivial("[RPE AI Siren Cycle]: Cycling sirens without Silent Backup enabled");
                while (Functions.IsPursuitStillRunning(pursuit) && policeVeh.Exists() && policeVeh.IsValid())
                {
                    GameFiber.Yield();

                    GameFiber.Sleep(10000);
                    if (Functions.IsPursuitStillRunning(pursuit) && policeVeh.IsValid() && policeVeh.Exists() && policeVeh.HasDriver && policeVeh.IsSirenOn)
                    {
                        policeVeh.IsSirenOn = false;
                        policeVeh.IsSirenSilent = true;

                        GameFiber.Sleep(1);

                        policeVeh.IsSirenOn = true;
                        policeVeh.IsSirenSilent = false;
                    }
                }

                Game.LogTrivial("[RPE AI Siren Cycle]: Pursuit is over OR policeVeh doesn't exist OR driver is out of policeVeh");
            }
        }
    }
}
