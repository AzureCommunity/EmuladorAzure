using System.Collections.Generic;
using System.Linq;
using Azure.HabboHotel.Items;

namespace Azure.HabboHotel.Rooms.Wired.Handlers.Triggers
{
    internal class GameEnds : IWiredItem
    {
        public GameEnds(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
        }

        public Interaction Type
        {
            get { return Interaction.TriggerGameEnd; }
        }

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items
        {
            get { return new List<RoomItem>(); }
            set { }
        }

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public string OtherString
        {
            get { return ""; }
            set { }
        }

        public string OtherExtraString
        {
            get { return ""; }
            set { }
        }

        public string OtherExtraString2
        {
            get { return ""; }
            set { }
        }

        public bool OtherBool
        {
            get { return true; }
            set { }
        }

        public bool Execute(params object[] stuff)
        {
            var conditions = Room.GetWiredHandler().GetConditions(this);
            var effects = Room.GetWiredHandler().GetEffects(this);
            if (conditions.Any())
            {
                foreach (var current in conditions)
                {
                    if (!current.Execute(null, Type)) return false;
                    WiredHandler.OnEvent(current);
                }
            }
            if (effects.Any())
            {
                foreach (var current2 in effects)
                {
                    current2.Execute(null, Type);
                    WiredHandler.OnEvent(current2);
                }
            }
            WiredHandler.OnEvent(this);
            return true;
        }
    }
}