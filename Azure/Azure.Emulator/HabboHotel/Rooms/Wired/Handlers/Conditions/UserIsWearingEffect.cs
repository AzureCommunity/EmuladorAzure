﻿using Azure.HabboHotel.Items;
using System.Collections.Generic;

namespace Azure.HabboHotel.Rooms.Wired.Handlers.Conditions
{
    internal class UserIsWearingEffect : IWiredItem
    {
        public UserIsWearingEffect(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
            OtherString = "0";
        }

        public Interaction Type
        {
            get { return Interaction.ConditionUserWearingEffect; }
        }

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

        public string OtherString { get; set; }

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

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public bool Execute(params object[] stuff)
        {
            if (stuff == null || !(stuff[0] is RoomUser))
                return false;
            var roomUser = (RoomUser)stuff[0];

            int effect;
            if (!int.TryParse(OtherString, out effect))
                return false;

            if (roomUser.IsBot || roomUser.GetClient() == null)
                return false;

            return roomUser.CurrentEffect == effect;
        }
    }
}