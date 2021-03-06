﻿using Azure.HabboHotel.GameClients;
using System.Collections.Generic;

namespace Azure.HabboHotel.Support
{
    /// <summary>
    /// Class HelperSession.
    /// </summary>
    internal class HelperSession
    {
        /// <summary>
        /// The helper
        /// </summary>
        internal GameClient Helper;

        /// <summary>
        /// The requester
        /// </summary>
        internal GameClient Requester;

        /// <summary>
        /// The chats
        /// </summary>
        internal List<string> Chats;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelperSession"/> class.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="requester">The requester.</param>
        /// <param name="question">The question.</param>
        internal HelperSession(GameClient helper, GameClient requester, string question)
        {
            this.Helper = helper;
            this.Requester = requester;
            this.Chats = new List<string> { question };
            this.Response(requester, question);
        }

        /// <summary>
        /// Responses the specified response client.
        /// </summary>
        /// <param name="responseClient">The response client.</param>
        /// <param name="response">The response.</param>
        internal void Response(GameClient responseClient, string response)
        {
        }
    }
}