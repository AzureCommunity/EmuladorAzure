using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Rooms;
using Azure.Messages;
using System;
using System.Linq;

namespace Azure.HabboHotel.Users.Messenger
{
    /// <summary>
    /// Class MessengerBuddy.
    /// </summary>
    internal class MessengerBuddy
    {
        /// <summary>
        /// The user name
        /// </summary>
        internal string UserName;

        /// <summary>
        /// The client
        /// </summary>
        internal GameClient Client;

        /// <summary>
        /// The _look
        /// </summary>
        private string _look;

        /// <summary>
        /// The _motto
        /// </summary>
        private string _motto;

        //private readonly int _lastOnline;
        /// <summary>
        /// The _appear offline
        /// </summary>
        private readonly bool _appearOffline;

        /// <summary>
        /// The _hide inroom
        /// </summary>
        private readonly bool _hideInroom;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessengerBuddy"/> class.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="look">The look.</param>
        /// <param name="motto">The motto.</param>
        /// <param name="lastOnline">The last online.</param>
        /// <param name="appearOffline">if set to <c>true</c> [appear offline].</param>
        /// <param name="hideInroom">if set to <c>true</c> [hide inroom].</param>
        internal MessengerBuddy(uint userId, string userName, string look, string motto, int lastOnline,
            bool appearOffline, bool hideInroom)
        {
            Id = userId;
            UserName = userName;
            _look = look;
            _motto = motto;
            //_lastOnline = lastOnline;
            _appearOffline = appearOffline;
            _hideInroom = hideInroom;
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        internal uint Id { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is online.
        /// </summary>
        /// <value><c>true</c> if this instance is online; otherwise, <c>false</c>.</value>
        internal bool IsOnline
        {
            get
            {
                return Client != null && Client.GetHabbo() != null && Client.GetHabbo().GetMessenger() != null &&
                       !Client.GetHabbo().GetMessenger().AppearOffline;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [in room].
        /// </summary>
        /// <value><c>true</c> if [in room]; otherwise, <c>false</c>.</value>
        internal bool InRoom
        {
            get { return CurrentRoom != null; }
        }

        /// <summary>
        /// Gets or sets the current room.
        /// </summary>
        /// <value>The current room.</value>
        internal Room CurrentRoom { get; set; }

        /// <summary>
        /// Updates the user.
        /// </summary>
        internal void UpdateUser()
        {
            Client = Azure.GetGame().GetClientManager().GetClientByUserId(Id);
            if (Client == null || Client.GetHabbo() == null)
                return;
            CurrentRoom = Client.GetHabbo().CurrentRoom;
            _look = Client.GetHabbo().Look;
            _motto = Client.GetHabbo().Motto;
        }

        /// <summary>
        /// Serializes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="session">The session.</param>
        internal void Serialize(ServerMessage message, GameClient session)
        {
            var value =
                session.GetHabbo().Relationships.FirstOrDefault(x => x.Value.UserId == Convert.ToInt32(Id)).Value;
            var i = (value == null) ? 0 : value.Type;
            message.AppendInteger(Id);
            message.AppendString(UserName);
            message.AppendInteger(IsOnline || Id == 0);

            message.AppendBool(Id == 0 || (!_appearOffline || session.GetHabbo().Rank >= 4) && IsOnline);
            message.AppendBool(Id != 0 && (!_hideInroom || session.GetHabbo().Rank >= 4) && InRoom);

            message.AppendString(IsOnline || Id == 0 ? _look : string.Empty);
            message.AppendInteger(0);
            message.AppendString(_motto);
            message.AppendString(string.Empty);
            message.AppendString(string.Empty);
            message.AppendBool(true); // persistedMessageUser
            message.AppendBool(false);
            message.AppendBool(false); // pocketHabboUser
            message.AppendShort(i);
        }
    }
}