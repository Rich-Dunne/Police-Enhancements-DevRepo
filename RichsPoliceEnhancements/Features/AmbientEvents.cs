using System;
using System.Collections.Generic;
using Rage;
using LSPD_First_Response.Mod.API;
using System.Linq;

namespace RichsPoliceEnhancements.Features
{
    internal class AmbientEvents
    {
        private static bool _eventActive = false;
        internal static AmbientEvent ambientEvent;

        internal enum EventFrequency
        {
            Common = 0,
            Uncommon = 1,
            Rare = 2
        }

        internal static void Main()
        {
            var commonEvents = Settings.eventFrequencies.Where(x => x.Value == "common").Select(x => x.Key).ToList();
            var uncommonEvents = Settings.eventFrequencies.Where(x => x.Value == "uncommon").Select(x => x.Key).ToList();
            var rareEvents = Settings.eventFrequencies.Where(x => x.Value == "rare").Select(x => x.Key).ToList();
            Game.LogTrivial($"[RPE Ambient Event]: Common events: {commonEvents.Count}, Uncommon events: {uncommonEvents.Count}, Rare events: {rareEvents.Count}");
            Game.LogTrivial($"[RPE Ambient Event]: Common Frequency: {Settings.CommonEventFrequency}, Uncommon Frequency: {Settings.UncommonEventFrequency}, Rare Frequency: {Settings.RareEventFrequency}");

            BeginLoopingForEvents(commonEvents, uncommonEvents, rareEvents);
        }

        private static void BeginLoopingForEvents(List<string> commonEvents, List<string> uncommonEvents, List<string> rareEvents)
        { 
            Game.LogTrivial($"[RPE Ambient Event]: Pre-event loop initialized.");
            while (true)
            {
                GameFiber.Sleep(Settings.EventCooldownTimer); //20000 for testing or Settings.EventCooldownTimer for release
                if(PlayerIsBusy())
                {
                    Game.LogTrivial($"[RPE Ambient Event]: The player is busy, try again later.");
                    continue;
                }
                if (_eventActive)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: An event is already running.");
                    continue;
                }

                SelectEvent();
            }

            bool PlayerIsBusy()
            {
                if (Functions.IsCalloutRunning())
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player busy, callout running/being dispatched.");
                    return true;
                }
                else if (Functions.GetActivePursuit() != null)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player busy, pursuit active.");
                    return true;
                }
                else if (Functions.IsPlayerAvailableForCalls())
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player not busy.");
                    return false;
                }
                else
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player busy state not recognized.");
                    return false;
                }
            }

            void SelectEvent()
            {
                var randomValue = GetRandomNumber(100); // GetRandomNumber(100); or 40 for testing
                Game.LogTrivial($"[RPE Ambient Event]: Choosing random event ({randomValue}).");
                if (randomValue <= Settings.CommonEventFrequency && commonEvents.Count > 0)
                {
                    var commonEvent = commonEvents[new Random().Next(commonEvents.Count)];
                    if (string.IsNullOrEmpty(commonEvent))
                    {
                        Game.LogTrivial($"[RPE Ambient Event]: No common event found.");
                        return;
                    }
                    Game.LogTrivial($"[RPE Ambient Event]: Starting {commonEvent} event.");
                    Game.LogTrivial($"[RPE Ambient Event]: Setting _eventActive to true");
                    _eventActive = true;
                    EventType eventType = (EventType)Enum.Parse(typeof(EventType), commonEvent);
                    ambientEvent = new AmbientEvent(eventType);
                }
                else if (randomValue > Settings.CommonEventFrequency && randomValue <= Settings.CommonEventFrequency + Settings.UncommonEventFrequency && uncommonEvents.Count > 0)
                {
                    var uncommonEvent = uncommonEvents[new Random().Next(uncommonEvents.Count)];
                    if (string.IsNullOrEmpty(uncommonEvent))
                    {
                        Game.LogTrivial($"[RPE Ambient Event]: No uncommon event found.");
                        return;
                    }
                    Game.LogTrivial($"[RPE Ambient Event]: Starting {uncommonEvent} event.");
                    Game.LogTrivial($"[RPE Ambient Event]: Setting _eventActive to true");
                    _eventActive = true;
                    EventType eventType = (EventType)Enum.Parse(typeof(EventType), uncommonEvent);
                    new AmbientEvent(eventType);
                }
                else if (randomValue > Settings.CommonEventFrequency + Settings.UncommonEventFrequency && rareEvents.Count > 0)
                {
                    var rareEvent = rareEvents[new Random().Next(rareEvents.Count)];
                    if (string.IsNullOrEmpty(rareEvent))
                    {
                        Game.LogTrivial($"[RPE Ambient Event]: No rare event found.");
                        return;
                    }
                    Game.LogTrivial($"[RPE Ambient Event]: Starting {rareEvent} event.");
                    Game.LogTrivial($"[RPE Ambient Event]: Setting _eventActive to true");
                    _eventActive = true;
                    EventType eventType = (EventType)Enum.Parse(typeof(EventType), rareEvent);
                    new AmbientEvent(eventType);
                }
            }

            int GetRandomNumber(int maxValue) => new Random().Next(1, maxValue);
        }

        internal static void SetEventActiveFalse()
        {
            _eventActive = false;
            //Game.LogTrivial($"[RPE Ambient Event]: _eventActive is {_eventActive}");
        }
    }
}
