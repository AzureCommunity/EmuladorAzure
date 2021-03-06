﻿using Azure.HabboHotel.GameClients;
using System;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class SetVideos. This class cannot be inherited.
    /// </summary>
    internal sealed class SetVideos : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetVideos"/> class.
        /// </summary>
        public SetVideos()
        {
            MinRank = -1;
            Description = "Update your Youtube Videos.";
            Usage = ":setvideos";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            session.GetHabbo().GetYoutubeManager().RefreshVideos();
            return true;
        }
    }
}