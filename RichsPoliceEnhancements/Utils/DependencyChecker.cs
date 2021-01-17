using System.IO;

namespace RichsPoliceEnhancements.Utils
{
    class DependencyChecker
    {
        internal static bool DoesPluginExist(string plugin)
        {
            var currentDirectory = Directory.GetCurrentDirectory() + "\\plugins\\lspdfr\\";
            if (File.Exists($"{currentDirectory}{plugin}"))
            {
                //Game.LogTrivial($"{plugin} is installed.");
                return true;
            }
            else
            {
                //Game.LogTrivial($"{plugin} is NOT installed.");
                return false;
            }
        }
    }
}
