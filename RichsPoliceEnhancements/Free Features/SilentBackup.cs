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
                bool PursuitOrCalloutActive = Functions.GetCurrentPullover() != null || Functions.IsCalloutRunning() && Functions.GetActivePursuit() == null;

                if (Game.LocalPlayer.Character.LastVehicle && PursuitOrCalloutActive)
                {
                    foreach (Vehicle policeVeh in Game.LocalPlayer.Character.GetNearbyVehicles(16).Where(v => v && v.IsPoliceVehicle && v != Game.LocalPlayer.Character.LastVehicle && v.DistanceTo2D(Game.LocalPlayer.Character.LastVehicle) <= 100f && v.IsSirenOn))
                    {
                        if (!Game.LocalPlayer.Character.LastVehicle.IsSirenOn)
                        {
                            Game.LogTrivial($"[RPE]:  Silencing nearby units");
                            policeVeh.IsSirenOn = false;
                            policeVeh.IsSirenSilent = true;
                        }
                        else
                        {
                            Game.LogTrivial($"[RPE]:  Enabling nearby units' sirens");
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
