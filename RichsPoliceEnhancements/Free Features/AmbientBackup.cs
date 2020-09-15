using Rage;
using LSPD_First_Response.Mod.API;

namespace RichsPoliceEnhancements
{
    class AmbientBackup
    {
        public static void Main()
        {
            LHandle pursuit = null;
            while (true)
            {
                GameFiber.Yield();
                if (Functions.GetActivePursuit() != null)
                {
                    pursuit = Functions.GetActivePursuit();
                }

                if (pursuit != null && Functions.IsPursuitStillRunning(pursuit))
                {
                    //Game.LogTrivial("[RPE Ambient Backup]: Player is in pursuit");
                    foreach (Vehicle veh in Game.LocalPlayer.Character.GetNearbyVehicles(16))
                    {
                        //Game.LogTrivial("[RPE Ambient Backup]: Searching for ambient cops");
                        if (veh.IsValid() && veh.IsPoliceVehicle && veh != Game.LocalPlayer.Character.CurrentVehicle && veh.HasDriver)
                        {
                            foreach (Ped ped in veh.Occupants)
                            {
                                if (!Functions.IsPedInPursuit(ped))
                                {
                                    Functions.AddCopToPursuit(Functions.GetActivePursuit(), ped);
                                    Game.LogTrivial("[RPE Ambient Backup]: Ambient cop added to pursuit.");
                                    //Game.DisplaySubtitle("Ambient cop added to pursuit.");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
