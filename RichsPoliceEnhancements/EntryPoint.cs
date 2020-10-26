﻿using Rage;
using LSPD_First_Response.Mod.API;
using System.Reflection;
using System.Linq;

[assembly: Rage.Attributes.Plugin("Rich's Police Enhancements V1.5", Author = "Rich", Description = "Quality of life features for police AI")]

// PCP ambient event, shirtless ped with extra health
// If there are any random crashes from object reference not set to an instance of the object when a player re-enters their vehicle, make sure player.LastVehicle is not being
// redefined with every loop.  Set it outside the loop, then reference it inside the loop.

namespace RichsPoliceEnhancements
{
    public class Main : Plugin
    {
        public override void Initialize()
        {
            Settings.LoadSettings();

            //if (Settings.EnableAmbientEvents)
            //{
            //    Game.LogTrivial("[RPE]: AmbientEvents are enabled.");
            //    GameFiber AmbientEventsFiber = new GameFiber(() => EventSelect.Main(), "Ambient Event Main");
            //    AmbientEventsFiber.Start();
            //}
            //else
            //{
            //    Game.LogTrivial("[RPE]: AmbientEvents are disabled.");
            //}
            if (Settings.EnableAmbientBackup)
            {
                Game.LogTrivial("[RPE]: AmbientBackup is enabled.");
                GameFiber AmbientBackupFiber = new GameFiber(() => AmbientBackup.Main());
                AmbientBackupFiber.Start();
            }
            else
            {
                Game.LogTrivial("[RPE]: AmbientBackup is disabled.");
            }

            if (Settings.EnableAISirenCycle)
            {
                Game.LogTrivial("[RPE]: AISirenCycle is enabled.");
                GameFiber SirenCycleFiber = new GameFiber(() => AISirenCycle.Main());
                SirenCycleFiber.Start();
            }
            else
            {
                Game.LogTrivial("[RPE]: AISirenCycle is disabled.");
            }

            if (Settings.EnableSilentBackup)
            {
                Game.LogTrivial("[RPE]: SilentBackup is enabled.");
                GameFiber SilentBackupFiber = new GameFiber(() => SilentBackup.Main());
                SilentBackupFiber.Start();
            }
            else
            {
                Game.LogTrivial("[RPE]: SilentBackup is disabled.");
            }

            if (Settings.EnableTVI)
            {
                Game.LogTrivial("[RPE]: TVI is enabled.");
                GameFiber TVIFiber = new GameFiber(() => TVI.Main());
                TVIFiber.Start();
            }
            else
            {
                Game.LogTrivial("[RPE]: TVI is disabled.");
            }

            if (Settings.EnableBOLO)
            {
                Game.LogTrivial("[RPE]: BOLO is enabled.");
                Settings.BOLOTimer *= 60000;
                Settings.BOLOFrequency *= 60000;
                GameFiber BOLOFiber = new GameFiber(() => BOLO.Main(Settings.BOLOTimer, Settings.BOLOFrequency));
                BOLOFiber.Start();
            }
            else
            {
                Game.LogTrivial("[RPE]: BOLO is disabled.");
            }

            if (Settings.EnablePRT)
            {
                Game.LogTrivial("[RPE]: PriorityRadioTraffic is enabled.");
                GameFiber PRTFiber = new GameFiber(() => PriorityRadioTraffic.Main());
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

            GetAssemblyVersion();
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

        void GetAssemblyVersion()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Game.LogTrivial($"Rich's Police Enhancements V{version} is ready.");
        }
    }
}