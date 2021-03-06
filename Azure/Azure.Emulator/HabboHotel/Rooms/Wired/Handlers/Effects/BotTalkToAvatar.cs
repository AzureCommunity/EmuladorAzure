﻿using Azure.HabboHotel.Items;
using Azure.Messages;
using Azure.Messages.Parsers;
using System.Collections.Generic;

namespace Azure.HabboHotel.Rooms.Wired.Handlers.Effects
{
    public class BotTalkToAvatar : IWiredItem
    {
        //private List<InteractionType> mBanned;
        public BotTalkToAvatar(RoomItem item, Room room)
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
                return Interaction.ActionBotTalkToAvatar;
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

        public int Delay
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            RoomUser roomUser = (RoomUser)stuff[0];
            //InteractionType item = (InteractionType)stuff[1];
            RoomUser bot = Room.GetRoomUserManager().GetBotByName(OtherString);
            if (bot == null) return false;
            if (OtherBool) // Whisper
            {
                var whisp = new ServerMessage(LibraryParser.OutgoingRequest("WhisperMessageComposer"));
                whisp.AppendInteger(bot.VirtualId);
                whisp.AppendString(OtherExtraString);
                whisp.AppendInteger(0);
                whisp.AppendInteger(2);
                whisp.AppendInteger(0);
                whisp.AppendInteger(-1);
                roomUser.GetClient().SendMessage(whisp);
            }
            else
            {
                bot.Chat(null, roomUser.GetUserName() + " : " + OtherExtraString, false, 0);
            }
            return true;
        }
    }
}