using System.Collections.Generic;
using System.Linq;
using LSPD_First_Response.Mod.API;
using Rage;

namespace RichsPoliceEnhancements
{
    internal static class Settings
    {
        internal enum EventFrequency
        {
            Common = 0,
            Uncommon = 1,
            Rare = 2
        }

        internal enum EventType 
        { 
            DrugDeal = 0,
            DriveBy = 1,
            CarJacking = 2,
            Assault = 4,
            RoadRage = 5,
            PublicIntoxication = 6,
            DUI = 7,
            Prostitution = 8,
            Protest = 9,
            SuspiciousCircumstances = 10,
            CriminalMischief = 11,
            OfficerAmbush = 12,
            CitizenAssist = 13,
            MentalHealth = 14
        }


        // Feature Settings
        internal static bool EnableAmbientBackup = false;
        internal static bool EnableAISirenCycle = false;
        internal static bool EnableSilentBackup = false;
        internal static bool EnableTVI = false;
        internal static bool EnableBOLO = false;
        internal static bool EnablePRT = false;
        internal static bool EnablePursuitUpdates = false;
        internal static bool EnableAmbientEvents = false;

        // Ambient Event settings
        internal static Dictionary<string, string> eventFrequencies = new Dictionary<string, string>();
        internal static int EventCooldownTimer = 5;
        internal static bool EventBlips = false;
        internal static int CommonEventFrequency = 70;
        internal static int UncommonEventFrequency = 20;
        internal static int RareEventFrequency = 10;
        internal static string AssaultFrequency = "common";
        internal static string CarJackingFrequency = "uncommon";
        internal static string DrugDealFrequency = "common";
        internal static string DriveByFrequency = "rare";

        // BOLO Settings
        internal static int BOLOTimer = 10;
        internal static int BOLOFrequency = 5;

        // PRT Settings
        internal static int PRTToneTimer = 15;

        // Pursuit Update Settings
        internal static int PursuitUpdateTimer = 20;
        internal static readonly InitializationFile ini = new InitializationFile("Plugins/LSPDFR/RichsPoliceEnhancements.ini");

        internal static void LoadSettings()
        {
            Game.LogTrivial("[RPE]: Loading RichsPoliceEnhancements.ini settings");
            ini.Create();

            // Feature settings
            EnableAmbientBackup = ini.ReadBoolean("Features", "EnableAmbientBackup", false);
            EnableAISirenCycle = ini.ReadBoolean("Features", "EnableAISirenCycle", false);
            EnableSilentBackup = ini.ReadBoolean("Features", "EnableSilentBackup", false);
            EnableTVI = ini.ReadBoolean("Features", "EnableTVI", false);
            EnableBOLO = ini.ReadBoolean("Features", "EnableBOLO", false);
            EnablePRT = ini.ReadBoolean("Features", "EnablePriorityRadioTraffic", false);
            EnablePursuitUpdates = ini.ReadBoolean("Features", "EnablePursuitUpdates", false);
            EnableAmbientEvents = ini.ReadBoolean("Features", "EnableAmbientEvents", false);

            // Ambient Event Settings
            EventCooldownTimer = ini.ReadInt32("Ambient Events", "EventCooldownTimer", 5);
            EventBlips = ini.ReadBoolean("Ambient Events", "EventBlips", true);
            CommonEventFrequency = ini.ReadInt32("Ambient Events", "CommonEventFrequency", 70);
            UncommonEventFrequency = ini.ReadInt32("Ambient Events", "UnommonEventFrequency", 20);
            RareEventFrequency = ini.ReadInt32("Ambient Events", "RareEventFrequency", 10);
            AssaultFrequency = ini.ReadString("Ambient Events", "AssaultFrequency", "common");
            eventFrequencies.Add("Assault", AssaultFrequency);
            CarJackingFrequency = ini.ReadString("Ambient Events", "CarJackingFrequency", "uncommon");
            eventFrequencies.Add("CarJacking", CarJackingFrequency);
            DrugDealFrequency = ini.ReadString("Ambient Events", "DrugDealFrequency", "common");
            eventFrequencies.Add("DrugDeal", DrugDealFrequency);
            DriveByFrequency = ini.ReadString("Ambient Events", "DriveByFrequency", "rare");
            eventFrequencies.Add("DriveBy", DriveByFrequency);

            // BOLO Settings
            BOLOTimer = ini.ReadInt32("BOLO Settings", "BOLOTimer", 10);
            BOLOFrequency = ini.ReadInt32("BOLO Settings", "BOLOFrequency", 5);

            // PRT Settings
            PRTToneTimer = ini.ReadInt32("PRT Settings", "PRTToneTimer", 15);

            // Pursuit Update Settings
            PursuitUpdateTimer = ini.ReadInt32("Pursuit Update Settings", "PursuitUpdateTimer", 20);
        }
    }
}
