﻿using Azure.HabboHotel.Items;
using System;
using System.Collections.Generic;

namespace Azure.HabboHotel.Rooms.Wired.Handlers.Effects
{
    public class ResetTimers : IWiredItem
    {
        //private List<InteractionType> mBanned;
        public ResetTimers(RoomItem item, Room room)
        {
            this.Item = item;
            Room = room;
            this.OtherString = string.Empty;
            this.OtherExtraString = string.Empty;
            this.OtherExtraString2 = string.Empty;
            //this.mBanned = new List<InteractionType>();
        }

        public Interaction Type
        {
            get
            {
                return Interaction.ActionResetTimer;
            }
        }

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items
        {
            get
            {
                return new List<RoomItem>();
            }
            set
            {
            }
        }

        public int Delay { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            //var roomUser = (RoomUser)stuff[0];
            //var item = (InteractionType)stuff[1];
            Room.LastTimerReset = DateTime.Now;
            return true;
        }
    }
}