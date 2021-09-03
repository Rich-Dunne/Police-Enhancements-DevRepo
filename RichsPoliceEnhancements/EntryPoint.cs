using Rage;
using LSPD_First_Response.Mod.API;
using System.Reflection;
using System.Linq;
using RichsPoliceEnhancements.Utils;
using RichsPoliceEnhancements.Features;

[assembly: Rage.Attributes.Plugin("Rich's Police Enhancements", Author = "Rich", Description = "Quality of life features for police AI", PrefersSingleInstance = true)]

namespace RichsPoliceEnhancements
{
    public class Main : Plugin
    {
        public override void Initialize()
        {
            Settings.LoadSettings();
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
        }

        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                InitializeFeatures();
                GetAssemblyVersion();
            }
        }


        public override void Finally()
        {
            if (IsPluginLoaded("VocalDispatch"))
            {
                PriorityRadioTraffic.VDPRTCancel.ReleaseVocalDispatchAPI();
                PriorityRadioTraffic.VDPRTRequest.ReleaseVocalDispatchAPI();
            }

            Game.LogTrivial("[RPE]: Rich's Police Enhancements has been cleaned up.");

            bool IsPluginLoaded(string pluginName) => Functions.GetAllUserPlugins().ToList().Any(a => a.FullName.Contains(pluginName));
        }

        private static void InitializeFeatures()
        {
            Game.AddConsoleCommands();

            if (Settings.EnableAmbientBackup)
            {
                Game.LogTrivial("[RPE]: AmbientBackup is enabled.");
                GameFiber.StartNew(() => AmbientBackup.Main(), "RPE Ambient Backup Fiber");
            }
            else
            {
                Game.LogTrivial("[RPE]: AmbientBackup is disabled.");
            }

            if (Settings.EnableAISirenCycle)
            {
                Game.LogTrivial("[RPE]: AISirenCycle is enabled.");
                GameFiber.StartNew(() => AISirenCycle.Main(), "RPE Siren Cycle Fiber");
            }
            else
            {
                Game.LogTrivial("[RPE]: AISirenCycle is disabled.");
            }

            if (Settings.BackupMimicLights)
            {
                Game.LogTrivial("[RPE]: BackupMimicLights is enabled.");
                GameFiber.StartNew(() => BackupMimicLights.Main(), "RPE Backup Mimic Lights Fiber");
            }
            else
            {
                Game.LogTrivial("[RPE]: BackupMimicLights is disabled.");
            }

            if (Settings.EnableTVI)
            {
                Game.LogTrivial("[RPE]: TVI is enabled.");
                GameFiber.StartNew(() => TVI.Main(), "RPE TVI Fiber");
            }
            else
            {
                Game.LogTrivial("[RPE]: TVI is disabled.");
            }

            if (Settings.EnablePRT)
            {
                Game.LogTrivial("[RPE]: PriorityRadioTraffic is enabled.");
                GameFiber.StartNew(() => PriorityRadioTraffic.Main(DependencyChecker.DoesPluginExist("PoliceSmartRadio.dll"), DependencyChecker.DoesPluginExist("VocalDispatch.dll"), DependencyChecker.DoesPluginExist("GrammarPolice.dll")), "RPE Priority Radio Traffic Fiber");
            }
            else
            {
                Game.LogTrivial("[RPE]: PriorityRadioTraffic is disabled.");
            }

            if (Settings.EnablePursuitUpdates)
            {
                Game.LogTrivial("[RPE]: AutomaticPursuitUpdates is enabled.");
                Events.OnPursuitStarted += PursuitUpdates.PursuitUpdateHandler;
            }
            else
            {
                Game.LogTrivial("[RPE]: AutomaticPursuitUpdates is disabled.");
            }

            if (Settings.EnableSuspectStamina)
            {
                Game.LogTrivial("[RPE]: SuspectStamina is enabled.");
                GameFiber.StartNew(() => SuspectStamina.Main(), "RPE Suspect Stamina Fiber");
            }
            else
            {
                Game.LogTrivial("[RPE]: SuspectStamina is disabled.");
            }

            if (Settings.CanTripDuringFootPursuit)
            {
                Game.LogTrivial($"[RPE]: SuspectTrip is enabled.");
                GameFiber.StartNew(() => SuspectTrip.Main(), "RPE Suspect Trip Fiber");
            }
            else
            {
                Game.LogTrivial($"[RPE]: SuspectTrip is disabled.");
            }
        }

        private static void GetAssemblyVersion()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Game.LogTrivial($"Rich's Police Enhancements V{version} is ready.");
        }
    }
}