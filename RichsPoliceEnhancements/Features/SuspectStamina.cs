using Rage;
using LSPD_First_Response.Mod.API;
using System.Collections.Generic;
using System;
using System.Linq;

namespace RichsPoliceEnhancements.Features
{
    internal class SuspectStamina
    {
        internal static void Main()
        {
            var pursuitPeds = new List<Ped>();
            var timeLastChecked = Game.GameTime;
            LHandle pursuit = null;
            bool L4SpeedSet = false;
            bool L3SpeedSet = false;
            bool L2SpeedSet = false;
            bool L1SpeedSet = false;

            Game.LogTrivial($"[RPE Suspect Stamina]: Suspect stamina started.");
            while (true)
            {
                if (pursuit != null && Functions.GetActivePursuit() == null)
                {
                    Game.LogTrivial($"[RPE Suspect Stamina]: Setting pursuit as null.");
                    pursuit = null;
                    pursuitPeds.Clear();
                }
                if (pursuit == null && Functions.GetActivePursuit() != null && pursuitPeds.Count == 0)
                {
                    Game.LogTrivial($"[RPE Suspect Stamina]: Assigning active pursuit to pursuit handle.");
                    pursuit = Functions.GetActivePursuit();
                    GetPursuitPeds();
                    AssignStamina();
                }

                if(pursuit != null & pursuitPeds.Count > 0)
                {
                    if (Math.Abs(Game.GameTime - timeLastChecked) >= 500)
                    {
                        AdjustStamina();
                        timeLastChecked = Game.GameTime;
                    }
                }
                GameFiber.Yield();
            }

            void GetPursuitPeds()
            {
                foreach (Ped ped in Functions.GetPursuitPeds(Functions.GetActivePursuit()))
                {
                    pursuitPeds.Add(ped);
                }
            }

            void AssignStamina()
            {
                foreach(Ped ped in pursuitPeds)
                {
                    ped.Metadata.Stamina = 100;
                    Rage.Native.NativeFunction.Natives.SET_ENTITY_MAX_SPEED(ped, 6f);
                    Game.LogTrivial($"[RPE Suspect Stamina]: Stamina is {ped.Metadata.Stamina}");
                }
            }

            void AdjustStamina()
            {
                foreach (Ped ped in pursuitPeds.Where(p => p && p.IsOnFoot && !Functions.IsPedArrested(p)))
                {
                    //Game.LogTrivial($"[RPE Suspect Stamina]: Trying to drain stamina for {ped.Model.Name}");
                    if (ped.Speed > 3f && ped.Metadata.Stamina > 0)
                    {
                        ped.Metadata.Stamina -= 1;
                        //Game.LogTrivial($"[RPE Suspect Stamina]: {ped.Model.Name} stamina is now {ped.Metadata.Stamina} (speed: {ped.Speed})");
                    }
                    else if((ped.IsWalking || ped.IsStopped || ped.IsStill || ped.Speed <= 1f) && ped.Metadata.Stamina < 100)
                    {
                        ped.Metadata.Stamina += 1;
                        //Game.LogTrivial($"[RPE Suspect Stamina]: {ped.Model.Name} stamina is now {ped.Metadata.Stamina} (speed: {ped.Speed})");
                    }

                    if(ped.Metadata.Stamina > 75)
                    {
                        if(!L4SpeedSet)
                        {
                            Game.LogTrivial($"[RPE Suspect Stamina]: {ped.Model.Name} stamina is level 4");
                            Rage.Native.NativeFunction.Natives.SET_ENTITY_MAX_SPEED(ped, 6f);
                            L4SpeedSet = true;
                            L3SpeedSet = false;
                            L2SpeedSet = false;
                            L1SpeedSet = false;
                        }
                    }
                    if(ped.Metadata.Stamina <= 75 && ped.Metadata.Stamina > 50)
                    {
                        if(!L3SpeedSet)
                        {
                            Game.LogTrivial($"[RPE Suspect Stamina]: {ped.Model.Name} stamina is level 3");
                            Rage.Native.NativeFunction.Natives.SET_ENTITY_MAX_SPEED(ped, 5f);
                            L4SpeedSet = false;
                            L3SpeedSet = true;
                            L2SpeedSet = false;
                            L1SpeedSet = false;
                        }
                        // move_injured_generic  run
                    }
                    if (ped.Metadata.Stamina <= 50 && ped.Metadata.Stamina > 25)
                    {
                        if(!L2SpeedSet)
                        {
                            Game.LogTrivial($"[RPE Suspect Stamina]: {ped.Model.Name} stamina is level 2");
                            Rage.Native.NativeFunction.Natives.SET_ENTITY_MAX_SPEED(ped, 4f);
                            L4SpeedSet = false;
                            L3SpeedSet = false;
                            L2SpeedSet = true;
                            L1SpeedSet = false;
                        }
                    }
                    if (ped.Metadata.Stamina <= 25)
                    {
                        if(!L1SpeedSet)
                        {
                            Game.LogTrivial($"[RPE Suspect Stamina]: {ped.Model.Name} stamina is level 1");
                            Rage.Native.NativeFunction.Natives.SET_ENTITY_MAX_SPEED(ped, 3f);
                            L4SpeedSet = false;
                            L3SpeedSet = false;
                            L2SpeedSet = false;
                            L1SpeedSet = true;
                        }

                        // Chance to stop and catch breath

                        // Chance to trip
                        if (new Random().Next(100) == 1)
                        {
                            Game.LogTrivial($"[RPE Suspect Stamina]: Suspect tripped");
                            ped.IsRagdoll = true;
                            GameFiber.Sleep(500);
                            if (!ped)
                            {
                                return;
                            }
                            ped.IsRagdoll = false;
                        }
                        // move_injured_generic runtowalk
                    }
                }
            }
        }
    }
}
