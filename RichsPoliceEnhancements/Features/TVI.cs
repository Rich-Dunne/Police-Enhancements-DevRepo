using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;

namespace RichsPoliceEnhancements
{
    class TVI
    {
        private static bool alreadyRunning = false;

        internal static void Main()
        {
            while (true)
            {
                if (Functions.GetActivePursuit() != null && !alreadyRunning)
                {
                    Game.LogTrivial("[RPE TVI]: Player is in pursuit.");
                    var pursuit = Functions.GetActivePursuit();
                    //GameFiber.Sleep(2000);
                    var suspectVeh = Functions.GetPursuitPeds(pursuit).Where(p => p && p.IsAlive).FirstOrDefault().CurrentVehicle;

                    if (suspectVeh && suspectVeh.HasDriver && suspectVeh.Speed > 0 && suspectVeh.FuelLevel > 0 && suspectVeh.Health > 0)
                    {
                        Game.LogTrivial("[RPE TVI]: Starting disable check loop.");
                        GameFiber DisabledCheckFiber = new GameFiber(() => VehicleDisableCheck(pursuit, suspectVeh));
                        DisabledCheckFiber.Start();
                        alreadyRunning = true;
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
                if (!suspectVeh || !suspectVeh.Driver || !suspectVeh.Driver.IsAlive)
                {
                    Game.LogTrivial($"[RPE TVI]: Suspect vehicle or driver is null or dead.");
                    alreadyRunning = false;
                    return;
                }
                var origHeading = suspectVeh.Heading;
                //Game.LogTrivial("[RPE TVI]: TVI looping on suspect vehicle.");
                foreach (Vehicle veh in suspectVeh.Driver.GetNearbyVehicles(16))
                {
                    //Game.LogTrivial("[RPE TVI]: Looping for cops in pursuit");
                    if (veh && veh.IsPoliceVehicle && veh.HasDriver && Rage.Native.NativeFunction.Natives.IS_ENTITY_TOUCHING_ENTITY<bool>(veh, suspectVeh))
                    {
                        GameFiber.Sleep(2000);
                        if (Math.Abs(origHeading - suspectVeh.Heading) > 110f && Math.Abs(origHeading - suspectVeh.Heading) < 360f)
                        {
                            if (Settings.EnablePursuitUpdates)
                            {
                                Game.DisplayNotification("~y~Automatic Pursuit Updates\n~w~Suspect vehicle has been ~r~disabled~w~.");
                            }
                            Game.LogTrivial("[RPE TVI]: Vehicle has been disabled.");
                            suspectVeh.EngineHealth = 20;
                            suspectVeh.IsDriveable = false;
                            suspectVeh.FuelLevel = 0;
                            alreadyRunning = false;
                            return;
                        }
                    }
                }
                GameFiber.Yield();
            }
        }
    }
}
