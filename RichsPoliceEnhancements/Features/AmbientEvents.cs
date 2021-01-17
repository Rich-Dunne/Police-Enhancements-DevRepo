using System;
using System.Collections.Generic;
using Rage;
using LSPD_First_Response.Mod.API;
using System.Linq;

namespace RichsPoliceEnhancements.Features
{
    internal class AmbientEvents
    {
        internal static List<string> CommonEvents { get; private set; } = new List<string>();
        internal static List<string> UncommonEvents { get; private set; } = new List<string>();
        internal static List<string> RareEvents { get; private set; } = new List<string>();
        private static bool EventActive { get; set; } = false;

        internal enum EventFrequency
        {
            Common = 0,
            Uncommon = 1,
            Rare = 2
        }

        internal static void Main()
        {
            CommonEvents.AddRange(Settings.EventFrequencies.Where(x => x.Value == "common").Select(x => x.Key).ToList());
            UncommonEvents.AddRange(Settings.EventFrequencies.Where(x => x.Value == "uncommon").Select(x => x.Key).ToList());
            RareEvents.AddRange(Settings.EventFrequencies.Where(x => x.Value == "rare").Select(x => x.Key).ToList());
            //var commonEvents = Settings.EventFrequencies.Where(x => x.Value == "common").Select(x => x.Key).ToList();
            //var uncommonEvents = Settings.EventFrequencies.Where(x => x.Value == "uncommon").Select(x => x.Key).ToList();
            //var rareEvents = Settings.EventFrequencies.Where(x => x.Value == "rare").Select(x => x.Key).ToList();
            Game.LogTrivial($"[RPE Ambient Event]: Common events: {CommonEvents.Count}, Uncommon events: {UncommonEvents.Count}, Rare events: {RareEvents.Count}");
            Game.LogTrivial($"[RPE Ambient Event]: Common Frequency: {Settings.CommonEventFrequency}, Uncommon Frequency: {Settings.UncommonEventFrequency}, Rare Frequency: {Settings.RareEventFrequency}");

            BeginLoopingForEvents();
        }

        private static void BeginLoopingForEvents()
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
                if (EventActive)
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
                else if (!Functions.IsPlayerAvailableForCalls())
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player not available for calls.");
                    return true;
                }
                else
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player busy state not recognized.");
                    return false;
                }
            }
        }

        private static void SelectEvent()
        {
            var randomValue = new Random().Next(1, 100); // 40 for testing
            Game.LogTrivial($"[RPE Ambient Event]: Choosing random event ({randomValue}).");
            
            if (randomValue <= Settings.CommonEventFrequency && CommonEvents.Count > 0)
            {
                var commonEvent = CommonEvents[new Random().Next(CommonEvents.Count)];
                if (string.IsNullOrEmpty(commonEvent))
                {
                    Game.LogTrivial($"[RPE Ambient Event]: No common event found.");
                    return;
                }
                InitializeNewEvent(commonEvent);
            }
            else if (randomValue > Settings.CommonEventFrequency && randomValue <= Settings.CommonEventFrequency + Settings.UncommonEventFrequency && UncommonEvents.Count > 0)
            {
                var uncommonEvent = UncommonEvents[new Random().Next(UncommonEvents.Count)];
                if (string.IsNullOrEmpty(uncommonEvent))
                {
                    Game.LogTrivial($"[RPE Ambient Event]: No uncommon event found.");
                    return;
                }
                InitializeNewEvent(uncommonEvent);
            }
            else if (randomValue > Settings.CommonEventFrequency + Settings.UncommonEventFrequency && RareEvents.Count > 0)
            {
                var rareEvent = RareEvents[new Random().Next(RareEvents.Count)];
                if (string.IsNullOrEmpty(rareEvent))
                {
                    Game.LogTrivial($"[RPE Ambient Event]: No rare event found.");
                    return;
                }
                InitializeNewEvent(rareEvent);
            }
        }

        private static void InitializeNewEvent(string @event)
        {
            Game.LogTrivial($"[RPE Ambient Event]: Starting {@event} event.");
            Game.LogTrivial($"[RPE Ambient Event]: Setting EventActive to true");
            EventActive = true;
            EventType eventType = (EventType)Enum.Parse(typeof(EventType), @event);
            new AmbientEvent(eventType);
        }

        internal static void SetEventActiveFalse()
        {
            EventActive = false;
        }
    }
}
