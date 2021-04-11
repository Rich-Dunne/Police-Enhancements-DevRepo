using System.Collections.Generic;
using Rage;

namespace RichsPoliceEnhancements
{
    internal static class Settings
    {
        // Feature Settings
        internal static bool EnableAmbientBackup { get; private set; } = false;
        internal static bool EnableAISirenCycle { get; private set; } = false;
        internal static bool EnableSilentBackup { get; private set; } = false;
        internal static bool EnableTVI { get; private set; } = false;
        internal static bool EnableBOLO { get; private set; } = false;
        internal static bool EnablePRT { get; private set; } = false;
        internal static bool EnablePursuitUpdates { get; private set; } = false;
        internal static bool EnableAmbientEvents { get; private set; } = false;
        internal static bool EnableSuspectStamina { get; private set; } = false;

        // Ambient Event settings
        internal static Dictionary<string, string> EventFrequencies { get; } = new Dictionary<string, string>();
        internal static int EventCooldownTimer { get; private set; } = 5;
        internal static bool EventBlips { get; private set; } = false;
        internal static int CommonEventFrequency { get; private set; } = 70;
        internal static int UncommonEventFrequency { get; private set; } = 20;
        internal static int RareEventFrequency { get; private set; } = 10;
        internal static string AssaultFrequency { get; private set; } = "off";
        internal static string CarJackingFrequency { get; private set; } = "off";
        internal static string DrugDealFrequency { get; private set; } = "off";
        internal static string DriveByFrequency { get; private set; } = "off";
        internal static string ProstitutionFrequency { get; private set; } = "off";

        // BOLO Settings
        internal static bool EnableBOLOStartBlip { get; private set; } = false;
        internal static int BOLOTimer { get; private set; } = 10;
        internal static int BOLOFrequency { get; private set; } = 5;

        // PRT Settings
        internal static int PRTToneTimer { get; private set; } = 15;
        internal static bool AutomaticPRT { get; private set; } = false;
        internal static bool DisablePRTNotifications { get; private set; } = false;

        // Pursuit Update Settings
        internal static int PursuitUpdateTimer { get; private set; } = 20;
        internal static bool DispatchUpdates { get; private set; } = false;
        internal static bool DisableNotifications { get; private set; } = false;

        // Ambient Backup Settings
        internal static bool AlwaysAcceptAmbientBackup { get; private set; } = false;

        // Suspect Stamina Settings
        internal static bool CanTripDuringFootPursuit { get; private set; } = false;
        internal static int TripChance { get; private set; } = 1;

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
            EnableSuspectStamina = ini.ReadBoolean("Features", "EnableSuspectStamina", false);

            // Ambient Event Settings
            EventCooldownTimer = ini.ReadInt32("Ambient Events", "EventCooldownTimer", 5);
            EventCooldownTimer *= 60000;
            EventBlips = ini.ReadBoolean("Ambient Events", "EventBlips", true);
            CommonEventFrequency = ini.ReadInt32("Ambient Events", "CommonEventFrequency", 70);
            UncommonEventFrequency = ini.ReadInt32("Ambient Events", "UnommonEventFrequency", 20);
            RareEventFrequency = ini.ReadInt32("Ambient Events", "RareEventFrequency", 10);
            AssaultFrequency = ini.ReadString("Ambient Events", "AssaultFrequency", "off");
            EventFrequencies.Add("Assault", AssaultFrequency);
            CarJackingFrequency = ini.ReadString("Ambient Events", "CarJackingFrequency", "off");
            EventFrequencies.Add("CarJacking", CarJackingFrequency);
            DrugDealFrequency = ini.ReadString("Ambient Events", "DrugDealFrequency", "off");
            EventFrequencies.Add("DrugDeal", DrugDealFrequency);
            DriveByFrequency = ini.ReadString("Ambient Events", "DriveByFrequency", "off");
            EventFrequencies.Add("DriveBy", DriveByFrequency);
            ProstitutionFrequency = ini.ReadString("Ambient Events", "ProstitutionFrequency", "off");
            EventFrequencies.Add("DriveBy", ProstitutionFrequency);

            // BOLO Settings
            EnableBOLOStartBlip = ini.ReadBoolean("BOLO Settings", "EnableBOLOStartBlip", false);
            BOLOTimer = ini.ReadInt32("BOLO Settings", "BOLOTimer", 10);
            BOLOTimer *= 60000;
            BOLOFrequency = ini.ReadInt32("BOLO Settings", "BOLOFrequency", 5);
            BOLOFrequency *= 60000;

            // PRT Settings
            PRTToneTimer = ini.ReadInt32("Priority Radio Traffic Settings", "PRTToneTimer", 15);
            AutomaticPRT = ini.ReadBoolean("Priority Radio Traffic Settings", "AutomaticPRT", false);
            DisablePRTNotifications = ini.ReadBoolean("Priority Radio Traffic Settings", "DisablePRTNotifications", false);

            // Pursuit Update Settings
            PursuitUpdateTimer = ini.ReadInt32("Pursuit Update Settings", "PursuitUpdateTimer", 20);
            PursuitUpdateTimer *= 1000;
            DispatchUpdates = ini.ReadBoolean("Pursuit Update Settings", "DispatchUpdates", false);
            DisableNotifications = ini.ReadBoolean("Pursuit Update Settings", "DisableNotifications", false);

            // Ambient Backup Settings
            AlwaysAcceptAmbientBackup = ini.ReadBoolean("Ambient Backup Settings", "AlwaysAcceptAmbientBackup", false);

            // Suspect Trip Settings
            CanTripDuringFootPursuit = ini.ReadBoolean("Suspect Trip", "CanTripDuringFootPursuit", false);
            TripChance = ini.ReadInt32("Suspect Trip", "TripChance", 1);
        }
    }
}
