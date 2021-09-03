using Rage;

namespace RichsPoliceEnhancements
{
    internal static class Settings
    {
        // Feature Settings
        internal static bool EnableAmbientBackup { get; private set; } = false;
        internal static bool EnableAISirenCycle { get; private set; } = false;
        internal static bool BackupMimicLights { get; private set; } = false;
        internal static bool EnableTVI { get; private set; } = false;
        internal static bool EnablePRT { get; private set; } = false;
        internal static bool EnablePursuitUpdates { get; private set; } = false;
        internal static bool EnableSuspectStamina { get; private set; } = false;

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

        private static readonly InitializationFile _ini = new InitializationFile("Plugins/LSPDFR/RichsPoliceEnhancements.ini");

        internal static void LoadSettings()
        {
            Game.LogTrivial("[RPE]: Loading RichsPoliceEnhancements.ini settings");
            _ini.Create();

            // Feature settings
            EnableAmbientBackup = _ini.ReadBoolean("Features", "EnableAmbientBackup", false);
            EnableAISirenCycle = _ini.ReadBoolean("Features", "EnableAISirenCycle", false);
            BackupMimicLights = _ini.ReadBoolean("Features", "EnableBackupMimicLights", false);
            EnableTVI = _ini.ReadBoolean("Features", "EnableTVI", false);
            EnablePRT = _ini.ReadBoolean("Features", "EnablePriorityRadioTraffic", false);
            EnablePursuitUpdates = _ini.ReadBoolean("Features", "EnablePursuitUpdates", false);
            EnableSuspectStamina = _ini.ReadBoolean("Features", "EnableSuspectStamina", false);

            // PRT Settings
            PRTToneTimer = _ini.ReadInt32("Priority Radio Traffic Settings", "PRTToneTimer", 15);
            AutomaticPRT = _ini.ReadBoolean("Priority Radio Traffic Settings", "AutomaticPRT", false);
            DisablePRTNotifications = _ini.ReadBoolean("Priority Radio Traffic Settings", "DisablePRTNotifications", false);

            // Pursuit Update Settings
            PursuitUpdateTimer = _ini.ReadInt32("Pursuit Update Settings", "PursuitUpdateTimer", 20);
            PursuitUpdateTimer *= 1000;
            DispatchUpdates = _ini.ReadBoolean("Pursuit Update Settings", "DispatchUpdates", false);
            DisableNotifications = _ini.ReadBoolean("Pursuit Update Settings", "DisableNotifications", false);

            // Ambient Backup Settings
            AlwaysAcceptAmbientBackup = _ini.ReadBoolean("Ambient Backup Settings", "AlwaysAcceptAmbientBackup", false);

            // Suspect Trip Settings
            CanTripDuringFootPursuit = _ini.ReadBoolean("Suspect Trip", "CanTripDuringFootPursuit", false);
            TripChance = _ini.ReadInt32("Suspect Trip", "TripChance", 1);
        }
    }
}
