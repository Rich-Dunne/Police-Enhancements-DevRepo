using System;
using System.Collections.Generic;

using Rage;
using LSPD_First_Response.Mod.API;

namespace RichsPoliceEnhancements
{

    // BUGS:
    // - 
    public class PursuitUpdates
    {
        private static Vehicle suspectVeh;
        private static string headingDirection, trafficCondition;
        //private static bool stopNotifications = false;
        private static int NotificationTimer = Settings.PursuitUpdateTimer *= 1000;

        public static void PursuitUpdateHandler(LHandle pursuit)
        {
            List<Vehicle> vehiclesList = new List<Vehicle>();

            Game.LogTrivial("[RPE Pursuit Update]: Pursuit started");
            GameFiber.StartNew(delegate
            {
                while (Functions.GetActivePursuit() == null)
                {
                    GameFiber.Yield();
                }

                foreach (Ped p in Functions.GetPursuitPeds(pursuit))
                {
                    Game.LogTrivial("[RPE Pursuit Update]: Pursuit ped found.");
                    if (p && !Functions.IsPedACop(p) && p.IsInAnyVehicle(false))
                    {
                        suspectVeh = p.CurrentVehicle;
                        break;
                    }
                }

                Game.LogTrivial("[RPE Pursuit Update]: Defining heading directions");
                if (suspectVeh.Exists() && suspectVeh.IsValid())
                {
                    for (int i = 0; i <= suspectVeh.Occupants.Length - 1; i++)
                    {
                        Game.LogTrivial("[RPE Pursuit Update]: Started a new notification fiber for occupant");
                        GameFiber NotificationUpdaterFiber = new GameFiber(() => NotificationUpdater(pursuit, suspectVeh.Occupants[i], vehiclesList, i));
                        NotificationUpdaterFiber.Start();

                        GameFiber.Sleep(1000);
                    }
                }
                else
                {
                    Game.LogTrivial("[RPE Pursuit Update]: suspectVeh is null for some reason.");
                }
            });
        }

        public static void NotificationUpdater(LHandle pursuit, Ped occupant, List<Vehicle> vehiclesList, int i)
        {
            bool stopNotifications = false;
            Game.LogTrivial("[RPE Pursuit Update]: Notifications running for suspect #" + (i + 1));
            GameFiber.StartNew(delegate
            {
                while (!stopNotifications)
                {
                    GameFiber.Yield();

                    if (!occupant.Exists() || !occupant.IsValid() || !occupant.IsAlive)
                    {
                        //Game.DisplayNotification("~y~Automatic Pursuit Updates~n~~w~Pursuit is over.");
                        Game.LogTrivial("[RPE Pursuit Update]: Suspect doesn't exist, isn't valid, or is dead.  Stopping notifications.");
                        stopNotifications = true;
                    }
                }
            });

            GameFiber.Sleep(5000);
            while (!stopNotifications) //Functions.IsPursuitStillRunning(pursuit) && 
            {
                GameFiber.Yield();

                Game.LogTrivial("[RPE Pursuit Update]: Running through notification loop.");
                GameFiber.Sleep(NotificationTimer);
                //GameFiber.Sleep(20000);
                if (occupant.IsInAnyVehicle(false) && occupant.CurrentVehicle.Driver == occupant && !Functions.IsPedVisualLost(occupant))
                {
                    if (occupant.CurrentVehicle.Heading >= 315 || occupant.CurrentVehicle.Heading < 45)
                    {
                        headingDirection = "north";
                    }
                    else if (occupant.CurrentVehicle.Heading >= 45 && occupant.CurrentVehicle.Heading < 135)
                    {
                        headingDirection = "west";
                    }
                    else if (occupant.CurrentVehicle.Heading >= 135 && occupant.CurrentVehicle.Heading < 225)
                    {
                        headingDirection = "south";
                    }
                    else if (occupant.CurrentVehicle.Heading >= 225 && occupant.CurrentVehicle.Heading < 315)
                    {
                        headingDirection = "east";
                    }


                    if (occupant.CurrentVehicle.Driver.Exists() && occupant.CurrentVehicle.Driver.IsValid())
                    {
                        if (vehiclesList.Count > 0)
                        {
                            vehiclesList.Clear();
                        }
                        foreach (Vehicle v in occupant.CurrentVehicle.Driver.GetNearbyVehicles(16))
                        {
                            if (v.IsValid() && v.Exists() && v.HasDriver && !v.IsPoliceVehicle && v != suspectVeh)
                            {
                                Game.LogTrivial("[RPE Pursuit Update]: Added nearby vehicle to list");
                                vehiclesList.Add(v);
                            }
                            else
                            {
                                Game.LogTrivial("[RPE Pursuit Update]: Vehicle found but not added");
                            }
                        }
                        stopNotifications = false;
                    }
                    else
                    {
                        Game.LogTrivial("[RPE Pursuit Update]: Suspect vehicle driver does not exist/is not valid.");
                        return;
                    }

                    //Game.LogTrivial("[RPE]: Notification updater trafficCondition");
                    if (vehiclesList.Count < 6)
                    {
                        trafficCondition = "~g~light";
                    }
                    else if (vehiclesList.Count >= 6 && vehiclesList.Count < 12)
                    {
                        trafficCondition = "~o~moderate";
                    }
                    else if (vehiclesList.Count >= 12)
                    {
                        trafficCondition = "~r~heavy";
                    }

                    Game.LogTrivial(string.Format("[RPE Pursuit Update]: Suspect {0} is heading {1} on {2}.  Speeds are {3}mph, traffic is {4}.", i + 1, headingDirection, World.GetStreetName(World.GetStreetHash(occupant.Position)), Convert.ToInt32(occupant.Speed) * 2, trafficCondition));
                    Game.DisplayNotification(string.Format("~y~Automatic Pursuit Updates~n~~w~Suspect {0} is heading ~b~{1} ~w~on ~b~{2}~w~.  Speeds are ~b~{3}mph~w~, traffic is {4}~w~.", i + 1, headingDirection, World.GetStreetName(World.GetStreetHash(occupant.Position)), Convert.ToInt32(occupant.Speed) * 2, trafficCondition));
                }

                if (occupant.IsValid() && occupant.Exists() && occupant.IsOnFoot && occupant.Speed > 2f && !Functions.IsPedVisualLost(occupant))// && occupant.IsRunning)
                {
                    Game.LogTrivial("[RPE Pursuit Update]: Suspect is running on foot");
                    if (occupant.Heading >= 315 || occupant.Heading < 45)
                    {
                        headingDirection = "north";
                    }
                    else if (occupant.Heading >= 45 && occupant.Heading < 135)
                    {
                        headingDirection = "west";
                    }
                    else if (occupant.Heading >= 135 && occupant.Heading < 225)
                    {
                        headingDirection = "south";
                    }
                    else if (occupant.Heading >= 225 && occupant.Heading < 315)
                    {
                        headingDirection = "east";
                    }

                    Game.DisplayNotification(string.Format("~y~Automatic Pursuit Updates~n~~w~Suspect {0} is running ~b~{1} ~w~on ~b~{2}~w~.", i + 1, headingDirection, World.GetStreetName(World.GetStreetHash(occupant.Position))));
                }
            }
            Game.LogTrivial("[RPE Pursuit Update]: Outside of notification loop, pursuit should be over.");
            //Game.DisplaySubtitle("[RPE Pursuit Update]: Notification Updater loop has stopped for suspect " + (i+1));
        }
    }
}
