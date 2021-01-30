using System.Collections.Generic;
using Rage;
using LSPD_First_Response.Mod.API;
using System.Linq;
using System;

namespace RichsPoliceEnhancements
{
    internal static class DrugDealEventFunctions
    {
        internal static void BeginEvent(AmbientEvent @event)
        {
            var attempts = 1;
            var dealers = new List<Ped>();
            var buyers = new List<Ped>();
            Game.LogTrivial($"[RPE Ambient Event]: Starting DrugDeal event.");

            LoopToFindEventPeds();

            if (@event.EventPeds.Count < 2)
            {
                Game.LogTrivial($"[RPE Ambient Event]: Unable to find suitable event peds after 100 attempts.  Ending event.");
                return;
            }

            GameFiber DrugDealInteractionFiber = new GameFiber(() => EventProcess(@event), "RPE DrugDeal Interaction Fiber");
            DrugDealInteractionFiber.Start();

            void LoopToFindEventPeds()
            {
                while (@event.EventPeds.Count < 2 && attempts <= 100 && !Functions.IsCalloutRunning() && Functions.GetActivePursuit() == null)
                {
                    dealers = FindDealers();
                    buyers = FindBuyers();
                    FindEventPedPair();
                    if (@event.EventPeds.Count == 2)
                    {
                        Game.LogTrivial($"Success on attempt {attempts}");
                        break;
                    }
                    @event.EventPeds.Clear();
                    dealers.Clear();
                    buyers.Clear();
                    attempts++;
                    GameFiber.Sleep(500);
                }
            }

            List<Ped> NearbyPeds() => World.GetAllPeds().Where(p => PedIsRelevant(p)).ToList();

            bool PedIsRelevant(Ped ped)
            {
                if (ped && ped.IsAlive && ped.Position.DistanceTo(Game.LocalPlayer.Character.Position) < 100f && !ped.IsPlayer && !ped.IsInjured && !ped.Model.Name.Contains("A_C") && ped.RelationshipGroup != RelationshipGroup.Cop)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            List<Ped> FindDealers()
            {
                return NearbyPeds().Where(p => p.IsOnFoot && p.RelationshipGroup == RelationshipGroup.AmbientGangBallas || p.RelationshipGroup == RelationshipGroup.AmbientGangFamily || p.RelationshipGroup == RelationshipGroup.AmbientGangMexican).ToList();
            }

            List<Ped> FindBuyers()
            {
                return NearbyPeds().Where(p => !dealers.Contains(p) && p.IsOnFoot && dealers.Any(x => p.DistanceTo2D(x) <= 10f) && dealers.Any(x => Math.Abs(x.Position.Z - p.Position.Z) < 3f)).ToList();
            }

            void FindEventPedPair()
            {
                // If driver is within 20f of any ped from victims, assign driver and that victim ped as event peds
                var dealer = dealers.FirstOrDefault(x => buyers.Any(y => y.DistanceTo2D(x) <= 10f));
                if (!dealer)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: No dealers found with a suitable buyer nearby.");
                    @event.Cleanup();
                    return;
                }
                var buyer = buyers.FirstOrDefault(x => x.DistanceTo2D(dealer) <= 10f);
                if (!buyer)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: No buyer found within range of the dealer.");
                    @event.Cleanup();
                    return;
                }

                new EventPed(@event, dealer, Role.PrimarySuspect, true);
                new EventPed(@event, buyer, Role.SecondarySuspect, false);
            }
        }

        private static void EventProcess(AmbientEvent @event)
        {
            var dealer = @event.EventPeds.Where(x => x.Role == Role.PrimarySuspect).FirstOrDefault();
            var buyer = @event.EventPeds.Where(x => x.Role == Role.SecondarySuspect).FirstOrDefault();
            Game.LogTrivial($"[RPE Ambient Event]: Running DrugDeal interaction.");

            if(!dealer.Ped || !buyer.Ped)
            {
                Game.LogTrivial($"[RPE Ambient Event]: Dealer or buyer is null.  Ending event.");
                @event.Cleanup();
                return;
            }

            // Dealer faces buyer, plays speech, waves
            Rage.Native.NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(dealer.Ped, buyer.Ped, -1);
            PlayDealerCueSpeech();
            dealer.Ped.Tasks.PlayAnimation("friends@frj@ig_1", "wave_c", 1, AnimationFlags.None);

            //Make buyer walk to dealer
            buyer.Ped.Tasks.GoToOffsetFromEntity(dealer.Ped, 1.0f, 0, 2.0f).WaitForCompletion();
            Rage.Native.NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(buyer.Ped, dealer.Ped, -1);

            PlayInteractionAudioAndAnimation();
            EndEvent(@event);
            
            void PlayDealerCueSpeech()
            {
                if (dealer.Ped.RelationshipGroup.Name == "AMBIENT_GANG_BALLAS" || dealer.Ped.RelationshipGroup.Name == "AMBIENT_GANG_FAMILY")
                {
                    dealer.Ped.PlayAmbientSpeech("A_M_M_SOUCENT_01_BLACK_FULL_01", "GREET_ACROSS_STREET", 0, SpeechModifier.ForceShouted);
                }
                else
                {
                    dealer.Ped.PlayAmbientSpeech("A_M_M_EASTSA_02_LATINO_FULL_01", "GREET_ACROSS_STREET", 0, SpeechModifier.ForceShouted);
                }
            }

            void PlayInteractionAudioAndAnimation()
            {
                if (buyer.Ped.RelationshipGroup == RelationshipGroup.AmbientGangMexican)
                {
                    buyer.Ped.PlayAmbientSpeech("A_M_Y_MEXTHUG_01_LATINO_FULL_01", "GENERIC_BUY", 0, SpeechModifier.Force);
                }
                else
                {
                    buyer.Ped.PlayAmbientSpeech("A_M_M_SOUCENT_01_BLACK_FULL_01", "GENERIC_BUY", 0, SpeechModifier.Force);
                }
                buyer.Ped.Tasks.PlayAnimation("amb@world_human_bum_standing@twitchy@idle_a", "idle_a", 1, AnimationFlags.Loop);
                dealer.Ped.Tasks.PlayAnimation("amb@world_human_drug_dealer_hard@male@idle_b", "idle_d", 1, AnimationFlags.Loop);

                //Blip BuyerBlip = buyer.Ped.AttachBlip();
                //BuyerBlip.Scale = 0.75f;
                //BuyerBlip.Color = Color.White;
            }
        }

        private static void EndEvent(AmbientEvent @event)
        {
            var dealer = @event.EventPeds.FirstOrDefault(x => x.Role == Role.PrimarySuspect);
            var buyer = @event.EventPeds.FirstOrDefault(x => x.Role == Role.SecondarySuspect);
            var pedsWandering = false;
            var oldDistance = Game.LocalPlayer.Character.DistanceTo2D(dealer.Ped);

            Game.LogTrivial($"[RPE Ambient Event]: In the wander/pursuit loop");
            while (true)
            {
                if (!dealer.Ped || !buyer.Ped)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Dealer or buyer is null.  Ending event.");
                    @event.Cleanup();
                    return;
                }

                if (Game.LocalPlayer.Character.DistanceTo2D(dealer.Ped) > 150f)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player is too far away.  Ending event.");
                    @event.Cleanup();
                    return;
                }

                if (Game.LocalPlayer.Character.DistanceTo2D(dealer.Ped.Position) < 10f && !pedsWandering)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player is triggering ped response.  Resetting event.");
                    if (new Random().Next(10) % 2 == 0)
                    {
                        MakePedsWander();
                        pedsWandering = true;
                    }
                    else
                    {
                        StartPursuit();
                        break;
                    }
                }

                if(Settings.EventBlips && dealer.Blip)
                {
                    if (Math.Abs(Game.LocalPlayer.Character.DistanceTo2D(dealer.Ped) - oldDistance) > 0.15 && Game.LocalPlayer.Character.DistanceTo2D(dealer.Ped) > oldDistance && dealer.Blip.Alpha > 0f)
                    {
                        dealer.Blip.Alpha -= 0.001f;
                    }
                    else if (Math.Abs(Game.LocalPlayer.Character.DistanceTo2D(dealer.Ped) - oldDistance) > 0.15 && Game.LocalPlayer.Character.DistanceTo2D(dealer.Ped) < oldDistance && dealer.Blip.Alpha < 1.0f)
                    {
                        dealer.Blip.Alpha += 0.01f;
                    }
                    oldDistance = Game.LocalPlayer.Character.DistanceTo2D(dealer.Ped);
                }
                GameFiber.Yield();
            }

            void MakePedsWander()
            {
                Game.LogTrivial($"[RPE Ambient Event]: Peds are wandering");
                dealer.Ped.Tasks.Wander();
                if (buyer.Ped.IsInAnyVehicle(false))
                {
                    buyer.Ped.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.DriveAroundVehicles);
                }
                else
                {
                    buyer.Ped.Tasks.Wander();
                }

                // Fade blip
                foreach (Blip blip in @event.EventBlips.Where(b => b))
                {
                    while (blip && blip.Alpha > 0)
                    {
                        blip.Alpha -= 0.01f;
                        GameFiber.Yield();
                    }
                    if (blip)
                    {
                        blip.Delete();
                    }
                }
                @event.Cleanup();
            }

            void StartPursuit()
            {
                foreach (Blip blip in @event.EventBlips)
                {
                    blip.Delete();
                }

                var pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(pursuit, buyer.Ped);
                Functions.AddPedToPursuit(pursuit, dealer.Ped);
                Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                Game.LogTrivial($"[RPE Ambient Event]: Pursuit initiated successfully");
                @event.Cleanup();
            }
        }
    }
}
