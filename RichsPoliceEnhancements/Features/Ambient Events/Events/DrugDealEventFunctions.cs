using System.Collections.Generic;
using System.Drawing;
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
            Game.LogTrivial($"[RPE Ambient Event]: Starting DrugDeal event.");

            var dealerPed = FindDealer();
            if(dealerPed == null)
            {
                Game.LogTrivial($"[RPE Ambient Event]: Dealer ped is null, ending event.");
                @event.Cleanup();
                return;
            }

            var buyerPed = FindBuyer(dealerPed);
            if(buyerPed == null)
            {
                Game.LogTrivial($"[RPE Ambient Event]: Buyer ped is null, ending event.");
                @event.Cleanup();
                return;
            }

            GameFiber DrugDealInteractionFiber = new GameFiber(() => DrugDealFootInteraction(@event), "RPE Drug Deal Interaction Fiber");
            DrugDealInteractionFiber.Start();

            List<Ped> NearbyPeds() => World.GetAllPeds().Where(p => PedIsRelevant(p)).ToList();

            bool PedIsRelevant(Ped ped)
            {
                if (ped && ped.IsAlive && ped.Position.DistanceTo(Game.LocalPlayer.Character.Position) < 100f && !ped.IsPlayer && !ped.IsInjured && !ped.Model.Name.Contains("A_C"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            EventPed FindDealer()
            {
                var dealer = NearbyPeds().Where(p => p.IsOnFoot && p.RelationshipGroup == RelationshipGroup.AmbientGangBallas || p.RelationshipGroup == RelationshipGroup.AmbientGangFamily || p.RelationshipGroup == RelationshipGroup.AmbientGangMexican).FirstOrDefault();
                if (!dealer)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Dealer returned null.");
                    return null;
                }
                else
                {
                    return new EventPed(@event, dealer, Role.PrimarySuspect);
                }
            }

            EventPed FindBuyer(EventPed dealer)
            {
                var buyer = NearbyPeds().OrderBy(p => p.DistanceTo2D(dealer.Ped)).Where(p => p.DistanceTo2D(dealer.Ped) > 0 && Math.Abs(p.Position.Z - dealer.Ped.Position.Z) < 3).FirstOrDefault();
                if (!buyer)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Buyer returned null.");
                    return null;
                }
                else
                {
                    return new EventPed(@event, buyer, Role.SecondarySuspect);
                }
            }
        }

        private static void DrugDealFootInteraction(AmbientEvent @event)
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
            PlayDealerQueueSpeech();
            dealer.Ped.Tasks.PlayAnimation("friends@frj@ig_1", "wave_c", 1, AnimationFlags.None);

            //Make buyer walk to dealer
            buyer.Ped.Tasks.GoToOffsetFromEntity(dealer.Ped, 1.0f, 0, 2.0f).WaitForCompletion();
            Rage.Native.NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(buyer.Ped, dealer.Ped, -1);

            PlayInteractionAudioAndAnimation();

            Game.LogTrivial($"[RPE Ambient Event]: In the wander/pursuit loop");
            while (true)
            {
                if(!dealer.Ped || !buyer.Ped)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Dealer or buyer is null.  Ending event.");
                    @event.Cleanup();
                    return;
                }

                if(Game.LocalPlayer.Character.DistanceTo2D(dealer.Ped) > 150f)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player is too far away.  Ending event.");
                    @event.Cleanup();
                    return;
                }

                if (Game.LocalPlayer.Character.DistanceTo2D(dealer.Ped.Position) < 10f)
                {
                    Game.LogTrivial($"[RPE Ambient Event]: Player is triggering ped response.  Resetting event.");
                    if (new Random().Next(10) % 2 == 0)
                    {
                        MakePedsWander();
                        break;
                    }
                    else
                    {
                        StartPursuit();
                        break;
                    }
            }
                GameFiber.Yield();
            }

            void PlayDealerQueueSpeech()
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
                
                foreach(Blip blip in @event.EventBlips.Where(b => b))
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
                foreach(Blip blip in @event.EventBlips)
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
