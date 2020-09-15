using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;

namespace RichsPoliceEnhancements
{
    class DriveByEventFunctions
    {
        public static void RunEventFunctions(Ped player, List<Ped> nearbyPeds, List<EventPed> eventPeds, List<EventVehicle> eventVehicles, List<Blip> eventBlips)
        {
            Game.LogTrivial($"[Rich Ambiance] Starting DriveBy event.");
            if (FindDriver(nearbyPeds, eventPeds) != null && FindTarget(nearbyPeds, eventPeds, eventBlips) != null)
            {
                DriveByInteraction(eventPeds, eventVehicles, eventBlips);
            }
            else
            {
                Game.LogTrivial($"[Rich Ambiance] Driver or target returned null.  Ending event.");
            }
        }

        private static Ped FindDriver(List<Ped> pedList, List<EventPed> eventPeds)
        {
            foreach (Ped p in pedList.Where(p => p.IsInAnyVehicle(false) && p.CurrentVehicle.IsCar && p.CurrentVehicle.Driver == p))
            {
                if (p.Exists() && p.IsValid() && p.IsAlive && (p.RelationshipGroup == RelationshipGroup.AmbientGangBallas || p.RelationshipGroup == RelationshipGroup.AmbientGangFamily || p.RelationshipGroup == RelationshipGroup.AmbientGangMexican))
                {
                    Game.LogTrivial($"[Rich Ambiance] Driver relationship group: {p.RelationshipGroup.Name}");
                    Ped driver = p;
                    driver.IsPersistent = true;
                    driver.BlockPermanentEvents = true;
                    //Blip blip = driver.AttachBlip();
                    //blip.Color = Color.Red;
                    //blips.Add(blip);
                    eventPeds.Add(new EventPed("DriveBy", driver));
                    return driver;
                }
            }
            Game.LogTrivial($"[Rich Ambiance] Could not find driver.");
            return null;
        }

        private static Ped FindTarget(List<Ped> pedList, List<EventPed> eventPeds, List<Blip> eventBlips)
        {
            Ped driver = eventPeds[0].Ped;
            Ped target;
            //List<Ped> sortedList = pedList.OrderBy(p => p.DistanceTo(driver) <= 10f).ToList();

            foreach (Ped p in pedList)
            {
                if (p.DistanceTo(driver.GetOffsetPositionFront(15f)) <= 20f && Math.Abs(driver.Position.Z - p.Position.Z) <= 10f && p.RelationshipGroup != driver.RelationshipGroup)
                {
                    //Game.LogTrivial($"Target relationship group: {p.RelationshipGroup.Name}");
                    // Ped Settings
                    target = p;
                    target.IsPersistent = true;
                    target.BlockPermanentEvents = true;

                    // Blip Settings
                    Blip blip = target.AttachBlip();
                    blip.Color = Color.White;
                    eventBlips.Add(blip);


                    eventPeds.Add(new EventPed("DriveBy", target));
                    Game.LogTrivial($"[Rich Ambiance] Target found.");
                    return target;
                }
            }
            Game.LogTrivial($"[Rich Ambiance] Could not find target.");
            return null;
        }

        private static void DriveByInteraction(List<EventPed> eventPeds, List<EventVehicle> eventVehicles, List<Blip> eventBlips)
        {
            WeaponHash[] weaponPool = { WeaponHash.MicroSMG, WeaponHash.APPistol, WeaponHash.CombatPistol, WeaponHash.Pistol, WeaponHash.Pistol50, WeaponHash.Smg };
            Ped driver = eventPeds[0].Ped;
            Ped target = eventPeds[1].Ped;
            driver.Tasks.Clear();

            if(driver.Inventory.Weapons.Count == 0)
            {
                Game.LogTrivial($"[Rich Ambiance] Giving ped random weapon from pool");
                driver.Inventory.GiveNewWeapon(weaponPool[new Random().Next(0, weaponPool.Length)], 50, true);
            }

            foreach(WeaponDescriptor w in driver.Inventory.Weapons)
            {
                Game.LogTrivial($"[Rich Ambiance] Weapon hash: {w.Hash.ToString()}");
            }

            //Game.LogTrivial($"Roll down windows");
            //Rage.Native.NativeFunction.Natives.x7AD9E6CE657D69E3(driver.Ped.CurrentVehicle, 0); // roll driver window down
            //Rage.Native.NativeFunction.Natives.x7AD9E6CE657D69E3(driver.Ped.CurrentVehicle, 1); // roll passenger window down
            //Rage.Native.NativeFunction.Natives.x9E5B5E4D2CCD2259(driver.Ped.CurrentVehicle, 0); // smash window

            Game.LogTrivial($"[Rich Ambiance] Drive to target's location");
            driver.Tasks.DriveToPosition(target.Position, 20f, VehicleDrivingFlags.Normal);
            //driver.Ped.Tasks.ChaseWithGroundVehicle(target.Ped);

            Game.LogTrivial($"[Rich Ambiance] Ped shooting at target");
            Rage.Native.NativeFunction.Natives.x10AB107B887214D8(driver,target,0); // vehicle shoot task

            if (Settings.EventBlips)
            {
                Blip blip = driver.AttachBlip();
                blip.Sprite = BlipSprite.GangVehicle;
                blip.Color = Color.Red;
                eventBlips.Add(blip);
            }

            GameFiber.Sleep(3000);
            driver.Tasks.Clear();
            driver.Tasks.CruiseWithVehicle(30f, VehicleDrivingFlags.Emergency);
            Game.LogTrivial($"[Rich Ambiance] Done assigning tasks.");

            // We run this loop so the event stays active until something happens.  Don't want to start another event while the player is still interacting with this one.
            while (!AmbientEvent.PrematureEndCheck(eventPeds, eventVehicles))
            {
                GameFiber.Yield();
            }
        }
    }
}
