﻿using Rage;
using LSPD_First_Response.Mod.API;
using System.Linq;
using System.Windows.Forms;
using System;
using RichsPoliceEnhancements.Utils;

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
        private static bool BackupOffered { get; set; } = false;

        internal static void Main()
        {
            Events.OnCalloutAccepted += Events_OnCalloutAccepted;

            while (true)
            {
                GameFiber.Yield();
                if (Functions.GetCurrentPullover() == null && Functions.GetActivePursuit() == null)
                {
                    BackupOffered = false;
                }

                if (Functions.GetActivePursuit() != null && !BackupOffered)
                {
                    if (Settings.AlwaysAcceptAmbientBackup)
                    {
                        CheckForAmbientUnits(Incident.Pursuit);
                    }
                    else
                    {
                        PromptForAmbientBackup(Incident.Pursuit);
                    }
                    continue;
                }

                //if (Functions.GetCurrentPullover() != null && !BackupOffered)
                //{
                //    if (Settings.AlwaysAcceptAmbientBackup)
                //    {
                //        CheckForAmbientUnits(Incident.TrafficStop);
                //    }
                //    else
                //    {
                //        PromptForAmbientBackup(Incident.TrafficStop);
                //    }
                //}

                GameFiber.Sleep(100);
            }
        }

        private static void Events_OnCalloutAccepted(LHandle handle)
        {
            Game.LogTrivial($"[RPE Ambient Backup]: Callout accepted");

            if (Functions.GetActivePursuit() != null && !BackupOffered)
            {
                if (Settings.AlwaysAcceptAmbientBackup)
                {
                    Game.LogTrivial($"[RPE Ambient Backup]: Always accept ambient backup.");
                    CheckForAmbientUnits(Incident.Pursuit);
                }
                else
                {
                    PromptForAmbientBackup(Incident.Pursuit);
                }
            }
        }

        private static void PromptForAmbientBackup(Incident incident)
        {
            GameFiber.Sleep(100);
            var startTime = Game.GameTime;
            Game.LogTrivial($"[RPE Ambient Backup]: Displaying backup prompt at {startTime}.");
            var notification = Game.DisplayNotification($"~o~Rich's Police Enhancements~w~\nIf a nearby unit is available during this incident, would you like them to assist you?  Press [~g~Y~w~] to accept or [~r~N~w~] to decline.");
            BackupOffered = true;

            // Accept input for 8 seconds (default time Help message is displayed)
            while (true)
            {
                var currentTime = Game.GameTime;
                if (Math.Abs(currentTime - startTime) >= 8000 || Game.IsKeyDown(Keys.N))
                {
                    //Game.LogTrivial($"[RPE Ambient Backup]: Current time: {currentTime}");
                    Game.LogTrivial($"[RPE Ambient Backup]: Player denied backup (intentionally or timed out).");
                    Game.RemoveNotification(notification);
                    Game.DisplayNotification($"~o~Rich's Police Enhancements~w~\nAmbient backup ~r~declined~w~.");

                    return;
                }
                else if (Game.IsKeyDown(Keys.Y))
                {
                    Game.LogTrivial($"[RPE Ambient Backup]: Player accepted backup.");
                    Game.RemoveNotification(notification);
                    Game.DisplayNotification($"~o~Rich's Police Enhancements~w~\nAmbient backup ~g~accepted~w~.  When a nearby unit is available, they will assist you.");
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
                    //if(incident == Incident.TrafficStop)
                    //{
                    //    GameFiber.StartNew(() => GiveAmbientUnitBackupTasks(ambientPoliceUnit), "Ambient Backup Task Fiber");
                    //    break;
                    //}
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
                foreach (Vehicle vehicle in Game.LocalPlayer.Character.GetNearbyVehicles(16).Where(v => v && v.IsPoliceVehicle && v != Game.LocalPlayer.Character.LastVehicle && v.HasDriver && v.Driver.IsAlive && v.Driver.IsAmbient() && !Functions.IsPedInPursuit(v.Driver)))
                {
                    return vehicle;
                }
                return null;
            }
        }

        //private static void GiveAmbientUnitBackupTasks(Vehicle ambientPoliceUnit)
        //{
        //    ambientPoliceUnit.IsPersistent = true;
        //    ambientPoliceUnit.Driver.IsPersistent = true;
        //    ambientPoliceUnit.Driver.BlockPermanentEvents = true;
        //    var ambientUnit = ambientPoliceUnit.Driver;
        //    var approachPosition = Game.LocalPlayer.Character.LastVehicle.GetOffsetPosition(new Vector3(0, -15f, 0)); // Null ref
        //    var backupPosition = Game.LocalPlayer.Character.LastVehicle.GetOffsetPosition(new Vector3(0, -8f, 0));
        //    var acceptedDistance = GetAcceptedStoppingDistance();

        //    ambientPoliceUnit.Driver.Tasks.DriveToPosition(approachPosition, 20f, (VehicleDrivingFlags)786868, acceptedDistance);
        //    while (VehicleAndDriverAreValid() && ambientPoliceUnit.DistanceTo2D(approachPosition) > acceptedDistance)
        //    {
        //        CheckUnitTaskStatus();
        //        GameFiber.Yield();
        //    }

        //    if (!VehicleAndDriverAreValid())
        //    {
        //        return;
        //    }

        //    Game.LogTrivial($"[RPE Ambient Backup]: Ambient backup unit is on final approach.");
        //    ambientPoliceUnit.Driver.Tasks.DriveToPosition(backupPosition, 5f, (VehicleDrivingFlags)786868, acceptedDistance).WaitForCompletion();
        //    if (!VehicleAndDriverAreValid())
        //    {
        //        return;
        //    }

        //    Game.LogTrivial($"[RPE Ambient Backup]: Ambient backup unit is exiting their vehicle.");
        //    ambientPoliceUnit.Driver.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
        //    while(ambientUnit && ambientPoliceUnit && ambientUnit.IsInVehicle(ambientPoliceUnit, false))
        //    {
        //        GameFiber.Yield();
        //    }
        //    if(!ambientUnit)
        //    {
        //        Game.LogTrivial($"Ambient unit is invalid.");
        //        return;
        //    }

        //    if(Game.LocalPlayer.Character.Inventory.EquippedWeapon != null)
        //    {
        //        ambientUnit.Inventory.EquippedWeapon = ambientUnit.Inventory.Weapons.First();
        //        if(Game.LocalPlayer.Character.IsAiming)
        //        {
        //            ambientUnit.Tasks.AimWeaponAt(Game.LocalPlayer.GetFreeAimingTarget(),-1);
        //        }
        //    }
        //    else
        //    {
        //        ambientUnit.Tasks.PlayAnimation("amb@world_human_cop_idles@male@base","base",1.0f,AnimationFlags.Loop); // Not working or not the right animation
        //    }
        //    Game.DisplayNotification($"~o~Rich's Police Enhancements~w~\nAmbient backup has arrived.  Convert them to a partner with ~y~Ultimate Backup ~w~to interact with them.");

        //    float GetAcceptedStoppingDistance()
        //    {
        //        var dist = (MathHelper.ConvertMetersPerSecondToMilesPerHour(15f) / (250 * 0.8f));
        //        return MathHelper.Clamp(dist, 2, 10);
        //    }

        //    void CheckUnitTaskStatus()
        //    {
        //        if (ambientPoliceUnit.Driver.Tasks.CurrentTaskStatus == TaskStatus.NoTask)
        //        {
        //            Game.LogTrivial($"[RPE Ambient Backup]: {ambientPoliceUnit.Model.Name} [{ambientPoliceUnit.Handle}] driver [{ambientPoliceUnit.Driver.Handle}] has no task.  Reassiging task.");
        //            if (ambientPoliceUnit.Driver.CurrentVehicle)
        //            {
        //                ambientPoliceUnit.Driver.Tasks.DriveToPosition(approachPosition, 15f, (VehicleDrivingFlags)263088, acceptedDistance);
        //            }
        //            else
        //            {
        //                Game.LogTrivial($"[RPE Ambient Backup]: {ambientPoliceUnit.Model.Name} [{ambientPoliceUnit.Handle}] driver [{ambientPoliceUnit.Driver.Handle}] is not in a vehicle.  Exiting loop.");
        //                return;
        //            }
        //        }
        //    }

        //    bool VehicleAndDriverAreValid()
        //    {
        //        if (ambientPoliceUnit == null || !ambientPoliceUnit)
        //        {
        //            Game.LogTrivial($"[RPE Ambient Backup]: ambientPoliceUnit is null");
        //            Dismiss();
        //            return false;
        //        }

        //        if (!ambientPoliceUnit.Driver || !ambientPoliceUnit.Driver.CurrentVehicle || !ambientPoliceUnit.Driver.IsAlive)
        //        {
        //            Game.LogTrivial($"[RPE Ambient Backup]: Driver is null or dead or not in a vehicle");
        //            Dismiss();
        //            return false;
        //        }
        //        return true;
        //    }

        //    void Dismiss()
        //    {
        //        if (ambientPoliceUnit)
        //        {
        //            ambientPoliceUnit.IsPersistent = false;
        //            if (ambientPoliceUnit.Driver)
        //            {
        //                ambientPoliceUnit.Driver.IsPersistent = false;
        //                ambientPoliceUnit.Driver.BlockPermanentEvents = false;
        //            }
        //        }
        //    }
        //}

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
