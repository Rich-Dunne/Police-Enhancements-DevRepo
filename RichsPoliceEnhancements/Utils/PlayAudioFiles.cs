using Rage;
using LSPD_First_Response.Mod.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RichsPoliceEnhancements.Utils
{
    internal static class PlayAudioFiles
    {
        internal static void Start()
        {
            string directory = Directory.GetCurrentDirectory() + "\\lspdfr\\audio\\scanner\\STREETS";
            GameFiber.StartNew(() =>
            {
                string[] files = Directory.GetFiles(directory, "*.wav");
                foreach(string file in files)
                {
                    Game.LogTrivial($"{file}");
                    var streetAudio = $"{Path.GetFileNameWithoutExtension(file).Replace(" ", "_").ToUpper()}";
                    Game.LogTrivial($"{streetAudio}");
                    Functions.PlayScannerAudio(streetAudio);
                    GameFiber.Sleep(3000);
                }
            }, "Play Audio Files Fiber");
        }
    }
}
