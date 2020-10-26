using System;
using System.IO;
using System.Reflection;

using Rage;
using LSPD_First_Response.Mod.API;
using VocalDispatchAPIExample;

namespace RichsPoliceEnhancements
{
    public class PriorityRadioTraffic
    {
        public static bool PRT = false;
        public static VocalDispatchHelper VDPRTRequest = new VocalDispatchHelper();
        public static VocalDispatchHelper VDPRTCancel = new VocalDispatchHelper();
        public static void Main()
        {
            bool PSR = false, VD = false;
            // Check if dependencies are installed / running
            foreach (Assembly a in Functions.GetAllUserPlugins())
            {
                if (a.FullName.Contains("PoliceSmartRadio"))
                {
                    Game.LogTrivial("[RPE Priority Radio Traffic]: PSR is installed.");
                    PSR = true;
                    Action audioLoopAction = new Action(InitAudioLoopFiber);
                    PoliceSmartRadio.API.Functions.AddActionToButton(InitAudioLoopFiber, "prt");
                }

                if (a.FullName.Contains("VocalDispatch"))
                {
                    Game.LogTrivial("[RPE Priority Radio Traffic]: VocalDispatch is installed.");
                    VD = true;

                    AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Utilities.ResolveAssemblyEventHandler);
                    VDPRTRequest.SetupVocalDispatchAPI("RichsPoliceEnhancements.RequestPriorityTraffic", new Utilities.VocalDispatchEventDelegate(VocalDispatchSaysPlayerIsRequestingPriorityTraffic));
                    VDPRTCancel.SetupVocalDispatchAPI("RichsPoliceEnhancements.CancelPriorityTraffic", new Utilities.VocalDispatchEventDelegate(VocalDispatchSaysPlayerIsCancelingPriorityTraffic));
                }
            }

            if (!PSR)
            {
                Game.LogTrivial("[RPE Priority Radio Traffic]: PoliceSmartRadio was not found or loaded correctly.");
            }
            if (!VD)
            {
                Game.LogTrivial("[RPE Priority Radio Traffic]: VocalDispatch was not found or loaded correctly.");
            }
        }

        public static bool VocalDispatchSaysPlayerIsRequestingPriorityTraffic() // Called by VocalDispatch
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
                Game.LogTrivial("[RPE Priority Radio Traffic]: Priority traffic is already enabled.");
            }
            return true;
        }

        // This always fires and cannot use VD while priority traffic true
        public static bool VocalDispatchSaysPlayerIsCancelingPriorityTraffic() //You do not need to call this function. VocalDispatch calls it for you once you've set it up properly.
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
                Game.LogTrivial("[RPE Priority Radio Traffic]: Priority traffic is already disabled.");
            }
            return true;
        }

        private static void InitAudioLoopFiber()
        {
            GameFiber PRTLoopFiber = new GameFiber(() => AudioLoop());
            PRTLoopFiber.Start();
        }

        private static void AudioLoop()
        {
            if (!PRT)
            {
                Game.DisplayNotification(string.Format("~r~~h~DISPATCH - PRIORITY TRAFFIC ALERT~h~~n~~s~~w~All units clear the channel for priority radio traffic."));
                PRT = true;
                GameFiber.Sleep(3000);
                while (PRT)
                {
                    GameFiber.Yield();
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(Directory.GetCurrentDirectory() + @"\lspdfr\audio\sfx\PRTTone.wav");
                    player.Play();
                    GameFiber.Sleep(RichsPoliceEnhancements.Settings.PRTToneTimer * 1000);
                }
                //Game.DisplayNotification(string.Format("~g~~h~DISPATCH - PRIORITY TRAFFIC ALERT~h~~n~~s~~w~All units be advised, priority radio traffic has been canceled.  This channel is now open."));
            }
            else
            {
                Game.DisplayNotification(string.Format("~g~~h~DISPATCH - PRIORITY TRAFFIC ALERT~h~~n~~s~~w~All units be advised, priority radio traffic has been canceled.  This channel is now open."));
                PRT = false;
            }
        }
    }
}
