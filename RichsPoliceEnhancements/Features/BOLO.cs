using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Engine.Scripting.Entities;
using System.IO;

namespace RichsPoliceEnhancements
{
    // TODO:
    // - Camera focus on BOLO if nearby
    internal class BOLO
    {
        private static bool BOLOActive = false;
        private enum Direction
        {
            north = 0,
            east = 1,
            south = 2,
            west = 3
        }

        //BOLO is in regards to...
        private static string[] boloReasons = {
            "a warrant out of Los Santos County",
            "a warrant out of Blaine County",
            "a citizen traffic complaint",
            "an individual who recently left a physical disturbance",
            "an individual who recently left a verbal disturbance",
            "an individual who recently left the scene of a crime",
            "an individual who recently shoplifted from a gas station",
            "an individual who recently shoplifted from a liquor store",
            "an individual who recently shoplifted from Binco Clothes",
            "an individual who recently menaced the reporting party",
            "suspicious circumstances in the area",
            "the registered owner is wanted for questioning by LSPD regarding a homicide",
            "the registered owner is wanted for questioning by LSSD regarding a homicide",
            "the registered owner is wanted for questioning by BCSO regarding a homicide",
            "the registered owner is wanted for questioning by LSPD regarding an assault",
            "the registered owner is wanted for questioning by LSSD regarding an assault",
            "the registered owner is wanted for questioning by BCSO regarding an assault",
            "the registered owner is wanted for questioning by LSPD regarding a burglary",
            "the registered owner is wanted for questioning by LSSD regarding a burglary",
            "the registered owner is wanted for questioning by BCSO regarding a burglary",
            "the registered owner is wanted for questioning by LSPD regarding an armed robbery",
            "the registered owner is wanted for questioning by LSSD regarding an armed robbery",
            "the registered owner is wanted for questioning by BCSO regarding an armed robbery",
            "a usual occupant wanted for questioning by LSPD regarding a homicide",
            "a usual occupant wanted for questioning by LSSD regarding a homicide",
            "a usual occupant wanted for questioning by BCSO regarding a homicide",
            "a usual occupant wanted for questioning by LSPD regarding an assault",
            "a usual occupant wanted for questioning by LSPD regarding an assault at the Vanilla Unicorn",
            "a usual occupant wanted for questioning by LSSD regarding an assault",
            "a usual occupant wanted for questioning by BCSO regarding an assault",
            "a usual occupant wanted for questioning by BCSO regarding an assault at the Hen House",
            "a usual occupant wanted for questioning by BCSO regarding an assault at the Yellow Jacket",
            "a usual occupant wanted for questioning by LSPD regarding a burglary",
            "a usual occupant wanted for questioning by LSSD regarding a burglary",
            "a usual occupant wanted for questioning by BCSO regarding a burglary",
            "a usual occupant wanted for questioning by LSPD regarding an armed robbery",
            "a usual occupant wanted for questioning by LSPD regarding an armed robbery at Vangelico",
            "a usual occupant wanted for questioning by LSSD regarding an armed robbery",
            "the vehicle being seen leaving the area of a homicide in Los Santos",
            "the vehicle being seen leaving the area of a homicide in Los Santos County",
            "the vehicle being seen leaving the area of a homicide in Blaine County",
            "the vehicle being seen leaving the area of an assault at the Yellow Jacket",
            "the vehicle being seen leaving the area of an assault at the Hen House",
            "the vehicle being seen leaving the area of an assault at the Vanilla Unicorn",
            "the vehicle being seen leaving the area of an armed robbery in Los Santos",
            "the vehicle being seen leaving the scene of an armed robbery at Dollar Pills in South LS",
            "the vehicle being seen leaving the scene of an armed robbery at the Innocence Blvd 24/7 store",
            "the vehicle being seen leaving the scene of an armed at the Pawn & Jewelry",
            "the vehicle being seen leaving the scene of an armed robbery at Vangelico",
            "the vehicle being seen leaving the scene of an armed robbery in Los Santos County",
            "the vehicle being seen leaving the scene of an armed robbery at the 24/7 store in Chumash",
            "the vehicle being seen leaving the scene of an armed robbery in Blaine County",
            "the vehicle being seen leaving the scene of an armed robbery at the Route 68 24/7 store",
            "the vehicle being seen leaving the scene of an armed robbery at Liquor Ace",
            "the vehicle reportedly driving off from the LTD station in South LS without paying for gas",
            "the vehicle reportedly driving off from the LTD station in Mirror Park without paying for gas",
            "the vehicle reportedly driving off from the LTD station in Banham Canyon without paying for gas",
            "the vehicle reportedly driving off from the Grapeseed LTD station without paying for gas",
            "the vehicle reportedly driving off from the Tataviam Truckstop without paying for gas",
            "the vehicle reportedly driving off from Perth St RON Station without paying for gas",
            "the vehicle reportedly driving off from Mcdonald St RON Station without paying for gas",
            "the vehicle reportedly driving off from Popular St RON Station without paying for gas",
            "the vehicle reportedly driving off from El Rancho Blvd  RON Station without paying for gas",
            "the vehicle reportedly driving off from Paleto Bay RON Station without paying for gas",
            "the driver reportedly stealing several items from the South LS LTD station",
            "the driver reportedly stealing several items from the Mirror Park LTD station",
            "the driver reportedly stealing several items and fled from the Banham Canyon LTD station",
            "the driver reportedly stealing several items from the Grapeseed LTD station",
            "the driver reportedly stealing several items from the Perth St RON Station",
            "the driver reportedly stealing several items from the Mcdonald St RON Station",
            "the driver reportedly stealing several items from the Popular St RON Station",
            "the driver reportedly stealing several items from the El Rancho Blvd RON Station",
            "the driver reportedly stealing several items from the Paleto Bay RON Station",
            "the driver reportedly stealing several items from the Vespucci Canals Rob’s Liquor",
            "the driver reportedly stealing several items from the Prosperity Street Rob’s Liquor",
            "the driver reportedly stealing several items from the El Rancho Boulevard Rob’s Liquor",
            "the driver reportedly stealing several items from the Ace Liquor in Sandy Shores",
            "the driver reportedly stealing several items from Vangelico in Rockford Hills",
            "the driver reportedly stealing several items from the Paleto Bay RON Station",
            "the driver reportedly stealing several items from the Dollar Pills in South LS",
            "the driver reportedly stealing several items from the Dollar Pills on Route 68 in Harmony",
            "the driver reportedly stealing several items from the Discount Store in South LS",
            "the driver reportedly stealing several items from the Paleto Boulevard Discount Store",
            "the driver reportedly stealing several items from the Grapeseed Discount Store",
            "the vehicle was recently reported stolen out of Los Santos",
            "the vehicle was recently reported stolen out of Los Santos County",
            "the vehicle was recently reported stolen out of Blaine County",
            "the vehicle being involved in a road rage incident",
            "the vehicle was involved in a hit and run accident in Los Santos",
            "the vehicle was involved in a hit and run accident in Los Santos County",
            "the vehicle was involved in a hit and run accident in Blaine County",
            "the vehicle was involved in a hit and run accident on the Los Santos Freeway",
            "the vehicle was involved in a hit and run accident on the Del Perro Freeway",
            "the vehicle was involved in a hit and run accident on the Olympic Freeway",
            "the vehicle was involved in a hit and run accident on the La Puerta Freeway",
            "the vehicle fled from LSPD during a traffic stop",
            "the vehicle fled from LSSD during a traffic stop",
            "the vehicle fled from BCSO during a traffic stop",
            "the vehicle fled from SAHP during a traffic stop",
            "the vehicle fled from LSPD during a traffic stop and nearly struck an officer",
            "the vehicle fled from LSSD during a traffic stop and nearly struck a deputy",
            "the vehicle fled from BCSO during a traffic stop and nearly struck a deputy",
            "the vehicle fled from SAHP during a traffic stop and nearly struck a trooper",
            "the vehicle fled from LSPD during a traffic stop and struck an officer",
            "the vehicle fled from LSSD during a traffic stop and struck a deputy",
            "the vehicle fled from BCSO during a traffic stop and struck a deputy",
            "the vehicle fled from SAHP during a traffic stop and struck a trooper",
            "the vehicle fled from LSPD during a traffic stop, shots fired at officers",
            "the vehicle fled from LSSD during a traffic stop, shots fired at deputies",
            "the vehicle fled from BCSO during a traffic stop, shots fired at deputies",
            "the vehicle fled from SAHP during a traffic stop, shots fired at troopers",
            "the driver reportedly discharged a weapon from the vehicle",
            "the driver left from the scene of a domestic in Los Santos",
            "the driver left from the scene of a domestic in Los Santos County",
            "the driver left from the scene of a domestic in Blaine County",
            "the driver left from the scene of a domestic in Los Santos and may be armed",
            "the driver left from the scene of a domestic in Los Santos County and may be armed",
            "the driver left from the scene of a domestic in Blaine County and may be armed",
            "a caller stating the occupant has active warrants",
            "a caller stating the occupant may be suicidal",
            "a caller stating the occupant may be intoxicated",
            "a caller stating the occupant made threats against a third party",
            "a caller stating the occupant made threats against themselves and others",
            "a caller stating the vehicle was driving recklessly"
        };

        internal static void Main(int BOLOTimer, int BOLOFrequency)
        {
            Game.LogTrivial($"[RPE BOLO]: BOLO fiber started with a timer of {BOLOTimer / 60000} minutes.");

            GameFiber.Sleep(60000);  // Disable for testing
            while (true)
            {
                if (!BOLOActive && Functions.GetActivePursuit() == null)
                {
                    var boloVehiclesList = new List<Vehicle>();
                    if (boloVehiclesList.Count > 0)
                    {
                        boloVehiclesList.Clear();
                    }

                    var boloVehicle = FindBOLOVehicle(boloVehiclesList);
                    if (boloVehicle)
                    {
                        boloVehicle.IsPersistent = true;
                        var boloSuspect = boloVehicle.Driver;
                        boloSuspect.IsPersistent = true;
                        BeginBOLO(boloVehicle, boloSuspect);
                    }
                    else
                    {
                        Game.LogTrivial("[RPE BOLO]: No suitable BOLO suspects found.");
                    }
                }
                else
                {
                    Game.LogTrivial($"[RPE BOLO]: BOLO is already active or no suitable vehicles found.  Trying again in {BOLOFrequency / 60000} minutes."); // Disable for testing
                }
                GameFiber.Sleep(BOLOFrequency);
            }

            void BeginBOLO(Vehicle boloVehicle, Ped boloSuspect)
            {
                BOLOActive = true;
                var startTime = Game.GameTime;
                var oldTime = Game.GameTime;
                DisplayBOLOInfo(boloVehicle);
                Game.LogTrivial($"BOLOTimer: {BOLOTimer}");
                while (boloVehicle && boloSuspect)
                {
                    var difference = Game.GameTime - oldTime;
                    if (Functions.IsPedArrested(boloSuspect) || (Functions.GetCurrentPullover() != null && Functions.GetPulloverSuspect(Functions.GetCurrentPullover()) == boloSuspect))
                    {
                        System.Media.SoundPlayer player = new System.Media.SoundPlayer(Directory.GetCurrentDirectory() + @"\lspdfr\audio\sfx\AlertTone.wav");
                        player.Play();
                        Game.DisplayNotification($"~r~~h~BOLO ALERT - CANCELLED~h~\n~g~You located the suspect(s).");
                        Game.LogTrivial($"[RPE BOLO]: BOLO ending, suspect stopped or arrested by player.");
                        BOLOActive = false;
                        break;
                    }
                    if (Game.GameTime - startTime >= BOLOTimer)
                    {
                        var player = new System.Media.SoundPlayer(Directory.GetCurrentDirectory() + @"\lspdfr\audio\sfx\AlertTone.wav");
                        player.Play();
                        Game.DisplayNotification($"~r~~h~BOLO ALERT - CANCELLED~h~\n~w~You failed to locate the suspect(s) in time.");
                        Game.LogTrivial($"[RPE BOLO]: BOLO ending, player failed to locate suspect(s) in time.");
                        BOLOActive = false;
                        break;
                    }
                    if (difference >= 60000 && UpdateCheck(boloVehicle, boloSuspect))
                    {
                        oldTime = Game.GameTime;
                        DisplayBOLOInfo(boloVehicle, true);
                    }
                    GameFiber.Sleep(1000);
                }
            }
        }

        private static Vehicle FindBOLOVehicle(List<Vehicle> boloVehiclesList)
        {
            Game.LogTrivial($"[RPE BOLO]: Looking for BOLO vehicles");
            var pulloverVehicle = GetPulloverVehicle();
            var boloVehicles = World.GetAllVehicles().Where(x => x && x.IsCar && !x.IsPoliceVehicle && x != Game.LocalPlayer.Character.CurrentVehicle && x != Game.LocalPlayer.Character.LastVehicle && x.HasDriver && x.Driver.IsAlive && x != pulloverVehicle);

            AddBOLOVehiclesToList();

            if (boloVehiclesList.Count > 0)
            {
                return ReturnBOLOVehicleToBeUsed();
            }
            else
            {
                return null;
            }

            Vehicle GetPulloverVehicle()
            {
                if (Functions.GetCurrentPullover() != null)
                {
                    Ped p = Functions.GetPulloverSuspect(Functions.GetCurrentPullover());
                    return p.CurrentVehicle;
                }
                return null;
            }

            void AddBOLOVehiclesToList()
            {
                foreach (Vehicle vehicle in boloVehicles)
                {
                    if (vehicle)
                    {
                        var suspectPersona = Functions.GetPersonaForPed(vehicle.Driver);
                        if (suspectPersona.Wanted)
                        {
                            boloVehiclesList.Add(vehicle);
                            Game.LogTrivial("[RPE BOLO]: BOLO vehicle added to list.");
                        }
                    }
                }
            }

            Vehicle ReturnBOLOVehicleToBeUsed()
            {
                int r = new Random().Next(0, boloVehiclesList.Count);
                //Game.LogTrivial($"[RPE BOLO]: Random number between 0 and {boloVehiclesList.Count}: {r}");
                Game.LogTrivial("[RPE BOLO]: Assigning boloVeh.");
                return boloVehiclesList[r];
            }
        }

        private static void DisplayBOLOInfo(Vehicle boloVeh, bool update = false)
        {
            string boloColor;
            try
            {
                boloColor = boloVeh.GetColors().PrimaryColorName;
            }
            catch
            {
                Game.LogTrivial($"[RPE BOLO]: There was a problem getting the vehicle's color.  Ending BOLO event.");
                BOLOActive = false;
                return;
            }
            var boloVehSkin = VehicleSkin.FromVehicle(boloVeh);
            var worldZone = Functions.GetZoneAtPosition(boloVeh.Position).GameName;
            var streetName = World.GetStreetName(World.GetStreetHash(boloVeh.Position));
            var boloReason = boloReasons[new Random().Next(0, boloReasons.Length)];
            var player = new System.Media.SoundPlayer(Directory.GetCurrentDirectory() + @"\lspdfr\audio\sfx\AlertTone.wav");
            player.Play();

            if (!update)
            {
                Game.DisplayNotification($"~y~~h~BOLO ALERT - OCCUPANT(S) WANTED~h~\n~s~~w~Be on the lookout for a(n): ~o~{boloColor} {boloVeh.Model.Name} (plate {boloVehSkin.LicensePlate})~w~ last seen heading ~b~{(Direction)GetSuspectDirection()} ~w~on ~b~{streetName} ~w~in ~b~{worldZone}.  ~w~This BOLO is in regards to ~r~{boloReason}.");
            }
            else
            {
                Game.DisplayNotification($"~y~~h~BOLO UPDATE - OCCUPANT(S) WANTED~h~\n~s~~o~{boloColor} {boloVeh.Model.Name} (plate {boloVehSkin.LicensePlate})~w~ last seen heading ~b~{(Direction)GetSuspectDirection()} ~w~on ~b~{streetName} ~w~in ~b~{worldZone}.");
            }
            

            int GetSuspectDirection()
            {
                if (boloVeh.Heading >= 315 || boloVeh.Heading < 45)
                {
                    return 0;
                }
                else if (boloVeh.Heading >= 45 && boloVeh.Heading < 135)
                {
                    return 3;
                }
                else if (boloVeh.Heading >= 135 && boloVeh.Heading < 225)
                {
                    return 2;
                }
                else if (boloVeh.Heading >= 225 && boloVeh.Heading < 315)
                {

                    return 1;
                }
                return -1;
            }
        }

        private static bool UpdateCheck(Vehicle boloVeh, Ped boloSuspect)
        {
            var policeVehicleSawBOLOVehicle = boloSuspect.GetNearbyVehicles(16).Where(x => x && x.IsPoliceVehicle && x.HasDriver && x.Driver.IsAlive && x.DistanceTo2D(boloVeh) <= 50f).FirstOrDefault();
            if (policeVehicleSawBOLOVehicle)
            {
                Game.LogTrivial("[RPE BOLO]: Police vehicle saw suspect.");
                return true;
            }

            var policePedSawBOLOVehicle = boloSuspect.GetNearbyPeds(16).Where(x => x && x.IsAlive && x.RelationshipGroup == "COP" && x.DistanceTo2D(boloVeh) <= 50f).FirstOrDefault();
            if (policePedSawBOLOVehicle)
            {
                Game.LogTrivial("[RPE BOLO]: Police ped saw suspect.");
                return true;
            }
            return false;
        }
    }
}
