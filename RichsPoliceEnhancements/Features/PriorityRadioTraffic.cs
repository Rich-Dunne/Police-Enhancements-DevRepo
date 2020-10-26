using System;
using System.IO;
using System.Reflection;
using Rage;
using LSPD_First_Response.Mod.API;
using VocalDispatchAPIExample;
using System.Linq;

namespace RichsPoliceEnhancements
{
    internal static class PriorityRadioTraffic
    {
        internal static bool PRT = false;
        internal static VocalDispatchHelper VDPRTRequest = new VocalDispatchHelper();
        internal static VocalDispatchHelper VDPRTCancel = new VocalDispatchHelper();

        internal static void Main()
        {
            if (IsPluginLoaded("PoliceSmartRadio"))
            {
                Game.LogTrivial("[RPE Priority Radio Traffic]: PSR is installed.");
                Action audioLoopAction = new Action(InitAudioLoopFiber);
                PoliceSmartRadio.API.Functions.AddActionToButton(InitAudioLoopFiber, "prt");
            }
            else
            {
                Game.LogTrivial("[RPE Priority Radio Traffic]: This feature requires PoliceSmartRadio, but it was not found.");
                return;
            }

            if (IsPluginLoaded("VocalDispatch"))
            {
                Game.LogTrivial("[RPE Priority Radio Traffic]: VocalDispatch is installed.");
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Utilities.ResolveAssemblyEventHandler);
                VDPRTRequest.SetupVocalDispatchAPI("RichsPoliceEnhancements.RequestPriorityTraffic", new Utilities.VocalDispatchEventDelegate(VocalDispatchSaysPlayerIsRequestingPriorityTraffic));
                VDPRTCancel.SetupVocalDispatchAPI("RichsPoliceEnhancements.CancelPriorityTraffic", new Utilities.VocalDispatchEventDelegate(VocalDispatchSaysPlayerIsCancelingPriorityTraffic));
            }

            bool IsPluginLoaded(string pluginName) => Functions.GetAllUserPlugins().ToList().Any(a => a.FullName.Contains(pluginName));

            void InitAudioLoopFiber()
            {
                GameFiber PRTLoopFiber = new GameFiber(() => AudioLoop());
                PRTLoopFiber.Start();
            }
        }

        private static bool VocalDispatchSaysPlayerIsRequestingPriorityTraffic() // Called by VocalDispatch
        {
            //Do your desired logic here. Returning false back to VocalDispatch will tell it to continue handling the request.
            //Game.DisplayNotification("VocalDispatch handled the request for priority radio traffic.");
            if (!PRT)
            {
                GameFiber PRTLoopFiber = new GameFiber(() => AudioLoop());
                PRTLoopFiber.Start();
            }
            else
            {
                Game.LogTrivial("[RPE Priority Radio Traffic]: Priority radio traffic is already enabled.");
            }
            return true;
        }

        // This always fires and cannot use VD while priority traffic true
        private static bool VocalDispatchSaysPlayerIsCancelingPriorityTraffic() //You do not need to call this function. VocalDispatch calls it for you once you've set it up properly.
        {
            //Do your desired logic here. Returning false back to VocalDispatch will tell it to continue handling the request.
            //Game.DisplayNotification("VocalDispatch handled the request for canceling priority radio traffic.");
            if (PRT)
            {
                GameFiber PRTLoopFiber = new GameFiber(() => AudioLoop());
                PRTLoopFiber.Start();
            }
            else
            {
                Game.LogTrivial("[RPE Priority Radio Traffic]: Priority radio traffic is already disabled.");
            }
            return true;
        }

        private static void AudioLoop()
        {
            if (!PRT)
            {
                Game.DisplayNotification($"~y~~h~DISPATCH - PRIORITY RADIO TRAFFIC ALERT~h~\n~s~~w~All units ~r~clear this channel~w~ for priority radio traffic.");
                PRT = true;
                GameFiber.Sleep(3000);
                while (PRT)
                {
                    GameFiber.Yield();
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(Directory.GetCurrentDirectory() + @"\lspdfr\audio\sfx\PRTTone.wav");
                    player.Play();
                    GameFiber.Sleep(Settings.PRTToneTimer * 1000);
                }
            }
            else
            {
                Game.DisplayNotification($"~y~~h~DISPATCH - PRIORITY RADIO TRAFFIC ALERT~h~\n~s~~w~All units be advised, priority radio traffic has been canceled.  This channel is now ~g~open.");
                PRT = false;
            }
        }
    }
}
