using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichsPoliceEnhancements.Features
{
    class SuspectTrip
    {
        private static List<Ped> PursuitPeds { get; } = new List<Ped>();
        private static LHandle Pursuit { get; set; } = null;
        private static bool TripCheckStarted { get; set; } = false;

        internal static void Main()
        {
            while (true)
            {
                CheckForPursuit();
                if (Pursuit != null && PursuitPeds.Count == 0)
                {
                    CollectSuspects();
                }
                if (!TripCheckStarted && PursuitPeds.Count > 0)
                {
                    PursuitPeds.ForEach(x => GameFiber.StartNew(() => TryToTripPed(x), "Trip Chance Fiber"));
                    TripCheckStarted = true;
                }
                GameFiber.Yield();
            }
        }

        private static void CheckForPursuit()
        {
            if (Pursuit != null && Functions.GetActivePursuit() == null)
            {
                Game.LogTrivial($"[RPE Suspect Trip]: Setting pursuit as null.");
                Pursuit = null;
                PursuitPeds.Clear();
                TripCheckStarted = false;
            }
            if (Pursuit == null && Functions.GetActivePursuit() != null && PursuitPeds.Count == 0)
            {
                Game.LogTrivial($"[RPE Suspect Trip]: Assigning active pursuit to pursuit handle.");
                Pursuit = Functions.GetActivePursuit();
            }
        }

        private static void CollectSuspects()
        {
            PursuitPeds.AddRange(GetPursuitPeds());
            PursuitPeds.Remove(Game.LocalPlayer.Character);
        }

        private static List<Ped> GetPursuitPeds() => Functions.GetPursuitPeds(Functions.GetActivePursuit()).Where(x => x && x.IsAlive && x != Game.LocalPlayer.Character && !Functions.IsPedACop(x)).ToList();

        private static void TryToTripPed(Ped ped)
        {
            GameFiber.Sleep(new Random().Next(5000)); // Stagger the trip loops

            while (Pursuit != null && Functions.IsPursuitStillRunning(Pursuit) && ped && !Functions.IsPedGettingArrested(ped))
            {
                if (ped.IsOnFoot && new Random().Next(100) <= Settings.TripChance)
                {
                    Game.LogTrivial($"[RPE Suspect Trip]: Suspect tripped");
                    ped.IsRagdoll = true;
                    GameFiber.Sleep(500);
                    if (!ped)
                    {
                        return;
                    }
                    ped.IsRagdoll = false;  
                }
                GameFiber.Sleep(1000);
            }
        }
    }
}
