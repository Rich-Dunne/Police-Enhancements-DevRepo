using Rage;
using LSPD_First_Response.Mod.API;
using System.Linq;
using System.Windows.Forms;
using System;

namespace RichsPoliceEnhancements.Features
{
    internal enum Incident
    {
        Pursuit = 0,
        TrafficStop = 1,
        Callout = 2
    }

    internal static class AmbientBackup
    {
        private static bool _backupOffered = false;
        internal static void Main()
        {
            //LHandle pursuit = null;
            LHandle trafficStop = null;

            Events.OnCalloutAccepted += Events_OnCalloutAccepted;
            while (true)
            {
                //CheckForActivePursuit();
                //if (pursuit != null && !backupOffered)
                //{

                //    PromptForAmbientBackup(Incident.Pursuit);
                //    continue;
                //}

                CheckForActiveTrafficStop();
                if(trafficStop != null && !_backupOffered)
                {
                    PromptForAmbientBackup(Incident.TrafficStop);
                }

                GameFiber.Sleep(100);
            }

            //void CheckForActivePursuit()
            //{
            //    if (Functions.GetActivePursuit() != null && pursuit == null)
            //    {
            //        pursuit = Functions.GetActivePursuit();
            //    }
            //    else if (pursuit != null && Functions.GetActivePursuit() == null)
            //    {
            //        pursuit = null;
            //        backupOffered = false;
            //    }
            //}

            void CheckForActiveTrafficStop()
            {

                if (Functions.GetCurrentPullover() != null && trafficStop == null)
                {
                    trafficStop = Functions.GetCurrentPullover();
                }
                else if (trafficStop != null && Functions.GetCurrentPullover() == null)
                {
                    trafficStop = null;
                    _backupOffered = false;
                }
            }
        }

        private static void Events_OnCalloutAccepted(LHandle handle)
        {
            Game.LogTrivial($"[RPE Ambient Backup]: Callout accepted");
            LHandle pursuit = null;
            CheckForActivePursuit();
            if (pursuit != null && !_backupOffered)
            {
                PromptForAmbientBackup(Incident.Pursuit);
            }

            void CheckForActivePursuit()
            {
                if (Functions.GetActivePursuit() != null && pursuit == null)
                {
                    pursuit = Functions.GetActivePursuit();
                }
                else if (pursuit != null && Functions.GetActivePursuit() == null)
                {
                    pursuit = null;
                    _backupOffered = false;
                }
            }
        }

        private static void PromptForAmbientBackup(Incident incident)
        {
            var startTime = Game.GameTime;
            Game.LogTrivial($"[RPE Ambient Backup]: Displaying backup prompt at {startTime}.");
            var notification = Game.DisplayNotification($"~o~Rich's Police Enhancements~w~\nIf a nearby unit is available during this incident, would you like them to assist you?  Press [~g~Y~w~] to accept or [~r~N~w~] to decline.");
            _backupOffered = true;

            // Accept input for 8 seconds (default time Help message is displayed)
            while (true)
            {
                var currentTime = Game.GameTime;
                if (Math.Abs(currentTime - startTime) >= 8000 || Game.IsKeyDown(Keys.N))
                {
                    Game.LogTrivial($"[RPE Ambient Backup]: Current time: {currentTime}");
                    Game.LogTrivial($"[RPE Ambient Backup]: Player denied backup (intentionally or timed out).");
                    Game.RemoveNotification(notification);
                    notification = Game.DisplayNotification($"~o~Rich's Police Enhancements~w~\nAmbient backup ~r~declined~w~.");

                    return;
                }
                else if (Game.IsKeyDown(Keys.Y))
                {
                    Game.LogTrivial($"[RPE Ambient Backup]: Player accepted backup.");
                    Game.RemoveNotification(notification);
                    notification = Game.DisplayNotification($"~o~Rich's Police Enhancements~w~\nAmbient backup ~g~accepted~w~.  When a nearby unit is available, they will assist you.");
                    CheckForAmbientUnits(incident);
                    break;
                }
                GameFiber.Yield();
            }
        }

        private static void CheckForAmbientUnits(Incident incident)
        {
            while (true)
            {
                var ambientPoliceUnit = GetNearbyPoliceVehicleWithDriver();
                if (ambientPoliceUnit != null)
                {
                    if(incident == Incident.TrafficStop)
                    {
                        GiveAmbientUnitBackupTasks(ambientPoliceUnit);
                        break;
                    }
                    if(incident == Incident.Pursuit)
                    {
                        AddAmbientUnitToPursuit(ambientPoliceUnit);
                        break;
                    }
                }
                GameFiber.Yield();
            }

            Vehicle GetNearbyPoliceVehicleWithDriver()
            {
                foreach (Vehicle vehicle in Game.LocalPlayer.Character.GetNearbyVehicles(16).Where(v => v && v.IsPoliceVehicle && v != Game.LocalPlayer.Character.LastVehicle && v.HasDriver && v.Driver.IsAlive && !Functions.IsPedInPursuit(v.Driver)))
                {
                    return vehicle;
                }
                return null;
            }
        }

        private static void GiveAmbientUnitBackupTasks(Vehicle ambientPoliceUnit)
        {
            GameFiber.StartNew(() =>
            {
                ambientPoliceUnit.IsPersistent = true;
                ambientPoliceUnit.Driver.IsPersistent = true;
                ambientPoliceUnit.Driver.BlockPermanentEvents = true;
                var approachPosition = Game.LocalPlayer.Character.LastVehicle.GetOffsetPosition(new Vector3(0, -15f, 0));
                var backupPosition = Game.LocalPlayer.Character.LastVehicle.GetOffsetPosition(new Vector3(0, -8f, 0));
                var acceptedDistance = GetAcceptedStoppingDistance();

                ambientPoliceUnit.Driver.Tasks.DriveToPosition(approachPosition, 20f, (VehicleDrivingFlags)786868, acceptedDistance);
                while (VehicleAndDriverAreValid() && ambientPoliceUnit.DistanceTo2D(approachPosition) > acceptedDistance)
                {
                    CheckUnitTaskStatus(approachPosition, acceptedDistance);
                    GameFiber.Yield();
                }

                if (!VehicleAndDriverAreValid())
                {
                    return;
                }

                Game.LogTrivial($"[RPE Ambient Backup]: Ambient backup unit is on final approach.");
                ambientPoliceUnit.Driver.Tasks.DriveToPosition(backupPosition, 5f, (VehicleDrivingFlags)786868, acceptedDistance).WaitForCompletion();

                Game.LogTrivial($"[RPE Ambient Backup]: Ambient backup unit is exiting their vehicle.");
                ambientPoliceUnit.Driver.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
            }, "Ambient Backup Task Fiber");

            float GetAcceptedStoppingDistance()
            {
                var dist = (MathHelper.ConvertMetersPerSecondToMilesPerHour(15f) / (250 * 0.8f));
                return MathHelper.Clamp(dist, 2, 10);
            }

            void CheckUnitTaskStatus(Vector3 backupPosition, float acceptedDistance)
            {
                if (ambientPoliceUnit.Driver.Tasks.CurrentTaskStatus == TaskStatus.NoTask)
                {
                    Game.LogTrivial($"[RPE Ambient Backup]: {ambientPoliceUnit.Model.Name} [{ambientPoliceUnit.Handle}] driver [{ambientPoliceUnit.Driver.Handle}] has no task.  Reassiging task.");
                    if (ambientPoliceUnit.Driver.CurrentVehicle)
                    {
                        ambientPoliceUnit.Driver.Tasks.DriveToPosition(backupPosition, 15f, (VehicleDrivingFlags)263088, acceptedDistance);
                    }
                    else
                    {
                        Game.LogTrivial($"[RPE Ambient Backup]: {ambientPoliceUnit.Model.Name} [{ambientPoliceUnit.Handle}] driver [{ambientPoliceUnit.Driver.Handle}] is not in a vehicle.  Exiting loop.");
                        return;
                    }
                }
            }

            bool VehicleAndDriverAreValid()
            {
                if (ambientPoliceUnit == null)
                {
                    Game.LogTrivial($"[RPE Ambient Backup]: ambientPoliceUnit is null");
                    Dismiss();
                    return false;
                }
                if (!ambientPoliceUnit)
                {
                    Game.LogTrivial($"[RPE Ambient Backup]: Vehicle is null");
                    Dismiss();
                    return false;
                }
                if (!ambientPoliceUnit.Driver || !ambientPoliceUnit.Driver.CurrentVehicle || !ambientPoliceUnit.Driver.IsAlive)
                {
                    Game.LogTrivial($"[RPE Ambient Backup]: Driver is null or dead or not in a vehicle");
                    Dismiss();
                    return false;
                }
                return true;
            }

            void Dismiss()
            {
                if (ambientPoliceUnit)
                {
                    ambientPoliceUnit.IsPersistent = false;
                    if (ambientPoliceUnit.Driver)
                    {
                        ambientPoliceUnit.Driver.IsPersistent = false;
                        ambientPoliceUnit.Driver.BlockPermanentEvents = false;
                    }
                }
            }
        }

        private static void AddAmbientUnitToPursuit(Vehicle ambientPoliceUnit)
        {
            if(Functions.GetActivePursuit() != null)
            {
                foreach (Ped occupant in ambientPoliceUnit.Occupants)
                {
                    Functions.AddCopToPursuit(Functions.GetActivePursuit(), occupant);
                }
            }
        }
    }
}
