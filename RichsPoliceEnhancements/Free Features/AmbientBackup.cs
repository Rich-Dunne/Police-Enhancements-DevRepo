using Rage;
using LSPD_First_Response.Mod.API;
using System.Linq;

namespace RichsPoliceEnhancements
{
    class AmbientBackup
    {
        internal static void Main()
        {
            LHandle pursuit = null;
            while (true)
            {
                if (Functions.GetActivePursuit() != null)
                {
                    pursuit = Functions.GetActivePursuit();
                }

                if (pursuit != null && Functions.IsPursuitStillRunning(pursuit))
                {
                    foreach (Vehicle veh in Game.LocalPlayer.Character.GetNearbyVehicles(16).Where(v => v && v.IsPoliceVehicle && v != Game.LocalPlayer.Character.CurrentVehicle && v.HasDriver))
                    {
                        foreach (Ped ped in veh.Occupants.Where(p => p && !Functions.IsPedInPursuit(p)))
                        {
                            Functions.AddCopToPursuit(Functions.GetActivePursuit(), ped);
                            Game.LogTrivial("[RPE Ambient Backup]: Ambient cop added to pursuit.");
                        }
                    }
                }
                GameFiber.Yield();
            }
        }
    }
}
