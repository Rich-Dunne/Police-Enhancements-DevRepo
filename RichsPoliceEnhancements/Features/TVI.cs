using System;
using System.Collections.Generic;
using System.Linq;

using Rage;
using LSPD_First_Response.Mod.API;

namespace RichsPoliceEnhancements
{
    // TODO:
    // - Implement occupant actions?
    class TVI
    {
        public static LHandle pursuit;
        public static Vehicle suspectVeh;
        public static List<Ped> pursuitPedsList = new List<Ped>();
        public static bool alreadyRunning, proximityLoop;

        public static void Main()
        {
            AppDomain.CurrentDomain.DomainUnload += MyTerminationHandler;
            alreadyRunning = false;
            while (true)
            {
                GameFiber.Yield();
                //Game.LogTrivial("[RPE]: TVI Looping.");
                // Get suspect vehicle
                if (Functions.GetActivePursuit() != null && !alreadyRunning)
                {
                    Game.LogTrivial("[RPE TVI]: Player is in pursuit.");
                    pursuit = Functions.GetActivePursuit();
                    pursuitPedsList = Functions.GetPursuitPeds(pursuit).ToList();
                    GameFiber.Wait(2000);
                    GetSuspectVehicle();
                    if (suspectVeh.Exists() && suspectVeh.IsValid() && suspectVeh.HasDriver && suspectVeh.Speed > 0 && suspectVeh.FuelLevel > 0 && suspectVeh.Health > 0)
                    {
                        Game.LogTrivial("[RPE TVI]: Starting disable check loop.");
                        GameFiber DisabledCheckFiber = new GameFiber(() => VehicleDisableCheck(pursuit, suspectVeh));
                        DisabledCheckFiber.Start();
                        alreadyRunning = true;
                    }
                }
            }
        }

        public static void GetSuspectVehicle()
        {
            // This was looping while the suspects were on foot out of their disabled vehicle because a new pursuit is started
            Game.LogTrivial("[RPE TVI]: Trying to get suspect vehicle");
            //Game.DisplaySubtitle("Trying to get suspect vehicle");
            foreach (Ped occupant in pursuitPedsList)
            {
                if (occupant.IsInAnyVehicle(false))
                {
                    Game.LogTrivial("[RPE TVI]: Ped in a vehicle.");
                    //Game.DisplaySubtitle("Ped in a vehicle.");
                    suspectVeh = occupant.CurrentVehicle;
                    break;
                }
                else
                {
                    Game.LogTrivial("[RPE TVI]: Ped not in a vehicle");
                    //pursuitPedsList.Remove(occupant);
                }
            }
        }

        public static void VehicleDisableCheck(LHandle pursuit, Vehicle suspectVeh)
        {
            float origHeading;
            Game.LogTrivial("[RPE TVI]: Looping for disabled vehicle");
            while (suspectVeh.Exists() && suspectVeh.IsValid() && suspectVeh.HasOccupants && Functions.IsPursuitStillRunning(pursuit))// && Game.LocalPlayer.Character.CurrentVehicle.DistanceTo2D(suspectVeh.Position) <= 4.8f)
            {
                GameFiber.Yield();

                if (suspectVeh.Exists() && suspectVeh.IsValid())
                {
                    //Game.LogTrivial("[RPE TVI]: TVI looping on suspect vehicle.");
                    origHeading = suspectVeh.Heading;
                    try
                    {
                        if (suspectVeh.Driver.Exists() && suspectVeh.Driver.IsValid())
                        {
                            //Game.LogTrivial("[RPE TVI]: Trying foreach on nearby police vehicles");
                            foreach (Vehicle veh in suspectVeh.Driver.GetNearbyVehicles(16))
                            {
                                //Game.LogTrivial("[RPE TVI]: Looping for cops in pursuit");
                                if (veh.IsValid() && veh.IsPoliceVehicle && veh.HasDriver && Rage.Native.NativeFunction.Natives.IS_ENTITY_TOUCHING_ENTITY<bool>(veh, suspectVeh))
                                {
                                    //Game.LogTrivial("[RPE TVI]: Police vehicle nearby suspect");
                                    //if (Rage.Native.NativeFunction.Natives.IS_ENTITY_TOUCHING_ENTITY<bool>(veh, suspectVeh))
                                    //{
                                    //Game.LogTrivial("[RPE TVI]: A vehicle has made contact with suspectVeh");
                                    GameFiber.Sleep(2000);
                                    if (Math.Abs(origHeading - suspectVeh.Heading) > 110f && Math.Abs(origHeading - suspectVeh.Heading) < 360f)// && Math.Abs(origHeading - suspectVeh.Heading) < 300f)
                                    {
                                        if (RichsPoliceEnhancements.Settings.EnablePursuitUpdates)
                                        {
                                            Game.DisplayNotification("~y~Automatic Pursuit Updates~n~~w~Suspect vehicle has been ~r~disabled~w~.");
                                        }
                                        Game.LogTrivial("[RPE TVI]: Vehicle has been disabled.");
                                        //Game.DisplaySubtitle("[RPE]: Vehicle has been disabled.");
                                        suspectVeh.EngineHealth = 20;
                                        suspectVeh.IsDriveable = false;
                                        suspectVeh.FuelLevel = 0;
                                        alreadyRunning = false;
                                        break;
                                    }
                                    //}
                                }
                            }
                        }
                    }
                    catch
                    {
                        Game.LogTrivial("[RPE TVI]: Something went wrong checking if player vehicle touched suspect vehicle");
                    }

                }
                else
                {
                    Game.LogTrivial("[RPE TVI]: suspectVeh invalid for some reason");
                    alreadyRunning = false;
                }
            }
            //Game.LogTrivial("[RPE]: Starting occupant actions.");
            //Game.DisplaySubtitle("[RPE]: Starting occupant actions.");
            Game.LogTrivial("[RPE TVI]: Done looping for disabled vehicle");
            alreadyRunning = false;
            //OccupantActions(suspectVeh);
        }

        public static void OccupantActions(Vehicle veh)
        {
            List<Ped> occupantsList = new List<Ped>();
            List<Ped> copsList = new List<Ped>();
            LSPD_First_Response.Engine.Scripting.Entities.Persona suspectPersona = null;
            Random r = new Random();
            if (veh.IsValid())
            {
                foreach (Ped occupant in veh.Occupants)
                {
                    occupantsList.Add(occupant);
                    int s = r.Next(0, 100);
                    Game.LogTrivial("[RPE TVI]: s is " + s);

                    if (s >= 75)
                    {
                        Game.LogTrivial("[RPE TVI]: This ped is armed");
                        if (!occupant.Inventory.HasLoadedWeapon)
                        {
                            occupant.Inventory.GiveNewWeapon("WEAPON_COMBATPISTOL", 17, true);
                        }
                        if (s >= 90 && s < 95)
                        {
                            //suspect.Tasks.FightAgainst(Game.LocalPlayer.Character);
                            occupant.Tasks.FightAgainstClosestHatedTarget(50f); // For pursuit suspects
                        }
                        if (s >= 95 && s <= 100)
                        {
                            Game.LogTrivial("[RPE TVI]: This ped should shoot from the vehicle (currently not working)");
                            //suspect.Tasks.FightAgainstClosestHatedTarget(50f);
                            //Rage.Native.NativeFunction.Natives.TASK_DRIVE_BY(occupant, occupant.CombatTarget, Vehicle targetVehicle, float targetX, float targetY, float targetZ, float distanceToShoot, int pedAccuracy, BOOL p8, Hash firingPattern)
                            //occupant.Tasks.FightAgainstClosestHatedTarget(20f);
                            //Rage.Native.NativeFunction.Natives.TASK_DRIVE_BY(occupant, occupant.CombatTarget);

                        }
                    }
                    if (s >= 50 && s < 75)
                    {
                        Game.LogTrivial("[RPE TVI]: This ped should run");
                    }
                    if (s < 50)
                    {
                        // Remove ped from pursuit
                        // If ped is wanted or has contraband, chance to run if no police nearby and player is far away (add back to current pursuit)
                        // If ped is innocent, remain in car
                        // If multiple compliants in the same car, assigning them to the same ped variable does not work
                        // Reference the vehicle
                        Functions.RemovePedFromPursuit(occupant);
                        Game.LogTrivial("[RPE TVI]: Ped hands up");
                        occupant.Tasks.PutHandsUp(-1, null);
                        //occupant.Tasks.PlayAnimation("anim@mp_player_intincarsurrenderstd@ds@", "enter", 1, AnimationFlags.None);
                        //occupant.Tasks.PlayAnimation("anim@mp_player_intincarsurrenderstd@ds@", "idle_a", 1, AnimationFlags.Loop);
                        occupant.BlockPermanentEvents = true;
                    }
                }
                //GameFiber.Sleep(10000);
                //Game.LogTrivial("[RPE]: Starting ProximityChecker");
                //GameFiber CopProximityChecker = new GameFiber(() => ProximityChecker(proximityLoop, suspectVeh, occupantsList, copsList));
                //CopProximityChecker.Start();
            }
        }

        public static void ProximityChecker(bool loop, Vehicle suspectVeh, List<Ped> occupantsList, List<Ped> copsList)
        {
            Game.LogTrivial("[RPE TVI]: In ProximityChecker");
            // This loop might cause issues when starting a new pursuit while the loop is still running?
            // Dead peds are still occupants
            while (loop)
            {
                GameFiber.Yield();
                Game.LogTrivial("[RPE TVI]: Looping ProximityChecker with " + suspectVeh.Occupants.ToList().Count + " occupants left.");
                foreach (Ped occupant in suspectVeh.Occupants)
                {
                    if (!occupant.IsAlive || !occupant.IsInVehicle(suspectVeh, false))
                    {
                        Game.LogTrivial("[RPE TVI]: Removing occupant from list");
                        occupantsList.Remove(occupant);
                    }
                    if (occupant.IsValid() && occupant.IsAlive && occupant.IsInVehicle(suspectVeh, false) && Game.LocalPlayer.Character.Position.DistanceTo(occupant.Position) > 25f)
                    {
                        Game.LogTrivial("[RPE TVI]: Player is far from vehicle");
                        foreach (Ped ped in occupant.GetNearbyPeds(10))
                        {
                            if (Functions.IsPedACop(ped) && ped.IsAlive && ped.DistanceTo(occupant) <= 25f)
                            {
                                Game.LogTrivial("[RPE TVI]: Cop nearby, adding to list");
                                copsList.Add(ped);
                            }
                        }

                        if (copsList.Count == 0)
                        {
                            Game.LogTrivial("[RPE TVI]: No cops near compliant suspect, RUN");
                            occupant.Tasks.Clear();
                            if (Functions.GetActivePursuit() == null)
                            {
                                Game.LogTrivial("[RPE TVI]: Creating new pursuit with fleeing occupant");
                                pursuit = Functions.CreatePursuit();
                                Functions.AddPedToPursuit(pursuit, occupant);
                                Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                            }
                            else
                            {
                                Game.LogTrivial("[RPE TVI]: Re-adding fleeing ped to current pursuit");
                                Functions.AddPedToPursuit(Functions.GetActivePursuit(), occupant);
                            }
                            occupantsList.Remove(occupant);
                        }
                        else if (copsList.Count > 0)
                        {
                            Game.LogTrivial("[RPE TVI]: Clear copslist");
                            copsList.Clear();
                        }
                    }
                }
                if (!suspectVeh.HasOccupants || occupantsList.Count == 0)
                {
                    Game.LogTrivial("[RPE TVI]: All occupants are out of the vehicle or dead.  Ending loop.");
                    loop = false;
                }
                GameFiber.Sleep(5000);
            }
        }

        public static void MyTerminationHandler(object sender, EventArgs e)
        {
            alreadyRunning = false;
            proximityLoop = false;
        }
    }
}
