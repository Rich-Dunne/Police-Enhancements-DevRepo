using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichsPoliceEnhancements.Utils
{
    internal static class HelperMethods
    {
        internal static List<Ped> GetReleventPedsForAmbientEvent() => World.GetAllPeds().Where(p => p.IsRelevantForAmbientEvent()).ToList();
    }
}
