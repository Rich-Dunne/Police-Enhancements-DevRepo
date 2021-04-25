using Rage;
using LSPD_First_Response.Mod.API;
using System.IO;

namespace RichsPoliceEnhancements.Utils
{
    internal static class PlayAudioFiles
    {
        internal static void Start()
        {
            string directory = Directory.GetCurrentDirectory() + "\\lspdfr\\audio\\scanner\\STREETS\\ProblemFiles";
            GameFiber.StartNew(() =>
            {
                string[] files = Directory.GetFiles(directory, "*.wav");
                foreach(string file in files)
                {
                    Game.LogTrivial($"{file}");
                    var streetAudio = $"{Path.GetFileNameWithoutExtension(file).Replace(" ", "_").ToUpper()}";
                    Game.LogTrivial($"{streetAudio}");
                    Functions.PlayScannerAudio(streetAudio);
                    GameFiber.Sleep(5000);
                }
            }, "Play Audio Files Fiber");
        }
    }
}
