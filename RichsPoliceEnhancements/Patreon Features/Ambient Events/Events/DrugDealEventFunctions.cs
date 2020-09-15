using System.Collections.Generic;
using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.API;
using System.Linq;

namespace RichsPoliceEnhancements
{
    class DrugDealEventFunctions
    {
        public static void RunEventFunctions(Ped player, List<Ped> nearbyPeds, List<EventPed> eventPeds, List<EventVehicle> eventVehicles, List<Blip> eventBlips)
        {
            Game.LogTrivial($"[Rich Ambiance] Starting DrugDeal event.");
            if (FindDealer(nearbyPeds, eventPeds, eventBlips) != null && FindBuyer(nearbyPeds, eventPeds) != null)
            {
                DrugFootInteraction(eventPeds);
            }
            else
            {
                Game.LogTrivial($"[Rich Ambiance] Dealer or buyer returned null.  Ending event.");
            }
        }

        private static Ped FindDealer(List<Ped> pedList, List<EventPed> eventPeds, List<Blip> eventBlips)
        {
            foreach (Ped p in pedList.Where(p => p.IsOnFoot && p.RelationshipGroup == RelationshipGroup.AmbientGangBallas || p.RelationshipGroup == RelationshipGroup.AmbientGangFamily || p.RelationshipGroup == RelationshipGroup.AmbientGangMexican))
            {
                // Ped Settings
                Ped dealer = p;
                dealer.IsPersistent = true;
                dealer.BlockPermanentEvents = true;

                // Blip Settings
                if (Settings.EventBlips)
                {
                    Blip dealerBlip = dealer.AttachBlip();
                    dealerBlip.Sprite = BlipSprite.StrangersandFreaks;
                    dealerBlip.Color = Color.Red;
                    dealerBlip.Scale = 0.75f;
                    eventBlips.Add(dealerBlip);
                }

                eventPeds.Add(new EventPed("DrugDeal", dealer));
                Game.LogTrivial($"[Rich Ambiance] Dealer found.");
                return dealer;
            }

            Game.LogTrivial($"[Rich Ambiance] No suitable dealer peds found.");
            return null;
        }

        private static Ped FindBuyer(List<Ped> pedList, List<EventPed> eventPeds)
        {
            Ped dealer = eventPeds[0].Ped;
            Ped buyer;
            List<Ped> sortedList = pedList.OrderBy(p => p.DistanceTo(dealer)).ToList();

            foreach (Ped p in sortedList)
            {
                //Game.LogTrivial($"Ped distance to dealer: {p.DistanceTo(dealer)}");
                //if (p.Exists() && p.IsValid() && dealer.Exists() && dealer.IsValid() && )
                if (p.DistanceTo(dealer) > 0)
                {
                    buyer = p;
                    buyer.IsPersistent = true;
                    buyer.BlockPermanentEvents = true;
                    eventPeds.Add(new EventPed("DrugDeal", buyer));
                    Game.LogTrivial($"[Rich Ambiance] Buyer found.");
                    return buyer;
                }
            }

            /*if (dealer.GetAttachedBlips() != null)
            {
                foreach (Blip blip in dealer.GetAttachedBlips())
                {
                    blip.Delete();
                }
            }*/
            dealer.IsPersistent = false;
            dealer.Tasks.Clear();
            dealer.Dismiss();

            Game.LogTrivial($"[Rich Ambiance] No buyers found close enough to dealer.");
            return null;
        }

        private static void DrugFootInteraction(List<EventPed> eventPeds)
        {
            Ped dealer = eventPeds[0].Ped;
            Ped buyer = eventPeds[1].Ped;
            bool pedsInteracting = false, pedsWandering = false;
            LHandle pursuit = null;
            Game.LogTrivial($"[Rich Ambiance] Running DrugDeal interaction.");
            
            //Play a sketchy animation on dealer
            if (dealer.RelationshipGroup.Name == "AMBIENT_GANG_BALLAS" || dealer.RelationshipGroup.Name == "AMBIENT_GANG_FAMILY")
            {
                dealer.PlayAmbientSpeech("A_M_M_SOUCENT_01_BLACK_FULL_01", "GREET_ACROSS_STREET", 0, SpeechModifier.ForceShouted);
            }
            else
            {
                dealer.PlayAmbientSpeech("A_M_M_EASTSA_02_LATINO_FULL_01", "GREET_ACROSS_STREET", 0, SpeechModifier.ForceShouted);
            }

            // Make dealer look at buyer and wave
            dealer.Face(buyer);
            dealer.Tasks.PlayAnimation("friends@frj@ig_1", "wave_c", 1, AnimationFlags.None);

            //Make buyer walk to dealer
            buyer.Tasks.GoToOffsetFromEntity(dealer, 1.0f, 0, 2.0f);

            // Loop before the peds are interacting 
            while (!pedsInteracting && !AmbientEvent.PrematureEndCheck(eventPeds))
            {
                // If buyer is close to dealer, make him interact with the dealer
                if (buyer.DistanceTo(dealer.Position) <= 1.5f && !pedsInteracting)
                {
                    Game.LogTrivial($"[Rich Ambiance] Buyer is interacting with dealer");
                    buyer.Face(dealer);
                    if(buyer.RelationshipGroup == RelationshipGroup.AmbientGangMexican)
                    {
                        buyer.PlayAmbientSpeech("A_M_Y_MEXTHUG_01_LATINO_FULL_01", "GENERIC_BUY", 0, SpeechModifier.Force);
                    }
                    else
                    {
                        buyer.PlayAmbientSpeech("A_M_M_SOUCENT_01_BLACK_FULL_01", "GENERIC_BUY", 0, SpeechModifier.Force);
                    }
                    buyer.Tasks.PlayAnimation("amb@world_human_bum_standing@twitchy@idle_a", "idle_a", 1, AnimationFlags.Loop);

                    //Blip BuyerBlip = buyer.Ped.AttachBlip();
                    //BuyerBlip.Scale = 0.75f;
                    //BuyerBlip.Color = Color.White;

                    dealer.Face(buyer);
                    dealer.Tasks.PlayAnimation("amb@world_human_drug_dealer_hard@male@idle_b", "idle_d", 1, AnimationFlags.Loop);
                    pedsInteracting = true;
                }
                GameFiber.Yield();
            }

            // Looping after peds are interacting but before the player gets too close
            //while(!EndEventEarly(dealer, buyer))
            while (!AmbientEvent.PrematureEndCheck(eventPeds))
            {
                //Game.LogTrivial($"[Rich Ambiance] In the wander/pursuit loop"); 

                // If player is close to dealer and there is no pursuit, make the suspects wander or initiate a pursuit
                if (Game.LocalPlayer.Character.DistanceTo(dealer.Position) < 10f && !pedsWandering)
                {
                    if (EventSelect.RandomNumber(2) == 0)
                    {
                        Game.LogTrivial($"[Rich Ambiance] Peds are wandering");
                        //Game.DisplayHelp("Player within distance");
                        dealer.Tasks.Wander();
                        buyer.Tasks.Wander();
                        /*if (buyer.Ped.IsInAnyVehicle(false))
                        {
                            buyer.Ped.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.DriveAroundVehicles); } else { buyer.Ped.Tasks.Wander();
                        }*/
                        pedsWandering = true;
                        //break;
                    }
                    else
                    {
                        if (dealer.GetAttachedBlips() != null)
                        {
                            foreach (Blip blip in dealer.GetAttachedBlips())
                            {
                                blip.Delete();
                            }
                        }
                        if (buyer.GetAttachedBlips() != null)
                        {
                            foreach (Blip blip in buyer.GetAttachedBlips())
                            {
                                blip.Delete();
                            }
                        }

                        pursuit = Functions.CreatePursuit();
                        Functions.AddPedToPursuit(pursuit, buyer);
                        Functions.AddPedToPursuit(pursuit, dealer);
                        Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                        Game.LogTrivial($"[Rich Ambiance] Pursuit initiated successfully");
                        break;
                    }
                }
                GameFiber.Yield();
            }
        }
    }
}
