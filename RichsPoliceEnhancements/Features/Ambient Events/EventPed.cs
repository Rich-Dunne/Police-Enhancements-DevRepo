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

        internal EventPed(AmbientEvent @event, Ped ped, Role role)
        {
            Event = @event;
            Ped = ped;
            SetPersistence();
            Role = role;
            if (Settings.EventBlips && Role == Role.PrimarySuspect)
            {
                CreateBlip();
            }
            Event.EventPeds.Add(this);
            Game.LogTrivial($"Ped location: {Ped.Position}");
        }

        private void SetPersistence()
        {
            Ped.IsPersistent = true;
            Ped.BlockPermanentEvents = true;
        }

        private void CreateBlip()
        {
            Blip = Ped.AttachBlip();
            Blip.Sprite = BlipSprite.StrangersandFreaks;
            if(Role == Role.PrimarySuspect)
            {
                Blip.Color = Color.Red;
            }
            Blip.Scale = 0.75f;
            Event.EventBlips.Add(Blip);
        }
    }
}
