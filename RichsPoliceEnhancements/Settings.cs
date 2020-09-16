﻿using System.Collections.Generic;
using Rage;

namespace RichsPoliceEnhancements
{
    internal static class Settings
    {
        internal static bool EnableAmbientBackup = true;
        internal static bool EnableAISirenCycle = true;
        internal static bool EnableSilentBackup = false;
        internal static bool EnableTVI = false;
        internal static bool EnableBOLO = false;
        internal static bool EnablePRT = false;
        internal static bool EnablePursuitUpdates = true;
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

        internal static void LoadSettings()
        {
            Game.LogTrivial("[RPE]: Loading RichsPoliceEnhancements.ini settings");
            InitializationFile ini = new InitializationFile("Plugins/LSPDFR/RichsPoliceEnhancements.ini");
            ini.Create();

            EnableAmbientBackup = ini.ReadBoolean("Features", "EnableAmbientBackup", true);
            EnableAISirenCycle = ini.ReadBoolean("Features", "EnableAISirenCycle", true);
            EnableSilentBackup = ini.ReadBoolean("Features", "EnableSilentBackup", false);
            EnableTVI = ini.ReadBoolean("Features", "EnableTVI", false);
            EnableBOLO = ini.ReadBoolean("Features", "EnableBOLO", false);
            EnablePRT = ini.ReadBoolean("Features", "EnablePriorityRadioTraffic", false);
            EnablePursuitUpdates = ini.ReadBoolean("Features", "EnablePursuitUpdates", true);
            EnableAmbientEvents = ini.ReadBoolean("Features", "EnableAmbientEvents", false);

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
