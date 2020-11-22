using System;
using System.Collections.Generic;
using Rage;
using LSPD_First_Response.Mod.API;
using System.Linq;

namespace RichsPoliceEnhancements
{
    public class PursuitUpdates
    {
        private static Vehicle suspectVeh;
        private static int NotificationTimer = Settings.PursuitUpdateTimer *= 1000;
        private static string[] arrestedAudio = new string[] {"SUSPECT_APPREHENDED", "SUSPECT_ARRESTED", "SUSPECT_IN_CUSTODY"};

        private enum Direction
        {
            north = 0,
            east = 1,
            south = 2,
            west = 3
        }

        private enum TrafficCondition
        {
            light = 0,
            moderate = 1,
            heavy = 2
        }

        internal static void PursuitUpdateHandler(LHandle pursuit)
        {
            List<Vehicle> vehiclesList = new List<Vehicle>();

            Game.LogTrivial($"[RPE Pursuit Update]: Pursuit started");
            GameFiber.StartNew(() =>
            {
                GameFiber.Sleep(5000);
                if(Functions.GetActivePursuit() == null)
                {
                    Game.LogTrivial($"GetActivePursuit is null.");
                    return;
                }

                suspectVeh = Functions.GetPursuitPeds(pursuit).Where(p => p && p.IsAlive && !Functions.IsPedACop(p) && p.IsInAnyVehicle(false)).FirstOrDefault()?.CurrentVehicle;

                if (suspectVeh)
                {
                    for(int i = 0; i < suspectVeh.Occupants.Count(); i++)
                    {
                        var occupant = suspectVeh.Occupants[i];
                        if (occupant && occupant.IsAlive && Functions.IsPedInPursuit(occupant))
                        {
                            GameFiber NotificationUpdaterFiber = new GameFiber(() => NotificationUpdater(occupant, vehiclesList, i));
                            NotificationUpdaterFiber.Start();
                        }
                        GameFiber.Sleep(1000);
                    }
                }
                else
                {
                    Game.LogTrivial($"[RPE Pursuit Update]: suspectVeh is null.");
                }
            });
        }

        private static void NotificationUpdater(Ped suspect, List<Vehicle> vehiclesList, int suspectNumber)
        {
            bool suspectVisualLost = false;
            suspectNumber++;
            Game.LogTrivial($"[RPE Pursuit Update]: Notifications running for suspect #{suspectNumber}");

            while (true)
            {
                if (!suspect)
                {
                    Game.LogTrivial($"[RPE Pursuit Update]: Suspect is invalid.  Stopping notifications.");
                    break;
                }
                if (!suspect.IsAlive)
                {
                    Game.LogTrivial($"[RPE Pursuit Update]: Suspect is dead.  Stopping notifications.");
                    Functions.PlayScannerAudio($"ATTENTION_ALL_UNITS SUSPECT_NEUTRALIZED");
                    break;
                }
                if(Functions.IsPedGettingArrested(suspect) || Functions.IsPedArrested(suspect))
                {
                    Game.LogTrivial($"[RPE Pursuit Update]: Suspect is arrested.  Stopping notifications.");
                    Functions.PlayScannerAudio($"ATTENTION_ALL_UNITS {arrestedAudio[new Random().Next(arrestedAudio.Length)]}");
                    break;
                }
                if (suspectVisualLost && !Functions.IsPedVisualLost(suspect))
                {
                    suspectVisualLost = false;
                }
                if (Functions.IsPedVisualLost(suspect))
                {
                    if (!suspectVisualLost)
                    {
                        Functions.PlayScannerAudio($"ATTENTION_ALL_UNITS EVADED ALL_UNITS_ATTEMPT_TO_REAQUIRE");
                        suspectVisualLost = true;
                    }
                    Game.LogTrivial($"[RPE Pursuit Update]: Visual lost for suspect.  No notifications will be given until visual is regained.");
                    GameFiber.Sleep(NotificationTimer);
                    continue;
                }

                if(suspect && !Functions.IsPedVisualLost(suspect))
                {
                    var direction = GetSuspectDirection();
                    var streetName = World.GetStreetName(World.GetStreetHash(suspect.Position));
                    var speed = Math.Round(MathHelper.ConvertMetersPerSecondToMilesPerHour(suspect.Speed));

                    if (suspect.CurrentVehicle && suspect.CurrentVehicle.Driver == suspect)
                    {
                        var trafficCondition = GetTrafficCondition();
                        var trafficConditionColor = GetTrafficConditionColor();
                        
                        Game.LogTrivial($"[RPE Pursuit Update]: Suspect {suspectNumber} is heading {(Direction)direction} on {streetName}.  Speeds are {speed}mph, traffic is {(TrafficCondition)trafficCondition}.");
                        Game.DisplayNotification($"~y~Automatic Pursuit Updates\n~w~Suspect {suspectNumber} is heading ~b~{(Direction)direction} ~w~on ~b~{streetName}~w~.  Speeds are ~b~{speed}mph~w~, traffic is {trafficConditionColor}{(TrafficCondition)trafficCondition}~w~.");

                        if (Settings.DispatchUpdates && suspect.CurrentVehicle.Speed > 0f)
                        {
                            var attentionAudio = $"ATTENTION_ALL_UNITS_0{new Random().Next(1, 21)}";
                            var headingAudio = $"SUSPECT_HEADING_0{new Random().Next(1, 4)}";
                            var directionAudio = $"DIRECTION_BOUND_{((Direction)direction).ToString().ToUpper()}";
                            var streetAudio = $"STREET_{streetName.Replace(" ", "_").ToUpper()}";
                            Functions.PlayScannerAudio($"{attentionAudio} {headingAudio} {directionAudio} ON_03 {streetAudio}");
                        }
                    }
                    else if (suspect.IsOnFoot && suspect.Speed > 2f)
                    {
                        Game.LogTrivial($"[RPE Pursuit Update]: Suspect {suspectNumber} is running {(Direction)direction} on {streetName}.");
                        Game.DisplayNotification($"~y~Automatic Pursuit Updates\n~w~Suspect {suspectNumber} is running ~b~{(Direction)direction} ~w~on ~b~{streetName}~w~.");
                    }
                }

                if(Functions.GetActivePursuit() == null)
                {
                    Game.LogTrivial($"[RPE Pursuit Update]: Pursuit is over.  Stopping notifications.");
                    break;
                }
                GameFiber.Sleep(NotificationTimer);
            }

            int GetSuspectDirection()
            {
                if (suspect.Heading >= 315 || suspect.Heading < 45)
                {
                    return 0;
                }
                else if (suspect.Heading >= 45 && suspect.Heading < 135)
                {
                    return 3;
                }
                else if (suspect.Heading >= 135 && suspect.Heading < 225)
                {
                    return 2;
                }
                else if (suspect.Heading >= 225 && suspect.Heading < 315)
                {

                    return 1;
                }
                return -1;
            }

            int GetTrafficCondition()
            {
                if (!suspect.CurrentVehicle)
                {
                    Game.LogTrivial($"[RPE Pursuit Update]: Cannot get traffic condition, suspect is on foot.");
                    return -1;
                }

                if (vehiclesList.Count > 0)
                {
                    vehiclesList.Clear();
                }
                foreach (Vehicle v in suspect.CurrentVehicle?.Driver.GetNearbyVehicles(16))
                {
                    if (v && v.HasDriver && !v.IsPoliceVehicle && v != suspectVeh)
                    {
                        //Game.LogTrivial("[RPE Pursuit Update]: Added nearby vehicle to list");
                        vehiclesList.Add(v);
                    }
                    else
                    {
                        //Game.LogTrivial("[RPE Pursuit Update]: Vehicle found but not added");
                    }
                }

                if (vehiclesList.Count < 6)
                {
                    return 0;
                }
                else if (vehiclesList.Count >= 6 && vehiclesList.Count < 12)
                {
                    return 1;
                }
                else if (vehiclesList.Count >= 12)
                {
                    return 2;
                }
                return -1;
            }

            string GetTrafficConditionColor()
            {
                if (GetTrafficCondition() == 0)
                {
                    return "~g~";
                }
                else if (GetTrafficCondition() == 1)
                {
                    return "~o~";
                }
                else if (GetTrafficCondition() == 2)
                {
                    return "~r~";
                }
                return "~w~";
            }
        }
    }
}
