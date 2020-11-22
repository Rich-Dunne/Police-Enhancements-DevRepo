using Rage;
using LSPD_First_Response.Mod.API;
using System.Linq;

namespace RichsPoliceEnhancements
{
    internal class SilentBackup
    {
        internal static void Main()
        {
            while (true)
            {
                //bool PursuitOrCalloutActive = Functions.GetCurrentPullover() != null || Functions.IsCalloutRunning() || Functions.GetActivePursuit() != null;
                bool isCalloutRunning = Functions.IsCalloutRunning();
                bool isCurrentPulloverActive = Functions.GetCurrentPullover() != null;
                bool isPursuitActive = Functions.GetActivePursuit() != null;

                if (Game.LocalPlayer.Character.LastVehicle && (isCalloutRunning || isCurrentPulloverActive || isPursuitActive))
                {
                    foreach (Vehicle policeVeh in Game.LocalPlayer.Character.GetNearbyVehicles(16).Where(v => v && v.IsPoliceVehicle && v.HasDriver && v != Game.LocalPlayer.Character.LastVehicle && v.DistanceTo2D(Game.LocalPlayer.Character.LastVehicle) <= 100f))// && v.IsSirenOn))
                    {
                        //Game.LogTrivial($"[RPE Silent Backup]: Found nearby police vehicle with siren on.");
                        if (!Game.LocalPlayer.Character.LastVehicle.IsSirenOn && policeVeh.IsSirenOn)
                        {
                            //Game.LogTrivial($"[RPE Silent Backup]:  Silencing nearby units");
                            policeVeh.IsSirenOn = false;
                            policeVeh.IsSirenSilent = true;
                        }
                        else if(Game.LocalPlayer.Character.LastVehicle.IsSirenOn)
                        {
                            //Game.LogTrivial($"[RPE Silent Backup]:  Enabling nearby units' sirens");
                            policeVeh.IsSirenOn = true;
                            policeVeh.IsSirenSilent = false;
                        }
                    }
                }
                GameFiber.Yield();
            }
        }
    }
}
