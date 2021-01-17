using System;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;

namespace RichsPoliceEnhancements.Features
{
    internal static class TVI
    {
        private static bool AlreadyRunning { get; set; } = false;

        internal static void Main()
        {
            while (true)
            {
                var pursuit = Functions.GetActivePursuit();
                if (pursuit == null)
                {
                    //Game.LogTrivial($"[RPE TVI]: Pursuit is null.");
                    AlreadyRunning = false;
                }
                else if (pursuit != null && !AlreadyRunning)
                {
                    var suspectVeh = Functions.GetPursuitPeds(pursuit).Where(p => p && p.IsAlive).FirstOrDefault()?.CurrentVehicle;

                    if (suspectVeh && suspectVeh.HasDriver && suspectVeh.Speed > 0 && suspectVeh.FuelLevel > 0 && suspectVeh.Health > 0)
                    {
                        Game.LogTrivial("[RPE TVI]: Starting disable check loop.");
                        GameFiber.StartNew(() => VehicleDisableCheck(pursuit, suspectVeh), "RPE TVI Disable Check Fiber");
                        AlreadyRunning = true;
                    }
                }

                GameFiber.Sleep(5000);
            }
        }

        internal static void VehicleDisableCheck(LHandle pursuit, Vehicle suspectVehicle)
        {
            Game.LogTrivial("[RPE TVI]: Looping for disabled vehicle");

            while (suspectVehicle && suspectVehicle.HasOccupants && Functions.IsPursuitStillRunning(pursuit))
            {
                if (PlayerOrSuspectIsInvalid(suspectVehicle))
                {
                    return;
                }

                var originalHeading = suspectVehicle.Heading;
                foreach (Vehicle veh in suspectVehicle.Driver.GetNearbyVehicles(16).Where(x => x && x.IsPoliceVehicle && x.HasDriver))
                {
                    //Game.LogTrivial("[RPE TVI]: Looping for cops in pursuit");
                    if (Rage.Native.NativeFunction.Natives.IS_ENTITY_TOUCHING_ENTITY<bool>(veh, suspectVehicle))
                    {
                        GameFiber.Sleep(2000);
                        if (PlayerOrSuspectIsInvalid(suspectVehicle))
                        {
                            return;
                        }

                        var headingDifference = GetAngleFromHeadingDifference(originalHeading, suspectVehicle.Heading);
                        //Game.LogTrivial($"[RPE TVI]: Heading difference: {headingDifference}");
                        if (headingDifference > 90 && headingDifference < 180)
                        {
                            DisableSuspectVehicle(suspectVehicle);
                            AlreadyRunning = false;
                            return;
                        }
                    }
                }

                GameFiber.Yield();
            }
            AlreadyRunning = false;
        }

        private static bool PlayerOrSuspectIsInvalid(Vehicle suspectVehicle)
        {
            if (!Game.LocalPlayer.Character.IsAlive)
            {
                Game.LogTrivial($"[RPE TVI]: Player is dead.");
                AlreadyRunning = false;
                return true;
            }

            if (!suspectVehicle || !suspectVehicle.Driver || !suspectVehicle.Driver.IsAlive)
            {
                Game.LogTrivial($"[RPE TVI]: Suspect vehicle or driver is null or dead.");
                AlreadyRunning = false;
                return true;
            }

            return false;
        }

        private static float GetAngleFromHeadingDifference(float originalHeading, float newHeading)
        {
            return Math.Min((originalHeading - newHeading) < 0 ? originalHeading - newHeading + 360 : originalHeading - newHeading, (newHeading - originalHeading) < 0 ? newHeading - originalHeading + 360 : newHeading - originalHeading);
        }

        private static void DisableSuspectVehicle(Vehicle suspectVehicle)
        {
            if (Settings.EnablePursuitUpdates)
            {
                Game.DisplayNotification("~y~Automatic Pursuit Updates\n~w~Suspect vehicle has been ~r~disabled~w~.");
            }
            Game.LogTrivial("[RPE TVI]: Vehicle has been disabled.");
            suspectVehicle.EngineHealth = 20;
            suspectVehicle.IsDriveable = false;
            suspectVehicle.FuelLevel = 0;
        }
    }
}
