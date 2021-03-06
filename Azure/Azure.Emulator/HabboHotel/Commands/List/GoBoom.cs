﻿using Azure.HabboHotel.GameClients;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class GoBoom. This class cannot be inherited.
    /// </summary>
    internal sealed class GoBoom : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GoBoom"/> class.
        /// </summary>
        public GoBoom()
        {
            MinRank = 5;
            Description = "BOOMMMMM";
            Usage = ":goboom";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = Azure.GetGame().GetRoomManager().GetRoom(session.GetHabbo().CurrentRoomId);
            room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            foreach (var user in room.GetRoomUserManager().GetRoomUsers()) user.ApplyEffect(108);
            return true;
        }
    }
}