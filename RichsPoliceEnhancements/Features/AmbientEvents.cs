using System;
using System.Collections.Generic;
using Rage;
using LSPD_First_Response.Mod.API;
using System.Linq;

namespace RichsPoliceEnhancements
{
    class AmbientEvents
    {

        private static AmbientEvent ambientEvent;

        internal enum EventFrequency
        {
            Common = 0,
            Uncommon = 1,
            Rare = 2
        }

        internal static void Main()
        {
            AppDomain.CurrentDomain.DomainUnload += TerminationHandler;
            var commonEvents = Settings.eventFrequencies.Where(x => x.Value == "common").Select(x => x.Key).ToList();
            var uncommonEvents = Settings.eventFrequencies.Where(x => x.Value == "uncommon").Select(x => x.Key).ToList();
            var rareEvents = Settings.eventFrequencies.Where(x => x.Value == "rare").Select(x => x.Key).ToList();
            Game.LogTrivial($"[RPE Ambient Event]: Common events: {commonEvents.Count}, Uncommon events: {uncommonEvents.Count}, Rare events: {rareEvents.Count}");
            Game.LogTrivial($"[RPE Ambient Event]: Common Frequency: {Settings.CommonEventFrequency}, Uncommon Frequency: {Settings.UncommonEventFrequency}, Rare Frequency: {Settings.RareEventFrequency}");

            BeginLoopingForEvents(commonEvents, uncommonEvents, rareEvents);
        }

        private static void BeginLoopingForEvents(List<string> commonEvents, List<string> uncommonEvents, List<string> rareEvents)
        { 
            bool eventActive = false;

            Game.LogTrivial($"[RPE Ambient Event]: Pre-event loop initialized.");
            while (true)
            {
                GameFiber.Sleep(20000); //20000 for testing or Settings.EventCooldownTimer for release
                if (!PlayerIsBusy() && !eventActive)
                {
                    SelectEvent();
                }
            }

            bool PlayerIsBusy()
            {
                if (Functions.IsCalloutRunning())
                {
                    Game.LogTrivial($"[RPE Ambient Event] Player busy, callout running/being dispatched.");
                    return true;
                }
                else if (Functions.GetActivePursuit() != null)
                {
                    Game.LogTrivial($"[RPE Ambient Event] Player busy, pursuit active.");
                    return true;
                }
                else if (Functions.IsPlayerAvailableForCalls())
                {
                    Game.LogTrivial($"[RPE Ambient Event] Player not busy.");
                    return false;
                }
                else
                {
                    Game.LogTrivial($"[RPE Ambient Event] Player busy state not recognized.");
                    return false;
                }
            }

            void SelectEvent()
            {
                int randomValue = GetRandomNumber(100);
                Game.LogTrivial($"[RPE Ambient Event] Choosing random event ({randomValue}).");
                if (randomValue > Settings.UncommonEventFrequency)
                {
                    var commonEvent = commonEvents[new Random().Next(commonEvents.Count)];
                    if (string.IsNullOrEmpty(commonEvent))
                    {
                        Game.LogTrivial($"[RPE Ambient Event] No common event found.");
                        return;
                    }

                    EventType eventType = (EventType)Enum.Parse(typeof(EventType), commonEvent);
                    ambientEvent = new AmbientEvent(eventType);
                }
                else if (randomValue <= Settings.UncommonEventFrequency && randomValue > Settings.RareEventFrequency)
                {
                    var uncommonEvent = uncommonEvents[new Random().Next(uncommonEvents.Count)];
                    if (string.IsNullOrEmpty(uncommonEvent))
                    {
                        Game.LogTrivial($"[RPE Ambient Event] No uncommon event found.");
                        return;
                    }

                    EventType eventType = (EventType)Enum.Parse(typeof(EventType), uncommonEvent);
                    new AmbientEvent(eventType);
                }
                else if (randomValue <= Settings.RareEventFrequency)
                {
                    var rareEvent = rareEvents[new Random().Next(rareEvents.Count)];
                    if (string.IsNullOrEmpty(rareEvent))
                    {
                        Game.LogTrivial($"[RPE Ambient Event] No rare event found.");
                        return;
                    }

                    EventType eventType = (EventType)Enum.Parse(typeof(EventType), rareEvent);
                    new AmbientEvent(eventType);
                }
            }

            int GetRandomNumber(int maxValue) => new Random().Next(0, maxValue);
        }

        private static void TerminationHandler(object sender, EventArgs e)
        {
            if(ambientEvent != null)
            {
                ambientEvent.Cleanup();
            }

            Game.LogTrivial("[RPE Ambient Event]: Plugin terminated.");
        }
    }
}
