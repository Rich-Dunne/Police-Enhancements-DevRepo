﻿using Rage;
using System;

namespace RichsPoliceEnhancements.Utils
{
    /// <summary>Differen ped categories 
    /// </summary>
    internal enum PedType
    {
        /// <summary>Any ped
        /// </summary>
        Any = 0,
        /// <summary>Cop peds 
        /// </summary>
        Cop = 1,
        //Firefigher = 2,
        //EMS = 3
    }

    /// <summary>A collection of potentially useful code snippets for GTA/LSPDFR development. 
    /// </summary>
    internal static class Extensions
    {
        /// <summary>Determines if a ped can be considered ambient.  Checks any type of ped by default.
        /// </summary>
        internal static bool IsAmbient(this Ped ped, PedType pedType = 0)
        {
            // Universal tasks (virtually all peds seem have this)
            var taskAmbientClips = Rage.Native.NativeFunction.Natives.GET_IS_TASK_ACTIVE<bool>(ped, 38);

            // Universal on-foot tasks (virtually all ambient walking peds seem to have this)
            var taskComplexControlMovement = Rage.Native.NativeFunction.Natives.GET_IS_TASK_ACTIVE<bool>(ped, 35);

            // Universal in-vehicle tasks (virtually all ambient driver peds seem to have this)
            var taskInVehicleBasic = Rage.Native.NativeFunction.Natives.GET_IS_TASK_ACTIVE<bool>(ped, 150);
            var taskCarDriveWander = Rage.Native.NativeFunction.Natives.GET_IS_TASK_ACTIVE<bool>(ped, 151);

            // On-foot ambient tasks
            var taskPolice = Rage.Native.NativeFunction.Natives.GET_IS_TASK_ACTIVE<bool>(ped, 58); // From ambient cop (non-freemode) walking around
            var taskWanderingScenario = Rage.Native.NativeFunction.Natives.GET_IS_TASK_ACTIVE<bool>(ped, 100); // From ambient cop walking around
            var taskUseScenario = Rage.Native.NativeFunction.Natives.GET_IS_TASK_ACTIVE<bool>(ped, 118); // From ambient cop standing still
            var taskScriptedAnimation = Rage.Native.NativeFunction.Natives.GET_IS_TASK_ACTIVE<bool>(ped, 134); // From UB ped waiting for interaction

            // In-vehicle controlled tasks
            var taskControlVehicle = Rage.Native.NativeFunction.Natives.GET_IS_TASK_ACTIVE<bool>(ped, 169); // From backup unit driving to player

            // If ped relationship group does not contain "cop" then this extension doesn't apply
            if (pedType == PedType.Cop && !ped.RelationshipGroup.Name.ToLower().Contains("cop"))
            {
                //Game.LogTrivial($"Ped does not belong to a cop relationship group.");
                return false;
            }

            // Ped is in a vehicle
            if (taskInVehicleBasic)
            {
                //Game.LogTrivial($"Ped is in a vehicle.");
                // Ped has a controlled driving task
                if (taskControlVehicle)
                {
                    //Game.LogTrivial($"Ped has a controlled driving task. (non-ambient)");
                    return false;
                }

                // Ped has a wander driving task
                if (taskCarDriveWander)
                {
                    //Game.LogTrivial($"Ped has a wander driving task. (ambient)");
                    return true;
                }

                // If the ped is in a vehicle but doesn't have a driving task, then it's a passenger.  Check if the vehicle's driver has a driving wander task
                if (ped.CurrentVehicle && ped.CurrentVehicle.Driver)
                {
                    var driverHasWanderTask = Rage.Native.NativeFunction.Natives.GET_IS_TASK_ACTIVE<bool>(ped.CurrentVehicle.Driver, 151);
                    if (driverHasWanderTask)
                    {
                        //Game.LogTrivial($"[Ambient Ped Check]: Ped is a passenger.  Vehicle's driver has a wander driving task. (ambient)");
                        return true;
                    }
                }

                if (taskAmbientClips)
                {
                    //Game.LogTrivial($"Ped only has InVehicleBasic and AmbientClips (ambient)");
                    return true;
                }
            }

            if (ped.IsOnFoot)
            {
                // UB unit on-foot, waiting for interaction
                if (ped.RelationshipGroup.Name == "UBCOP")
                {
                    //Game.LogTrivial($"Cop is UB unit. (non-ambient)");
                    return false;
                }

                // Cop ped walking around or standing still
                if ((taskComplexControlMovement && taskWanderingScenario) || (taskAmbientClips && taskUseScenario))
                {
                    //Game.LogTrivial($"Ped is wandering around or standing still. (ambient)");
                    return true;
                }
            }

            // If nothing else returns true before now, then the ped is probably being controlled and doing something else
            //Game.LogTrivial($"Nothing else has returned true by this point. (non-ambient)");
            return false;
        }

        /// <summary>Gets an offset Vector3 position from the current position.
        /// </summary>
        internal static Vector3 GetOffset(this Vector3 from, float heading, Vector3 offset)
        {
            float radians = MathHelper.ConvertDegreesToRadians(heading);

            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            float resultX = offset.X * cos - offset.Y * sin;
            float resultY = offset.X * sin + offset.Y * cos;

            return new Vector3(from.X + resultX, from.Y + resultY, from.Z + offset.Z);
        }

        /// <summary>Determines if the entity is within the bounds of the closest road node for the same side of the road as the entity.
        /// </summary>
        internal static bool WithinNearbyRoadNode(this Entity entity, Vector3 nodePosition, float nodeHeading) // IsOnRightSideRoad
        {
            return (360 + GetHeadingToTarget() - nodeHeading) % 360 > 180 ? true : false;

            float GetHeadingToTarget()
            {
                Vector3 direction = entity.Position - nodePosition;
                var heading = MathHelper.NormalizeHeading(MathHelper.ConvertDirectionToHeading(direction));
                return (float)Math.Truncate(heading);
            }
        }

        /// <summary>Determines if the position is within the bounds of the closest road node for the same side of the road as the position.
        /// </summary>
        internal static bool WithinNearbyRoadNode(this Vector3 position, Vector3 nodePosition, float nodeHeading) // IsOnRightSideRoad
        {
            return (360 + GetHeadingToTarget() - nodeHeading) % 360 > 180 ? true : false;

            float GetHeadingToTarget()
            {
                Vector3 direction = position - nodePosition;
                var heading = MathHelper.NormalizeHeading(MathHelper.ConvertDirectionToHeading(direction));
                return (float)Math.Truncate(heading);
            }
        }

        /// <summary>Inverts a given heading.
        /// </summary>
        internal static float Invert(this float f)
        {
            if (f >= 180)
            {
                return f - 180;
            }
            else
            {
                return f + 180;
            }
        }

        /// <summary>Determines if a ped meets the criteria for being usable in any ambient event.
        /// </summary>
        internal static bool IsRelevantForAmbientEvent(this Ped ped)
        {
            if (ped && ped.IsAlive && ped.Position.DistanceTo(Game.LocalPlayer.Character.Position) < 100f && !ped.IsPlayer && !ped.IsInjured && !ped.Model.Name.Contains("A_C") && ped.RelationshipGroup != RelationshipGroup.Cop)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Ensures a modified heading is within 360 degrees.
        /// </summary>
        internal static float Normalize(this float heading)
        {
            if (heading < 0)
            {
                return 360 - (Math.Abs(0 - heading));
            }
            else if (heading > 360)
            {
                return 0 + (Math.Abs(360 - heading));
            }
            else
            {
                return heading;
            }
        }

        /// <summary>
        /// Makes the vehicle honk it's horn for the given duration (in ms).
        /// </summary>
        internal static void HonkHorn(this Vehicle vehicle, int duration, bool heldDown = false, bool forever = false)
        {
            string mode = !heldDown ? "NORMAL" : "HELDDOWN";
            NativeMethods.StartVehicleHorn(vehicle, duration, mode, forever);
        }

        internal static void FaceEntity(this Ped ped, Entity entity, int duration)
        {
            NativeMethods.TaskTurnPedToFaceEntity(ped, entity, duration);
        }

        internal static void ShootFromVehicle(this Ped ped, Ped target, float unknown = 0)
        {
            NativeMethods.TaskVehicleShootAtPed(ped, target, unknown);
        }

        internal static void MaxSpeed(this Entity entity, float speed)
        {
            NativeMethods.SetEntityMaxSpeed(entity, speed);
        }
    }
}
