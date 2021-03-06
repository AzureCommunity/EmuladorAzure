﻿using Azure.HabboHotel.Items;
using System.Collections.Generic;

namespace Azure.HabboHotel.Rooms.Wired.Handlers.Effects
{
    public class CallStacks : IWiredItem
    {
        //private List<InteractionType> mBanned;
        public CallStacks(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
            //this.mBanned = new List<InteractionType>();
        }

        public Interaction Type
        {
            get
            {
                return Interaction.ActionCallStacks;
            }
        }

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

        public int Delay { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            RoomUser roomUser = (RoomUser)stuff[0];
            List<WiredItem> Effects = new List<WiredItem>();
            foreach (RoomItem item in Items)
            {
                if (!item.IsWired) continue;
                var wired = Room.GetWiredHandler().GetWired(item);
                if (wired == null) continue;
                var effects = Room.GetWiredHandler().GetEffects(wired);
                WiredHandler.OnEvent(wired);
                wired.Execute(roomUser, Type);
            }
            return true;
        }
    }
}