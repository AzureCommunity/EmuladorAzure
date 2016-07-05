﻿using Azure.HabboHotel.Items;
using Azure.HabboHotel.Users.Badges;
using System.Collections.Generic;
using System.Linq;

namespace Azure.HabboHotel.Rooms.Wired.Handlers.Conditions
{
    internal class UserIsWearingBadge : IWiredItem
    {
        public UserIsWearingBadge(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
            OtherString = string.Empty;
        }

        public Interaction Type
        {
            get { return Interaction.ConditionUserWearingBadge; }
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

            if (roomUser.IsBot || roomUser.GetClient() == null || roomUser.GetClient().GetHabbo() == null ||
                roomUser.GetClient()
                    .GetHabbo()
                    .GetBadgeComponent() == null || string.IsNullOrWhiteSpace(OtherString)) return false;

            return
                roomUser.GetClient()
                    .GetHabbo()
                    .GetBadgeComponent()
                    .BadgeList
                    .Values
                    .Cast<Badge>()
                    .Any(badge => badge.Slot > 0 && badge.Code.ToLower() == OtherString.ToLower());
        }
    }
}