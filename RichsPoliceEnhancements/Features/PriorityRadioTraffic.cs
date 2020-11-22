using System;
using System.IO;
using Rage;
using VocalDispatchAPIExample;
using GrammarPolice.API;
using LSPD_First_Response.Mod.API;

namespace RichsPoliceEnhancements
{
    internal static class PriorityRadioTraffic
    {
        private static bool PRT = false, AudioLooping = false;
        private static System.Media.SoundPlayer player = new System.Media.SoundPlayer(Directory.GetCurrentDirectory() + @"\lspdfr\audio\sfx\PRTTone.wav");
        internal static VocalDispatchHelper VDPRTRequest = new VocalDispatchHelper();
        internal static VocalDispatchHelper VDPRTCancel = new VocalDispatchHelper();

        internal static void Main(bool policeSmartRadioInstalled, bool vocalDispatchInstalled, bool grammarPoliceInstalled)
        {
            if (Settings.AutomaticPRT)
            {
                LSPD_First_Response.Mod.API.Events.OnPursuitStarted += OnPursuitStarted;
            }

            if (policeSmartRadioInstalled)
            {
                Game.LogTrivial("[RPE Priority Radio Traffic]: PoliceSmartRadio is installed.");
                AddButtonToPSR();
            }

            if (grammarPoliceInstalled)
            {
                Game.LogTrivial("[RPE Priority Radio Traffic]: GrammarPolice is installed.");
                HandleGPActionEvent();
            }
            else if (vocalDispatchInstalled)
            {
                Game.LogTrivial("[RPE Priority Radio Traffic]: VocalDispatch is installed.");
                HandleVDFunctions();
            }

            void InitAudioLoopFiber()
            {
                TogglePRT(!PRT);
            }

            void AddButtonToPSR()
            {
                Action audioLoopAction = new Action(InitAudioLoopFiber);
                PoliceSmartRadio.API.Functions.AddActionToButton(InitAudioLoopFiber, "prt");
            }

            void HandleGPActionEvent()
            {
                GrammarPolice.API.Events.OnAction += Events_OnAction;
            }

            void HandleVDFunctions()
            {
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Utilities.ResolveAssemblyEventHandler);
                VDPRTRequest.SetupVocalDispatchAPI("RichsPoliceEnhancements.RequestPriorityTraffic", new Utilities.VocalDispatchEventDelegate(VocalDispatchSaysPlayerIsRequestingPriorityTraffic));
                VDPRTCancel.SetupVocalDispatchAPI("RichsPoliceEnhancements.CancelPriorityTraffic", new Utilities.VocalDispatchEventDelegate(VocalDispatchSaysPlayerIsCancelingPriorityTraffic));
            }
        }

        private static void Events_OnAction(string action)
        {
            if (action == "panic" && Settings.AutomaticPRT)
            {
                TogglePRT(true);
            }
        }

        private static bool VocalDispatchSaysPlayerIsRequestingPriorityTraffic() // Called by VocalDispatch
        {
            //Do your desired logic here. Returning false back to VocalDispatch will tell it to continue handling the request.
            //Game.DisplayNotification("VocalDispatch handled the request for priority radio traffic.");
            if (!PRT)
            {
                TogglePRT(true);
            }
            else
            {
                Game.LogTrivial("[RPE Priority Radio Traffic]: Priority radio traffic is already enabled.");
            }
            return true;
        }

        private static bool VocalDispatchSaysPlayerIsCancelingPriorityTraffic() //You do not need to call this function. VocalDispatch calls it for you once you've set it up properly.
        {
            //Do your desired logic here. Returning false back to VocalDispatch will tell it to continue handling the request.
            //Game.DisplayNotification("VocalDispatch handled the request for canceling priority radio traffic.");
            if (PRT)
            {
                TogglePRT(false);
            }
            else
            {
                Game.LogTrivial("[RPE Priority Radio Traffic]: Priority radio traffic is already disabled.");
            }
            return true;
        }

        public static void TogglePRT(bool enabled)
        {
            PRT = enabled;
            if (PRT && AudioLooping)
            {
                Game.LogTrivial($"[RPE PRT]: Priority radio tone is already playing.");
                return;
            }
            if (!PRT && !AudioLooping)
            {
                Game.LogTrivial($"[RPE PRT]: Priority radio tone is not playing.");
                return;
            }

            if (PRT)
            {
                Game.DisplayNotification($"~y~~h~DISPATCH - PRIORITY RADIO TRAFFIC ALERT~h~\n~s~~w~All units ~r~clear this channel~w~ for priority radio traffic.");
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio($"ATTENTION_THIS_IS_DISPATCH");
                GameFiber.Sleep(3000);
                AudioLoop();
            }
            else if (!PRT)
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio($"ATTENTION_THIS_IS_DISPATCH");
                Game.DisplayNotification($"~y~~h~DISPATCH - PRIORITY RADIO TRAFFIC ALERT~h~\n~s~~w~All units be advised, priority radio traffic has been canceled.  This channel is now ~g~open~w~.");
            }
        }

        private static void AudioLoop()
        {
            GameFiber.StartNew(() =>
            {
                while (PRT)
                {
                    AudioLooping = true;
                    player.Play();
                    GameFiber.Sleep(Settings.PRTToneTimer * 1000);
                }
                AudioLooping = false;
            }, "PRT Audio Loop Fiber");
        }

        private static void OnPursuitStarted(LHandle pursuit)
        {
            GameFiber.StartNew(() =>
            {
                TogglePRT(true);
                while (LSPD_First_Response.Mod.API.Functions.GetActivePursuit() != null)
                {
                    GameFiber.Sleep(1000);
                }
                TogglePRT(false);
            });
        }
    }
}
