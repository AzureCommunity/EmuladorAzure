using System;

namespace Azure.HabboHotel.Rooms
{
    /// <summary>
    /// Class UserWalksOnArgs.
    /// </summary>
    public class UserWalksOnArgs : EventArgs
    {
        /// <summary>
        /// The user
        /// </summary>
        internal readonly RoomUser User;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserWalksOnArgs"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        public UserWalksOnArgs(RoomUser user)
        {
            this.User = user;
        }
    }
}