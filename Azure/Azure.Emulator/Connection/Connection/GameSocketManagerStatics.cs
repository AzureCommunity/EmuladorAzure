namespace Azure.Connection.Connection
{
    /// <summary>
    /// Class GameSocketManagerStatics.
    /// </summary>
    public class GameSocketManagerStatics
    {
        /// <summary>
        /// The buffer size
        /// </summary>
        public static readonly int BufferSize = 2048;

        /// <summary>
        /// The maximum packet size
        /// </summary>
        public static readonly int MaxPacketSize = (BufferSize - 4);
    }
}