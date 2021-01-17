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
        internal static void BeginEvent(AmbientEvent @event)
        {
            var attempts = 1;
            var drivers = new List<Ped>();
            var victims = new List<Ped>();
            Game.LogTrivial($"[RPE Ambient Event]: Starting DriveBy event.");

            LoopToFindEventPeds();


            if (@event.EventPeds.Count < 2)
            {
                Game.LogTrivial($"[RPE Ambient Event]: Unable to find suitable event peds after 100 attempts.  Ending event.");
                return;
            }

            GameFiber DriveByInteractionFiber = new GameFiber(() => EventProcess(@event), "RPE DriveBy Interaction Fiber");
            DriveByInteractionFiber.Start();

            void LoopToFindEventPeds()
            {
                while (@event.EventPeds.Count < 2 && attempts <= 100 && !Functions.IsCalloutRunning() && Functions.GetActivePursuit() == null)
                {
                    drivers = FindDrivers();
                    victims = FindVictims();
                    FindEventPedPair();
                    if (@event.EventPeds.Count == 2)
                    {
                        Game.LogTrivial($"Success on attempt {attempts}");
                        break;
                    }
                    @event.EventPeds.Clear();
                    drivers.Clear();
                    victims.Clear();
                    attempts++;
                    GameFiber.Sleep(500);
                }
            }

            List<Ped> NearbyPeds() => World.GetAllPeds().Where(p => PedIsRelevant(p)).ToList();

            bool PedIsRelevant(Ped ped)
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

            List<Ped> FindDrivers()
            {
                return NearbyPeds().Where(p => p.IsInAnyVehicle(false) && (p.RelationshipGroup == RelationshipGroup.AmbientGangBallas || p.RelationshipGroup == RelationshipGroup.AmbientGangFamily || p.RelationshipGroup == RelationshipGroup.AmbientGangMexican)).ToList();
            }

            List<Ped> FindVictims()
            {
                return NearbyPeds().Where(p => !drivers.Contains(p) && drivers.Any(x => p.DistanceTo2D(x) <= 15f) && drivers.Any(x => Math.Abs(x.Position.Z - p.Position.Z) <= 5f) && drivers.Any(x => x.RelationshipGroup != p.RelationshipGroup) && p.CurrentVehicle != drivers.Any(x => x.CurrentVehicle)).ToList();
            }

            void FindEventPedPair()
            {
                // If driver is within 20f of any ped from victims, assign driver and that victim ped as event peds
                var driver = drivers.FirstOrDefault(x => victims.Any(y => y.DistanceTo2D(x) <= 20f));
                if (!driver)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: No drivers found with a suitable victim nearby.");
                    @event.Cleanup();
                    return;
                }
                var victim = victims.FirstOrDefault(x => x.DistanceTo2D(driver) <= 20f);
                if (!victim)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: No victim found within range of the driver.");
                    @event.Cleanup();
                    return;
                }

                new EventPed(@event, driver, Role.PrimarySuspect, true, (BlipSprite)229);
                new EventPed(@event, victim, Role.Victim, false);
            }
        }

        private static void EventProcess(AmbientEvent @event)
        {
            WeaponHash[] weaponPool = { WeaponHash.MicroSMG, WeaponHash.APPistol, WeaponHash.CombatPistol, WeaponHash.Pistol, WeaponHash.Pistol50};
            var driver = @event.EventPeds.FirstOrDefault(x => x.Role == Role.PrimarySuspect);
            var victim = @event.EventPeds.FirstOrDefault(x => x.Role == Role.Victim);
            Functions.SetPedResistanceChance(driver.Ped, 100);
            Game.LogTrivial($"[RPE Ambient Event]: Running DriveBy interaction.");

            if (!driver.Ped || !victim.Ped)
            {
                Game.LogTrivial($"[RPE Ambient Event]: Driver or victim is null.  Ending event.");
                @event.Cleanup();
                return;
            }

            driver.Ped.Tasks.Clear();
            GiveDriverWeapon();
            DriverEngageThenFlee();
            EndEvent(@event);

            void GiveDriverWeapon()
            {
                if (driver.Ped.Inventory.Weapons.Count == 0)
                {
                    Game.LogTrivial($"[RPE Ambient Event] Giving driver random weapon from pool");
                    driver.Ped.Inventory.GiveNewWeapon(weaponPool[new Random().Next(0, weaponPool.Length)], 50, true);
                }
                foreach(WeaponDescriptor weapon in driver.Ped.Inventory.Weapons)
                {
                    Game.LogTrivial($"[RPE Ambient Event] Weapon hash: {weapon.Hash}");
                }
            }

            void DriverEngageThenFlee()
            {
                Game.LogTrivial($"[RPE Ambient Event] Driver shooting at victim");
                Rage.Native.NativeFunction.Natives.x10AB107B887214D8(driver.Ped, victim.Ped, 0); // vehicle shoot task
                driver.Blip.Alpha = 100;

                GameFiber.Sleep(3000);
                driver.Ped.Tasks.Clear();
                driver.Ped.Tasks.CruiseWithVehicle(30f, VehicleDrivingFlags.Emergency);
                Game.LogTrivial($"[RPE Ambient Event] Done assigning tasks.");
            }
        }

        private static void EndEvent(AmbientEvent @event)
        {
            var driver = @event.EventPeds.FirstOrDefault(x => x.Role == Role.PrimarySuspect);
            var victim = @event.EventPeds.FirstOrDefault(x => x.Role == Role.Victim);
            var oldDistance = Game.LocalPlayer.Character.DistanceTo2D(driver.Ped);

            while (true)
            {
                if (!driver.Ped || !victim.Ped || !driver.Ped.IsAlive || !victim.Ped.IsAlive || Functions.IsPedGettingArrested(driver.Ped))
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Driver or victim is null or dead, or driver is arrested.  Ending event.");
                    @event.Cleanup();
                    return;
                }

                if (Game.LocalPlayer.Character.DistanceTo2D(driver.Ped) > 150f)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player is too far away.  Ending event.");
                    @event.Cleanup();
                    return;
                }

                if (Functions.GetActivePursuit() != null && Functions.IsPedInPursuit(driver.Ped))
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player is in pursuit of driver.  Ending event.");
                    @event.Cleanup();
                    return;
                }

                if (Math.Abs(Game.LocalPlayer.Character.DistanceTo2D(driver.Ped) - oldDistance) > 0.15 && Game.LocalPlayer.Character.DistanceTo2D(driver.Ped) > oldDistance && driver.Blip.Alpha > 0f)
                {
                    driver.Blip.Alpha -= 0.001f;
                }
                else if (Math.Abs(Game.LocalPlayer.Character.DistanceTo2D(driver.Ped) - oldDistance) > 0.15 && Game.LocalPlayer.Character.DistanceTo2D(driver.Ped) < oldDistance && driver.Blip.Alpha < 1.0f)
                {
                    driver.Blip.Alpha += 0.01f;
                }
                oldDistance = Game.LocalPlayer.Character.DistanceTo2D(driver.Ped);
                GameFiber.Yield();
            }
        }
    }
}
