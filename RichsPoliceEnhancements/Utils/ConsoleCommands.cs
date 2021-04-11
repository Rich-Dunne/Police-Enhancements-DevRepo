﻿using Rage.Attributes;
using Rage.ConsoleCommands.AutoCompleters;
using LSPD_First_Response.Mod.API;
using Rage;
using System.Linq;
using RichsPoliceEnhancements.Features;
using System.Reflection;

namespace RichsPoliceEnhancements.Utils
{
    [Obfuscation(Exclude = false, Feature = "-rename", ApplyToMembers = false)]
    internal static class ConsoleCommands
    {
        [ConsoleCommand("ForceEndPursuit")]
        internal static void Command_ForceEndPursuit([ConsoleCommandParameter(AutoCompleterType = typeof(ConsoleCommandAutoCompleterBoolean), Name = "ForceEndPursuit")] bool enabled = true)
        {
            if(!enabled)
            {
                return;
            }
            if(Functions.GetActivePursuit() == null)
            {
                Game.LogTrivial($"[RPE]: There is no active pursuit to force end.");
                return;
            }

            var pursuit = Functions.GetActivePursuit();
            DismissPursuitPeds(pursuit);
            Functions.ForceEndPursuit(pursuit);

        }

        private static void DismissPursuitPeds(LHandle pursuit)
        {
            var pursuitPeds = Functions.GetPursuitPeds(pursuit);
            foreach(Ped ped in pursuitPeds.Where(x => x != Game.LocalPlayer.Character))
            {
                ped.Dismiss();
            }
        }

        [ConsoleCommand("BeginAmbientEvent")]
        internal static void Command_BeginAmbientEvent([ConsoleCommandParameter(AutoCompleterType = typeof(ConsoleCommandParameterAutoCompleterEnum), Name = "BeginAmbientEvent")] EventType eventType)
        {
            AmbientEvents.InitializeNewEvent(eventType);
        }

        [ConsoleCommand("EndAmbientEvent")]
        internal static void Command_EndAmbientEvent([ConsoleCommandParameter(AutoCompleterType = typeof(ConsoleCommandAutoCompleterBoolean), Name = "EndAmbientEvent")] bool end = true)
        {
            if(AmbientEvents.ActiveEvent == null)
            {
                Game.LogTrivial($"ActiveEvent is null.");
                return;
            }

            AmbientEvents.ActiveEvent.TransitionToState(State.Ending);
        }

        //[ConsoleCommand("PlayAudioFiles")]
        //internal static void Command_PlayAudioFiles([ConsoleCommandParameter(AutoCompleterType = typeof(ConsoleCommandAutoCompleterBoolean), Name = "PlayAudioFiles")] bool enabled = true)
        //{
        //    PlayAudioFiles.Start();
        //}
    }
}
