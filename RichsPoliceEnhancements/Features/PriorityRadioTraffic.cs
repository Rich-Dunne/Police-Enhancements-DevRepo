using System;
using System.IO;
using Rage;
using VocalDispatchAPIExample;
using LSPD_First_Response.Mod.API;
using System.Xml;
using System.Xml.Linq;

namespace RichsPoliceEnhancements.Features
{
    internal static class PriorityRadioTraffic
    {
        private static bool PRT { get; set; } = false;
        private static bool AudioLooping { get; set; } = false;
        private static System.Media.SoundPlayer SoundPlayer { get; } = new System.Media.SoundPlayer(Directory.GetCurrentDirectory() + @"\lspdfr\audio\sfx\PRTTone.wav");
        internal static VocalDispatchHelper VDPRTRequest { get; } = new VocalDispatchHelper();
        internal static VocalDispatchHelper VDPRTCancel { get; } = new VocalDispatchHelper();

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
                SubscribeGrammarPoliceOnActionEvent();
                CheckForOutdatedXMLs();
            }
            else if (vocalDispatchInstalled)
            {
                Game.LogTrivial("[RPE Priority Radio Traffic]: VocalDispatch is installed.");
                HandleVDFunctions();
            }
        }

        private static void CheckForOutdatedXMLs()
        {
            
            var pathToFile = Directory.GetCurrentDirectory() + "\\plugins\\LSPDFR\\GrammarPolice\\grammar\\en-US\\custom\\commands";
            
            // Remove requestPRT.xml
            var requestPRTExists = File.Exists(pathToFile + "\\requestPRT.xml");
            if(requestPRTExists)
            {
                Game.LogTrivial($"[RPE Priority Radio Traffic]: requestPRT needs to be deleted.");
                File.Delete(pathToFile + "\\requestPRT.xml");
                Game.LogTrivial($"[RPE Priority Radio Traffic]: requestPRT deleted successfully.");
            }
            else
            {
                Game.LogTrivial($"[RPE Priority Radio Traffic]: requestPRT does not exist.");
            }

            // Remove <Action> element from cancelPRT.xml
            XDocument document = XDocument.Load(pathToFile + "\\cancelPRT.xml");
            foreach (XElement element in document.Element("Command").Elements())
            {
                if (element.Name == "Action")
                {
                    element.Remove();
                    document.Save(pathToFile + "\\cancelPRT.xml");
                    Game.LogTrivial($"[RPE Priority Radio Traffic]: \"Action\" node removed from cancelPRT.xml");
                }
            }
        }

        private static void AddButtonToPSR()
        {
            Action audioLoopAction = new Action(InitAudioLoopFiber);
            PoliceSmartRadio.API.Functions.AddActionToButton(InitAudioLoopFiber, "prt");


            void InitAudioLoopFiber()
            {
                TogglePRT(!PRT);
            }
        }

        private static void SubscribeGrammarPoliceOnActionEvent()
        {
            GrammarPolice.API.Events.OnAction += GrammarPoliceEvents_OnAction;
        }

        private static void HandleVDFunctions()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Utilities.ResolveAssemblyEventHandler);
            VDPRTRequest.SetupVocalDispatchAPI("RichsPoliceEnhancements.RequestPriorityTraffic", new Utilities.VocalDispatchEventDelegate(VocalDispatchSaysPlayerIsRequestingPriorityTraffic));
            VDPRTCancel.SetupVocalDispatchAPI("RichsPoliceEnhancements.CancelPriorityTraffic", new Utilities.VocalDispatchEventDelegate(VocalDispatchSaysPlayerIsCancelingPriorityTraffic));
        }

        private static void GrammarPoliceEvents_OnAction(string action)
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
                if (!Settings.DisablePRTNotifications)
                {
                    Game.DisplayNotification($"~y~~h~DISPATCH - PRIORITY RADIO TRAFFIC ALERT~h~\n~s~~w~All units ~r~clear this channel~w~ for priority radio traffic.");
                }
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio($"ATTENTION_THIS_IS_DISPATCH");
                GameFiber.Sleep(3000);
                AudioLoop();
            }
            else
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio($"ATTENTION_THIS_IS_DISPATCH WE_ARE_CODE_4");
                if (!Settings.DisablePRTNotifications)
                {
                    Game.DisplayNotification($"~y~~h~DISPATCH - PRIORITY RADIO TRAFFIC ALERT~h~\n~s~~w~All units be advised, priority radio traffic has been canceled.  This channel is now ~g~open~w~.");
                }
            }
        }

        private static void AudioLoop()
        {
            GameFiber.StartNew(() =>
            {
                while (PRT)
                {
                    AudioLooping = true;
                    SoundPlayer.Play();
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
            }, "Automatic PRT Fiber");
        }
    }
}
