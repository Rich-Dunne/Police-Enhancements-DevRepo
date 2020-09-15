using Rage;
using LSPD_First_Response.Mod.API;

namespace RichsPoliceEnhancements
{
    // BUGS:
    // - 
    public class SilentBackup
    {
        public static void Main()
        {
            while (true)
            {
                GameFiber.Yield();
                //Game.LogTrivial("[RPE Silent Backup]: Looping for backup.");
                if (Game.LocalPlayer.Character.LastVehicle.Exists() && Game.LocalPlayer.Character.LastVehicle.IsValid())
                {
                    //Game.LogTrivial("[RPE Silent Backup]: Player's last vehicle exists and is valid.");
                    if (Functions.GetCurrentPullover() != null || Functions.IsCalloutRunning() && Functions.GetActivePursuit() == null)
                    {
                        //Game.LogTrivial("[RPE Silent Backup]: Traffic stop or callout is active.");
                        foreach (Vehicle cop in Game.LocalPlayer.Character.GetNearbyVehicles(16))
                        {
                            //Game.LogTrivial("[RPE Silent Backup]: Looping through nearby vehicles.");
                            if (Game.LocalPlayer.LastVehicle.Exists() && Game.LocalPlayer.LastVehicle.IsValid() && cop.Exists() && cop.IsValid() && cop.IsPoliceVehicle && cop != Game.LocalPlayer.LastVehicle && cop.DistanceTo(Game.LocalPlayer.Character.LastVehicle) <= 100f && !Game.LocalPlayer.Character.LastVehicle.IsSirenOn && cop.IsSirenOn)
                            {
                                //Game.DisplaySubtitle(cop.GetType().Assembly.FullName); // Shows Rage because Rage commands spawned the vehicle
                                Game.LogTrivial("[RPE Silent Backup]: Found nearby cop car with siren on while your siren is off.");
                                if (cop.HasDriver)
                                {
                                    Game.LogTrivial("[RPE Silent Backup]: Silencing nearby units.");
                                    //Game.DisplaySubtitle("Silencing nearby units");
                                    cop.IsSirenOn = false;
                                    cop.IsSirenSilent = true;
                                }

                                //GameFiber.Sleep(5000);
                                if (!cop.HasDriver && !Game.LocalPlayer.Character.LastVehicle.IsSirenOn)
                                {
                                    //Game.DisplaySubtitle("Driver out of backup vehicle");
                                    cop.IsSirenOn = false;
                                    cop.IsSirenSilent = true;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
