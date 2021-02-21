using Rage.Attributes;
using Rage.ConsoleCommands.AutoCompleters;
using LSPD_First_Response.Mod.API;
using Rage;
using System.Linq;

namespace RichsPoliceEnhancements.Utils
{
    class ConsoleCommands
    {
        [ConsoleCommand("ForceEndPursuit")]
        internal static void Command_ForceEndPursuit([ConsoleCommandParameter(AutoCompleterType = typeof(ConsoleCommandAutoCompleterBoolean), Name = "ForceEndPursuit")] bool enabled = true)
        {
            if(enabled && Functions.GetActivePursuit() != null)
            {
                var pursuit = Functions.GetActivePursuit();
                DismissPursuitPeds(pursuit);
                Functions.ForceEndPursuit(pursuit);
            }
            else
            {
                Game.LogTrivial($"[RPE]: There is no active pursuit to force end.");
            }
        }

        private static void DismissPursuitPeds(LHandle pursuit)
        {
            var pursuitPeds = Functions.GetPursuitPeds(pursuit);
            foreach (Ped ped in pursuitPeds)
            {
                ped.Dismiss();
            }
        }
    }
}
