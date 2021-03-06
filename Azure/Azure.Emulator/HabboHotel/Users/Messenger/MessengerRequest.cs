using Azure.Messages;

namespace Azure.HabboHotel.Users.Messenger
{
    /// <summary>
    /// Class MessengerRequest.
    /// </summary>
    internal class MessengerRequest
    {
        /// <summary>
        /// The _user name
        /// </summary>
        private readonly string _userName;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessengerRequest"/> class.
        /// </summary>
        /// <param name="toUser">To user.</param>
        /// <param name="fromUser">From user.</param>
        /// <param name="userName">Name of the user.</param>
        internal MessengerRequest(uint toUser, uint fromUser, string userName)
        {
            To = toUser;
            From = fromUser;
            _userName = userName;
        }

        /// <summary>
        /// Gets to.
        /// </summary>
        /// <value>To.</value>
        internal uint To { get; private set; }

        /// <summary>
        /// Gets from.
        /// </summary>
        /// <value>From.</value>
        internal uint From { get; private set; }

        /// <summary>
        /// Serializes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        internal void Serialize(ServerMessage request)
        {
            request.AppendInteger(From);
            request.AppendString(_userName);
            var habboForName = Azure.GetHabboForName(_userName);
            request.AppendString((habboForName != null) ? habboForName.Look : "");
        }
    }
}