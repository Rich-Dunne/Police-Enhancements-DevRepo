using System;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;

namespace RichsPoliceEnhancements
{
    internal static class TVI
    {
        private static bool _alreadyRunning = false;

        internal static void Main()
        {
            while (true)
            {
                var pursuit = Functions.GetActivePursuit();
                if (pursuit == null)
                {
                    //Game.LogTrivial($"[RPE TVI]: Pursuit is null.");
                    _alreadyRunning = false;
                }
                else if (pursuit != null && !_alreadyRunning)
                {
                    var suspectVeh = Functions.GetPursuitPeds(pursuit).Where(p => p && p.IsAlive).FirstOrDefault()?.CurrentVehicle;

                    if (suspectVeh && suspectVeh.HasDriver && suspectVeh.Speed > 0 && suspectVeh.FuelLevel > 0 && suspectVeh.Health > 0)
                    {
                        Game.LogTrivial("[RPE TVI]: Starting disable check loop.");
                        GameFiber DisabledCheckFiber = new GameFiber(() => VehicleDisableCheck(pursuit, suspectVeh), "RPE TVI Disable Check Fiber");
                        DisabledCheckFiber.Start();
                        _alreadyRunning = true;
                    }
                }
                GameFiber.Sleep(5000);
            }
        }

        internal static void VehicleDisableCheck(LHandle pursuit, Vehicle suspectVeh)
        {
            Game.LogTrivial("[RPE TVI]: Looping for disabled vehicle");

            while (suspectVeh && suspectVeh.HasOccupants && Functions.IsPursuitStillRunning(pursuit))
            {
                if (!Game.LocalPlayer.Character.IsAlive)
                {
                    Game.LogTrivial($"[RPE TVI]: Player is dead.");
                    _alreadyRunning = false;
                    return;
                }

                if (!suspectVeh || !suspectVeh.Driver || !suspectVeh.Driver.IsAlive)
                {
                    Game.LogTrivial($"[RPE TVI]: Suspect vehicle or driver is null or dead.");
                    _alreadyRunning = false;
                    return;
                }

                var origHeading = suspectVeh.Heading;
                foreach (Vehicle veh in suspectVeh.Driver.GetNearbyVehicles(16).Where(x => x && x.IsPoliceVehicle && x.HasDriver))
                {
                    //Game.LogTrivial("[RPE TVI]: Looping for cops in pursuit");
                    if (Rage.Native.NativeFunction.Natives.IS_ENTITY_TOUCHING_ENTITY<bool>(veh, suspectVeh))
                    {
                        var originalSuspectHandlingData = suspectVeh.HandlingData;
                        GameFiber.Sleep(2000);
                        var headingDifference = getAngleFromHeadingDifference(origHeading, suspectVeh.Heading);
                        Game.LogTrivial($"[RPE TVI]: Heading difference: {headingDifference}");
                        if (headingDifference > 90 && headingDifference < 180)
                        {
                            if (Settings.EnablePursuitUpdates)
                            {
                                Game.DisplayNotification("~y~Automatic Pursuit Updates\n~w~Suspect vehicle has been ~r~disabled~w~.");
                            }
                            Game.LogTrivial("[RPE TVI]: Vehicle has been disabled.");
                            suspectVeh.EngineHealth = 20;
                            suspectVeh.IsDriveable = false;
                            suspectVeh.FuelLevel = 0;
                            _alreadyRunning = false;
                            return;
                        }
                    }
                }
                GameFiber.Yield();

                float getAngleFromHeadingDifference(float originalHeading, float newHeading)
                {
                    return Math.Min((originalHeading - newHeading) < 0 ? originalHeading - newHeading + 360 : originalHeading - newHeading, (newHeading - originalHeading) < 0 ? newHeading - originalHeading + 360 : newHeading - originalHeading);
}
            }
            _alreadyRunning = false;
        }
    }
}
