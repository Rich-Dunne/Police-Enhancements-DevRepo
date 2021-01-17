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
            if (Settings.EnableAmbientEvents)
            {
                Game.LogTrivial("[RPE]: AmbientEvents are enabled.");
                GameFiber AmbientEventsFiber = new GameFiber(() => AmbientEvents.Main(), "RPE Ambient Event Main Fiber");
                AmbientEventsFiber.Start();
            }
            else
            {
                Game.LogTrivial("[RPE]: AmbientEvents are disabled.");
            }

            if (Settings.EnableAmbientBackup)
            {
                Game.LogTrivial("[RPE]: AmbientBackup is enabled.");
                GameFiber AmbientBackupFiber = new GameFiber(() => AmbientBackup.Main(), "RPE Ambient Backup Fiber");
                AmbientBackupFiber.Start();
            }
            else
            {
                Game.LogTrivial("[RPE]: AmbientBackup is disabled.");
            }

            if (Settings.EnableAISirenCycle)
            {
                Game.LogTrivial("[RPE]: AISirenCycle is enabled.");
                GameFiber SirenCycleFiber = new GameFiber(() => AISirenCycle.Main(), "RPE Siren Cycle Fiber");
                SirenCycleFiber.Start();
            }
            else
            {
                Game.LogTrivial("[RPE]: AISirenCycle is disabled.");
            }

            if (Settings.EnableSilentBackup)
            {
                Game.LogTrivial("[RPE]: SilentBackup is enabled.");
                GameFiber SilentBackupFiber = new GameFiber(() => SilentBackup.Main(), "RPE Silent Backup Fiber");
                SilentBackupFiber.Start();
            }
            else
            {
                Game.LogTrivial("[RPE]: SilentBackup is disabled.");
            }

            if (Settings.EnableTVI)
            {
                Game.LogTrivial("[RPE]: TVI is enabled.");
                GameFiber TVIFiber = new GameFiber(() => TVI.Main(), "RPE TVI Fiber");
                TVIFiber.Start();
            }
            else
            {
                Game.LogTrivial("[RPE]: TVI is disabled.");
            }

            if (Settings.EnableBOLO)
            {
                Game.LogTrivial("[RPE]: BOLO is enabled.");
                GameFiber BOLOFiber = new GameFiber(() => BOLO.Main(), "RPE BOLO Fiber");
                BOLOFiber.Start();
            }
            else
            {
                Game.LogTrivial("[RPE]: BOLO is disabled.");
            }

            if (Settings.EnablePRT)
            {
                Game.LogTrivial("[RPE]: PriorityRadioTraffic is enabled.");
                GameFiber PRTFiber = new GameFiber(() => PriorityRadioTraffic.Main(DependencyChecker.DoesPluginExist("PoliceSmartRadio.dll"), DependencyChecker.DoesPluginExist("VocalDispatch.dll"), DependencyChecker.DoesPluginExist("GrammarPolice.dll")), "RPE Priority Radio Traffic Fiber");
                PRTFiber.Start();
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
                GameFiber SuspectStaminaFiber = new GameFiber(() => SuspectStamina.Main(), "RPE Suspect Stamina Fiber");
                SuspectStaminaFiber.Start();
            }
            else
            {
                Game.LogTrivial("[RPE]: SuspectStamina is disabled.");
            }
        }

        private static void GetAssemblyVersion()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Game.LogTrivial($"Rich's Police Enhancements V{version} is ready.");
        }
    }
}