using Rage;
using System.Drawing;

namespace RichsPoliceEnhancements
{
    enum Role
    {
        PrimarySuspect = 0,
        SecondarySuspect = 1,
        Victim = 1,
    }

    internal class EventPed
    {
        internal AmbientEvent Event { get; private set; }

        internal Ped Ped { get; private set; }

        internal Role Role { get; private set; }

        internal Blip Blip { get; private set; }

        internal EventPed(AmbientEvent @event, Ped ped, Role role, bool giveBlip, BlipSprite sprite = BlipSprite.StrangersandFreaks)
        {
            Event = @event;
            Ped = ped;
            SetPersistence();
            Role = role;
            if (Settings.EventBlips && giveBlip)
            {
                CreateBlip(sprite);
            }
            Event.EventPeds.Add(this);
        }

        private void SetPersistence()
        {
            Ped.IsPersistent = true;
            Ped.BlockPermanentEvents = true;
        }

        private void CreateBlip(BlipSprite sprite)
        {
            Blip = Ped.AttachBlip();
            Blip.Sprite = sprite;
            if(Role == Role.PrimarySuspect)
            {
                Blip.Color = Color.Red;
                if(Event.EventType == EventType.DriveBy)
                {
                    Blip.Alpha = 0;
                }
            }
            if(Role == Role.Victim)
            {
                Blip.Color = Color.White;
            }
            Blip.Scale = 0.75f;
            Event.EventBlips.Add(Blip);
        }
    }
}
