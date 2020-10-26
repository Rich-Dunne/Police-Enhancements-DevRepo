using System;
using System.Collections.Generic;
using Rage;
using LSPD_First_Response.Mod.API;

namespace RichsPoliceEnhancements
{
    class EventSelect
    {
        private static Ped player = Game.LocalPlayer.Character; // Assign player's character to ped 'player'
        private static AmbientEvent ambientEvent;

        public static void Main()
        {
            bool eventActive = false;
            List<string> commonEvents = new List<string>();
            List<string> uncommonEvents = new List<string>();
            List<string> rareEvents = new List<string>();

            // ambientWorldEvent = new AmbientEvent("RoadRage", player); // Should loop constantly checking for vehicle collision with another vehicle with a random chance for road rage.

            foreach (KeyValuePair<string, string> eventFrequency in Settings.eventFrequencies)
            {
                //Game.LogTrivial($"[Rich Ambiance] {eventFrequency.Key}: {eventFrequency.Value}");
                switch (eventFrequency.Value)
                {
                    case "common":
                        commonEvents.Add(eventFrequency.Key);
                        break;

                    case "uncommon":
                        uncommonEvents.Add(eventFrequency.Key);
                        break;

                    case "rare":
                        rareEvents.Add(eventFrequency.Key);
                        break;
                }
            }
            Game.LogTrivial($"[Rich Ambiance] Common events: {commonEvents.Count}, Uncommon events: {uncommonEvents.Count}, Rare events: {rareEvents.Count}");
            Game.LogTrivial($"[Rich Ambiance] Common Frequency: {Settings.CommonEventFrequency}, Uncommon Frequency: {Settings.UncommonEventFrequency}, Rare Frequency: {Settings.RareEventFrequency}");


            Game.LogTrivial($"[Rich Ambiance] Pre-event loop initialized.");
            while (true)
            {
                GameFiber.Sleep(Settings.EventCooldownTimer); //20000 for testing or eventCooldownTimer for release
                if (!PlayerIsBusy(player))
                {
                    if (ambientEvent == null)
                    {
                        int r = RandomNumber(100);
                        Game.LogTrivial($"[Rich Ambiance] Choosing random event ({r}).");
                        switch (r)
                        {
                            case var expression when r < Settings.CommonEventFrequency:
                                Game.LogTrivial($"[Rich Ambiance] Starting random common event.");
                                try
                                {
                                    ambientEvent = new AmbientEvent(commonEvents[new Random().Next(commonEvents.Count)], player);
                                }
                                catch
                                {
                                    Game.LogTrivial($"[Rich Ambiance] There are no common events.  Check the .ini settings");
                                }
                                break;
                            case var expression when r >= Settings.CommonEventFrequency && r < Settings.UncommonEventFrequency:
                                Game.LogTrivial($"[Rich Ambiance] Starting random uncommon event.");
                                try
                                {
                                    ambientEvent = new AmbientEvent(uncommonEvents[new Random().Next(uncommonEvents.Count)], player);
                                }
                                catch
                                {
                                    Game.LogTrivial($"[Rich Ambiance] There are no uncommon events.  Check the .ini settings");
                                }
                                break;
                            case var expression when r >= Settings.RareEventFrequency:
                                Game.LogTrivial($"[Rich Ambiance] Starting random rare event.");
                                try
                                {
                                    ambientEvent = new AmbientEvent(rareEvents[new Random().Next(rareEvents.Count)], player);
                                }
                                catch
                                {
                                    Game.LogTrivial($"[Rich Ambiance] There are no rare events.  Check the .ini settings");
                                }
                                break;
                        }
                        ambientEvent = null;
                    }
                    else if (ambientEvent != null && eventActive)
                    {
                        Game.LogTrivial($"[Rich Ambiance] An event is currently active.");
                    }
                }
            }
        }

        // Function to get random number between 0 and i
        public static int RandomNumber(int i)
        {
            while (true)
            {
                int r = new Random().Next(0, i);
                //Game.LogTrivial("[Rich Ambiance] r is " + r + ".");
                return r;
            }
        }

        private static bool PlayerIsBusy(Ped player)
        {
            if (Functions.IsCalloutRunning())
            {
                Game.LogTrivial($"[Rich Ambiance] Player busy, callout running/being dispatched.");
                return true;
            }
            else if (Functions.GetActivePursuit() != null)
            {
                Game.LogTrivial($"[Rich Ambiance] Player busy, pursuit active.");
                return true;
            }
            else if (Functions.IsPlayerAvailableForCalls())
            {
                Game.LogTrivial($"[Rich Ambiance] Player not busy.");
                return false;
            }
            else
            {
                Game.LogTrivial($"[Rich Ambiance] Player busy state not recognized.");
                return false;
            }
        }
    }
}
