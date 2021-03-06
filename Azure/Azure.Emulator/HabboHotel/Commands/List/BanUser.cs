﻿using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Support;
using System.Linq;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class BanUser. This class cannot be inherited.
    /// </summary>
    internal sealed class BanUser : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BanUser"/> class.
        /// </summary>
        public BanUser()
        {
            MinRank = 4;
            Description = "Ban a user!";
            Usage = ":ban [USERNAME] [TIME] [REASON]";
            MinParams = -2;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var user = Azure.GetGame().GetClientManager().GetClientByUserName(pms[0]);

            if (user == null)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (user.GetHabbo().Rank >= session.GetHabbo().Rank)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("user_is_higher_rank"));
                return true;
            }
            var length = int.Parse(pms[1]);

            var message = pms.Length < 3 ? string.Empty : string.Join(" ", pms.Skip(2));
            if (string.IsNullOrWhiteSpace(message)) message = Azure.GetLanguage().GetVar("command_ban_user_no_reason");

            ModerationTool.BanUser(session, user.GetHabbo().Id, length, message);
            Azure.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, user.GetHabbo().UserName, "Ban",
                    string.Format("User has been banned [{0}]", pms[1]));
            return true;
        }
    }
}