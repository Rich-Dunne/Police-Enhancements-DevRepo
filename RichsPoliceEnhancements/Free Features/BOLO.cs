using System;
using System.Collections.Generic;
using System.Linq;

using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Engine.Scripting;
using System.IO;

namespace RichsPoliceEnhancements
{
    // TODO:
    // - Camera focus on BOLO if nearby
    // BUGS:
    // - stalion = 0x72A4C31E in notification for some reason
    public class BOLO
    {
        public static bool BOLOActive, StartBOLOFromCommand, CancelBOLOFromCommand;
        // Hexcolor index https://docs.google.com/spreadsheets/d/11ikZaC0_B7BUPDpZYzBZ5oj57Ba1uh54E8gQ0yCzG0g/pubhtml
        // Reference RGB from list of colors https://pastebin.com/knRKeQxT
        // Reference vehicle color index https://pastebin.com/pwHci0xK
        public static IDictionary<uint, string> carColorsDict = new Dictionary<uint, string>()
        {
            {0xFF242A2D,"black"},
            {0xFF3F2D18,"brown"},
            {0xFFB01259,"hot pink"},
            {0xFF6B0000,"red"},
            {0xFF001B57,"metallic blue"},
            {0xFF2070D8,"blue"},
            {0xFF00441B,"green"},
            {0xFF418503,"lime green"},
            {0xFFAD7B47,"metallic gold"},
            {0xFF5A5E66,"metallic silver"},
            {0xFF808C8C,"chrome"},
            {0xFF080808,"black"},
            {0xFF0F0F0F,"black"},
            {0xFF121110,"black"},
            {0xFF1C1E21,"dark gray"},
            {0xFF292C2E,"dark silver"},
            {0xFF777C87,"silver"},
            {0xFF515459,"gray"},
            {0xFF323B47,"bluish silver"},
            {0xFF333333,"gray"},
            {0xFF1F2226,"dark silver"},
            {0xFF23292E,"bluish silver"},
            {0xFF690000,"red"},
            {0xFF8A0B00,"red"},
            {0xFF6B0B00,"red"},
            {0xFF611009,"red"},
            {0xFF4A0A0A,"orange"},
            {0xFF470E0E,"maroon"},
            {0xFF380C00,"maroon"},
            {0xFF26030B,"maroon"},
            {0xFF080000,"red"},
            {0xFF630012,"red"},
            {0xFF8F2F55,"pink"},
            {0xFFF69799,"salmon pink"},
            {0xFF802800,"orange"},
            {0xFFBD4800,"orange"},
            {0xFFC26610,"bright orange"},
            {0xFF4A341B,"bronze"},
            {0xFFF5890F,"yellow"},
            {0xFFD9A600,"yellow"},
            {0xFFA2A827,"fluorescent yellow"},
            {0xFF001207,"dark green"},
            {0xFF001A0B,"green"},
            {0xFF00211E,"green"},
            {0xFF1F261E,"olive green"},
            {0xFF003805,"bright green"},
            {0xFF0B4145,"green"},
            {0xFF568F00,"lime green"},
            {0xFF000108,"dark blue"},
            {0xFF000D14,"blue"},
            {0xFF001029,"dark blue"},
            {0xFF1C2F4F,"blue"},
            {0xFF3B4E78,"blue"},
            {0xFF272D3B,"blue"},
            {0xFF95B2DB,"blue"},
            {0xFF3E627A,"blue"},
            {0xFF1C3140,"blue"},
            {0xFF0E316D,"blue"},
            {0xFF0055C4,"blue"},
            {0xFF395A83,"light blue"},
            {0xFF120D07,"brown"},
            {0xFF221918,"brown"},
            {0xFF262117,"brown"},
            {0xFF291B06,"brown"},
            {0xFF332111,"brown"},
            {0xFF241309,"brown"},
            {0xFF3B1700,"brown"},
            {0xFF3D3023,"brown"},
            {0xFF37382B,"brown"},
            {0xFF575036,"brown"},
            {0xFF5E5343,"brown"},
            {0xFF6E6246,"brown"},
            {0xFB998D73,"brown"},
            {0xFF161629,"purple"},
            {0xFF00080F,"purple"},
            {0xFF320642,"bright purple"},
            {0xFFCFC0A5,"cream"},
            {0xFFF0F0F0,"white"},
            {0xFFB3B9C9,"white"},
            {0xFF050505,"black"},
            {0xFF121212,"gray"},
            {0xFF2F3233,"light gray"},
            {0xFFD9D9D9,"white"},
            {0xFF0F1E73,"blue"},
            {0xFF030E2E,"dark blue"},
            {0xFF001C32,"dark blue"},
            {0xFF050008,"dark purple"},
            {0xFF780000,"red"},
            {0xFF360000,"dark red"},
            {0xFFAB3F00,"orange"},
            {0xFFDE7E00,"yellow"},
            {0xFF243022,"green"},
            {0xFF121710,"green"},
            {0xFF2B302B,"green"},
            {0xFF323325,"olive drab"},
            {0xFF3B352D,"dark rarth"},
            {0xFF706656,"desert tan"},
            {0xFF3B4045,"gray"},
            {0xFF5E646B,"silver"},
            {0xFF47391B,"pure gold"},
            {0xFF0F1012,"black"},
            {0xFF212121,"black"},
            {0xFF5B5D5E,"silver"},
            {0xFF888A99,"silver"},
            {0xFF697187,"silver"},
            {0xFF3B4654,"bluish silver"},
            {0xFF6E4F2D,"gold"},
            {0xFF520000,"red"},
            {0xFF8C0404,"bright red"},
            {0xFF4A1000,"red"},
            {0xFF592525,"red"},
            {0xFF754231,"golden red"},
            {0xFF210804,"dark red"},
            {0xFF0F1F15,"dark green"},
            {0xFF023613,"green"},
            {0xFF162419,"dark green"},
            {0xFF2A3625,"green"},
            {0xFF455C56,"green"},
            {0xFF1A182E,"purple"},
            {0xFF09142E,"dark blue"},
            {0xFF0F1021,"dark blue"},
            {0xFF152A52,"blue"},
            {0xFF324654,"blue"},
            {0xFF152563,"blue"},
            {0xFF223BA1,"blue"},
            {0xFF1F1FA1,"bright blue"},
            {0xFF2A3754,"dark blue"},
            {0xFF303C5E,"blue"},
            {0xFF3B6796,"baby blue"},
            {0xFF57514B,"champagne"},
            {0xFF1F1709,"brown"},
            {0xFF3D311D,"brown"},
            {0xFF665847,"light brown"},
            {0xFF615F55,"beige"},
            {0xFF241E1A,"brown"},
            {0xFF171413,"dark brown"},
            {0xFF3B372F,"beige"},
            {0xFF1A1E21,"black"},
            {0xFF000000,"chrome"},
            {0xFFB0B0B0,"off-white"},
            {0xFF999999,"off-white"},
            {0xFFB56519,"orange"},
            {0xFFC45C33,"light orange"},
            {0xFF47783C,"green"},
            {0xFFBA8425,"yellow"},
            {0xFF2A77A1,"blue"},
            {0xFF6B5F54,"brown"},
            {0xFFC96E34,"orange"},
            {0xFF3F4228,"green"},
            {0xFFFFFFFF,"white"},
            {0xFF69BD45,"bright green"},
            {0xFF00AEEF,"fluorescent blue"},
            {0xFF565751,"green"},
            {0xFF414347,"gray"},
            {0xFF6690B5,"blue"},
            {0xFFFFD859,"yellow"},
            {0xFF998D73,"tan" }
        };
        //BOLO is in regards to...
        public static string[] boloReasons = {
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

        public static void Main(int BOLOTimer, int BOLOFrequency)
        {
            LSPD_First_Response.Engine.Scripting.Entities.Persona suspectPersona = null;
            List<Vehicle> vehiclesList = new List<Vehicle>();
            Vehicle boloVeh;
            Ped boloSuspect;

            Game.LogTrivial("[RPE BOLO]: BOLO fiber started with a timer of " + BOLOTimer / 60000 + " minutes.");

            GameFiber.Sleep(60000);  // Disable for testing
            while (true)
            {
                GameFiber.Yield();
                //if (Game.IsKeyDown(Keys.P))
                if (!BOLOActive && Functions.GetActivePursuit() == null) // Disable for testing && StartBOLO()
                {
                    StartBOLOFromCommand = false;
                    BOLOActive = true;
                    if (vehiclesList.Count > 0)
                    {
                        vehiclesList.Clear();
                    }

                    // Find BOLO vehicles in world
                    Game.LogTrivial("[RPE BOLO]: Looking for BOLO vehicles");
                    boloVeh = FindBoloVeh(suspectPersona, vehiclesList);

                    if (boloVeh != null)
                    {
                        boloVeh.IsPersistent = true;
                        boloSuspect = boloVeh.Driver;
                        boloSuspect.IsPersistent = true;
                        DisplayBOLOInfo(boloVeh);

                        // GameFiber to run in the background checking for end conditions
                        GameFiber EndCheckFiber = new GameFiber(() => EndCheck(boloSuspect, boloVeh, BOLOFrequency));
                        EndCheckFiber.Start();

                        GameFiber.Sleep(BOLOTimer / 2);
                        while (BOLOActive && boloSuspect.Exists() && boloSuspect.IsValid())
                        {
                            GameFiber.Yield();
                            Game.LogTrivial("[RPE BOLO]: Running update checker.");
                            UpdateCheck(boloVeh, boloSuspect);
                            GameFiber.Sleep(BOLOTimer / 2);
                            //Game.LogTrivial("[RPE BOLO]: Checking distance between suspect and player before ending BOLO.");
                            //if (boloVeh.Exists() && boloVeh.IsValid() && Game.LocalPlayer.Character.DistanceTo(boloVeh.Position) > 50f)
                            //{
                            BOLOActive = false;
                            GameFiber.Sleep(BOLOFrequency);
                            //}
                        }
                    }
                    else
                    {
                        BOLOActive = false;
                        //Game.DisplaySubtitle(string.Format("No suitable BOLO suspects found"));
                        Game.LogTrivial("[RPE BOLO]: No suitable BOLO suspects found.");
                    }
                }
                else
                {
                    // Sleep [user defined time] minutes before attempting another BOLO
                    Game.LogTrivial("[RPE BOLO]: BOLO is already active or no suitable vehicles found.  Trying again in " + BOLOFrequency / 60000 + " minutes."); // Disable for testing
                    GameFiber.Sleep(BOLOFrequency); // Disable for testing
                }
            }
        }

        private static bool StartBOLO()
        {
            int r = new Random().Next(0, 2);
            if (r == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private static Vehicle FindBoloVeh(LSPD_First_Response.Engine.Scripting.Entities.Persona suspectPersona, List<Vehicle> vehiclesList)
        {
            Vehicle pulloverVeh = new Vehicle(); // What is the purpose of this?
            if (Functions.GetCurrentPullover() != null)
            {
                Ped p = Functions.GetPulloverSuspect(Functions.GetCurrentPullover());
                pulloverVeh = p.CurrentVehicle;
            }

            foreach (Vehicle veh in World.GetAllVehicles())
            {
                if (veh.IsValid() && !Game.LocalPlayer.Character.IsInVehicle(veh, false) && !veh.IsPoliceVehicle && veh.IsCar && veh.HasDriver && veh.Driver != Game.LocalPlayer.Character && veh != pulloverVeh)
                {
                    Game.LogTrivial("[RPE BOLO]: Vehicle found");
                    //BOLOBlip = suspect.AttachBlip();
                    suspectPersona = Functions.GetPersonaForPed(veh.Driver);
                    if (suspectPersona.Wanted == true)
                    {
                        vehiclesList.Add(veh);
                        Game.LogTrivial("[RPE BOLO]: BOLO vehicle added to list.");
                    }
                }
            }

            if (vehiclesList.Count > 0)
            {
                int r = new Random().Next(0, vehiclesList.Count);
                Game.LogTrivial("[RPE BOLO]: Random number between 0 and " + vehiclesList.Count.ToString() + ": " + r.ToString());
                Game.LogTrivial("[RPE BOLO]: Assigning boloVeh.");
                return vehiclesList[r];
            }
            else
            {
                return null;
            }
        }

        private static void DisplayBOLOInfo(Vehicle boloVeh)
        {
            LSPD_First_Response.Engine.Scripting.Entities.VehicleSkin boloVehSkin;
            float suspectHeading;
            string headingDirection = null, vehColorName = null;
            uint vehColorHex;

            Game.LogTrivial("[RPE BOLO]: Getting suspectVeh.");
            boloVehSkin = LSPD_First_Response.Engine.Scripting.Entities.VehicleSkin.FromVehicle(boloVeh);

            Game.LogTrivial("[RPE BOLO]: Getting suspectZone");
            WorldZone suspectZone = Functions.GetZoneAtPosition(boloVeh.Position);

            Game.LogTrivial("[RPE BOLO]: Getting street name.");
            World.GetStreetName(World.GetStreetHash(boloVeh.Position));

            Game.LogTrivial("[RPE BOLO]: Getting suspectHeading.");
            suspectHeading = boloVeh.Heading;

            Game.LogTrivial("[RPE BOLO]: Getting license plate.");
            string boloPlate = boloVehSkin.LicensePlate;

            // Heading
            // North (360), South (180), East(270), West(90)
            if (suspectHeading >= 315 || suspectHeading < 45)
            {
                headingDirection = "north";
            }
            else if (suspectHeading >= 45 && suspectHeading < 135)
            {
                headingDirection = "west";
            }
            else if (suspectHeading >= 135 && suspectHeading < 225)
            {
                headingDirection = "south";
            }
            else if (suspectHeading >= 225 && suspectHeading < 315)
            {
                headingDirection = "east";
            }

            vehColorHex = Convert.ToUInt32("0x" + boloVeh.PrimaryColor.Name.ToUpper(), 16);
            Game.LogTrivial("[RPE BOLO]: vehColor hex: 0x" + boloVeh.PrimaryColor.Name.ToUpper());

            for (int i = 0; i < carColorsDict.Count; i++)
            {
                if (vehColorHex == carColorsDict.Keys.ElementAt(i))
                {
                    Game.LogTrivial("[RPE BOLO]: vehColor name: " + carColorsDict[carColorsDict.Keys.ElementAt(i)]);
                    vehColorName = carColorsDict[carColorsDict.Keys.ElementAt(i)];
                    break;
                }
            }

            int r = new Random().Next(0, boloReasons.Length);

            System.Media.SoundPlayer player = new System.Media.SoundPlayer(Directory.GetCurrentDirectory() + @"\lspdfr\audio\sfx\AlertTone.wav");
            player.Play();
            Game.DisplayNotification(string.Format("~r~~h~BOLO ALERT - OCCUPANT(S) WANTED~h~~n~~s~~w~Be on the lookout for a(n): ~o~ {0} {1} (plate {2})~w~ last seen heading ~b~{3} ~w~on ~b~{4} ~w~in ~b~{5}.  ~w~This BOLO is in regards to ~y~{6}. ", vehColorName, boloVeh.Model.Name, boloPlate, headingDirection, World.GetStreetName(World.GetStreetHash(boloVeh.Position)), suspectZone.RealAreaName, boloReasons[r]));
        }

        private static void UpdateBOLOInfo(Vehicle boloVeh)
        {
            LSPD_First_Response.Engine.Scripting.Entities.VehicleSkin boloVehSkin;
            float suspectHeading;
            string headingDirection = null, vehColorName = null;
            uint vehColorHex;

            Game.LogTrivial("[RPE BOLO]: Getting updated BOLO information.");
            //Game.LogTrivial("[RPE]: Getting suspectVeh.");
            boloVehSkin = LSPD_First_Response.Engine.Scripting.Entities.VehicleSkin.FromVehicle(boloVeh);

            //Game.LogTrivial("[RPE]: Getting suspectZone");
            WorldZone suspectZone = Functions.GetZoneAtPosition(boloVeh.Position);

            //Game.LogTrivial("[RPE]: Getting street name.");
            World.GetStreetName(World.GetStreetHash(boloVeh.Position));

            //Game.LogTrivial("[RPE]: Getting suspectHeading.");
            suspectHeading = boloVeh.Heading;

            //Game.LogTrivial("[RPE]: Getting license plate.");
            string boloPlate = boloVehSkin.LicensePlate;

            // Heading
            // North (360), South (180), East(270), West(90)
            if (suspectHeading >= 315 || suspectHeading < 45)
            {
                headingDirection = "north";
            }
            else if (suspectHeading >= 45 && suspectHeading < 135)
            {
                headingDirection = "west";
            }
            else if (suspectHeading >= 135 && suspectHeading < 225)
            {
                headingDirection = "south";
            }
            else if (suspectHeading >= 225 && suspectHeading < 315)
            {
                headingDirection = "east";
            }

            vehColorHex = Convert.ToUInt32("0x" + boloVeh.PrimaryColor.Name.ToUpper(), 16);
            //Game.LogTrivial("[RPE]: vehColor hex: 0x" + boloVeh.PrimaryColor.Name.ToUpper());

            for (int i = 0; i < carColorsDict.Count; i++)
            {
                if (vehColorHex == carColorsDict.Keys.ElementAt(i))
                {
                    vehColorName = carColorsDict[carColorsDict.Keys.ElementAt(i)];
                    break;
                }
            }

            System.Media.SoundPlayer player = new System.Media.SoundPlayer(Directory.GetCurrentDirectory() + @"\lspdfr\audio\sfx\AlertTone.wav");
            player.Play();
            Game.DisplayNotification(string.Format("~r~~h~BOLO ALERT - UPDATE~h~~n~~s~~w~The~o~ {0} {1} (plate {2})~w~ was recently seen heading ~b~{3} ~w~on ~b~{4} ~w~in ~b~{5}.", vehColorName, boloVeh.Model.Name, boloPlate, headingDirection, World.GetStreetName(World.GetStreetHash(boloVeh.Position)), suspectZone.RealAreaName));

        }

        private static void UpdateCheck(Vehicle boloVeh, Ped boloSuspect)
        {
            bool update = false;
            foreach (Vehicle v in boloSuspect.GetNearbyVehicles(16))
            {
                //Game.DisplaySubtitle(string.Format("Searching for nearby police vehicles."));
                Game.LogTrivial("[RPE BOLO]: Searching for nearby police vehicles");
                if (v.DistanceTo(boloVeh) <= 50f)
                {
                    Game.LogTrivial("[RPE BOLO]: A vehicle is nearby.");
                }
                if (v.IsPoliceVehicle && v.DistanceTo(boloVeh) <= 50f)
                {
                    //Game.DisplaySubtitle(string.Format("Police vehicle saw suspect."));
                    Game.LogTrivial("[RPE BOLO]: Police vehicle saw suspect.");
                    update = true;
                    break;
                }
            }
            if (!update)
            {
                //Game.DisplaySubtitle(string.Format("Searching for nearby police peds."));
                Game.LogTrivial("[RPE BOLO]: Searching for nearby police peds.");
                foreach (Ped p in boloSuspect.GetNearbyPeds(16))
                {
                    if (p.RelationshipGroup == "COP" && p.DistanceTo(boloVeh) <= 50f)
                    {
                        //Game.DisplaySubtitle(string.Format("Police ped saw suspect."));
                        Game.LogTrivial("[RPE BOLO]: Police ped saw suspect.");
                        update = true;
                        break;
                    }
                }
            }
            if (!update && StartBOLO())
            {
                update = true;
            }
            if (update)
            {
                //Game.DisplaySubtitle(string.Format("Incoming BOLO update."));
                Game.LogTrivial("[RPE BOLO]: Player is receiving a BOLO update..");
                UpdateBOLOInfo(boloVeh);
            }
        }

        private static void EndCheck(Ped boloSuspect, Vehicle boloVeh, int BOLOFrequency)
        {
            bool success = false;
            while (BOLOActive && boloSuspect.IsValid() && boloSuspect.Exists())
            {
                GameFiber.Yield();
                // A crash might be happening because of this for some reason
                if (boloSuspect.IsValid() && boloSuspect.Exists() && Functions.IsPedArrested(boloSuspect))// || Functions.IsPedStoppedByPlayer(boloSuspect))
                {
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(Directory.GetCurrentDirectory() + @"\lspdfr\audio\sfx\AlertTone.wav");
                    player.Play();
                    Game.DisplayNotification(string.Format("~r~~h~BOLO ALERT - CANCELLED~h~~n~~g~You located the suspect(s)."));
                    Game.LogTrivial("[RPE BOLO]: BOLO ending, suspect stopped or arrested by player.");
                    BOLOActive = false;
                    success = true;
                }
                else if (Functions.IsPlayerPerformingPullover())
                {
                    if (boloSuspect.IsValid() && boloSuspect.Exists() && Functions.GetPulloverSuspect(Functions.GetCurrentPullover()) == boloSuspect)
                    {
                        System.Media.SoundPlayer player = new System.Media.SoundPlayer(Directory.GetCurrentDirectory() + @"\lspdfr\audio\sfx\AlertTone.wav");
                        player.Play();
                        Game.DisplayNotification(string.Format("~r~~h~BOLO ALERT - CANCELLED~h~~n~~g~You located the suspect(s)."));
                        Game.LogTrivial("[RPE BOLO]: BOLO ending, player pulling over suspect");
                        BOLOActive = false;
                        success = true;
                    }
                }
            }
            if (!BOLOActive || !boloSuspect.IsValid() || !boloSuspect.Exists())
            {
                if (boloVeh.Exists())
                {
                    boloVeh.IsPersistent = false;
                    boloVeh.Dismiss();
                }
                if (boloSuspect.Exists())
                {
                    boloSuspect.IsPersistent = false;
                    boloSuspect.Dismiss();
                }
                boloSuspect = null;
                boloVeh = null;
                if (!success)
                {
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(Directory.GetCurrentDirectory() + @"\lspdfr\audio\sfx\AlertTone.wav");
                    player.Play();
                    Game.DisplayNotification(string.Format("~r~~h~BOLO ALERT - CANCELLED~h~~n~The suspects were unable to be located."));
                    Game.LogTrivial("[RPE BOLO]: BOLO ending, UTL");
                    GameFiber.Sleep(BOLOFrequency);
                }
            }
        }

    }
}
