namespace Azure.HabboHotel.Rooms
{
    /// <summary>
    /// Struct InvokedChatMessage
    /// </summary>
    internal struct InvokedChatMessage
    {
        /// <summary>
        /// The user
        /// </summary>
        internal RoomUser User;

        /// <summary>
        /// The message
        /// </summary>
        internal string Message;

        /// <summary>
        /// The shout
        /// </summary>
        internal bool Shout;

        /// <summary>
        /// The colour type
        /// </summary>
        internal int ColourType;

        /// <summary>
        /// The count
        /// </summary>
        internal int Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokedChatMessage"/> struct.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="message">The message.</param>
        /// <param name="shout">if set to <c>true</c> [shout].</param>
        /// <param name="colour">The colour.</param>
        /// <param name="count">The count.</param>
        public InvokedChatMessage(RoomUser user, string message, bool shout, int colour, int count)
        {
            this.User = user;
            this.Message = message;
            this.Shout = shout;
            this.ColourType = colour;
            this.Count = count;
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        internal void Dispose()
        {
            this.User = null;
            this.Message = null;
        }
    }
}