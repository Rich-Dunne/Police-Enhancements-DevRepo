using System.Collections.Generic;
using Rage;

namespace RichsPoliceEnhancements
{
    internal static class Settings
    {
        // Patreon Features
        internal static string id = PatronVerification.passThrough(PatronVerification.GetID());
        internal static string PatronKey = null; // This cannot reference VerifyUser because the file can just be shared and it will always work.  Must be manually set to each user's ID
        internal static bool EnableAmbientEvents = true;

        // Free Features
        internal static bool EnableAmbientBackup = true;
        internal static bool EnableAISirenCycle = true;
        internal static bool EnableSilentBackup = false;
        internal static bool EnableTVI = false;
        internal static bool EnableBOLO = false;
        internal static bool EnablePRT = false;
        internal static bool EnablePursuitUpdates = true;

        // Ambient Event Settings
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

        internal static void LoadSettings()
        {
            Game.LogTrivial("[RPE]: Loading RichsPoliceEnhancements.ini settings");
            InitializationFile ini = new InitializationFile("Plugins/LSPDFR/RichsPoliceEnhancements.ini");
            ini.Create();

            // Patreon Features
            PatronKey = ini.ReadString("Patreon Features", "PatronKey", null);
            EnableAmbientEvents = ini.ReadBoolean("Patreon Features", "EnableAmbientEvents", true);

            // Free Features
            EnableAmbientBackup = ini.ReadBoolean("Free Features", "EnableAmbientBackup", true);
            EnableAISirenCycle = ini.ReadBoolean("Free Features", "EnableAISirenCycle", true);
            EnableSilentBackup = ini.ReadBoolean("Free Features", "EnableSilentBackup", false);
            EnableTVI = ini.ReadBoolean("Free Features", "EnableTVI", false);
            EnableBOLO = ini.ReadBoolean("Free Features", "EnableBOLO", false);
            EnablePRT = ini.ReadBoolean("Free Features", "EnablePriorityRadioTraffic", false);
            EnablePursuitUpdates = ini.ReadBoolean("Free Features", "EnablePursuitUpdates", true);

            // Ambient Event Settings
            EventCooldownTimer = ini.ReadInt32("Ambient Events", "EventCooldownTimer", 5);
            EventBlips = ini.ReadBoolean("Ambient Events", "EventBlips", true);
            CommonEventFrequency = ini.ReadInt32("Ambient Events", "CommonEventFrequency", 70);
            UncommonEventFrequency = 100 - ini.ReadInt32("Ambient Events", "UnommonEventFrequency", 20);
            RareEventFrequency = 100 - ini.ReadInt32("Ambient Events", "RareEventFrequency", 10);
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
