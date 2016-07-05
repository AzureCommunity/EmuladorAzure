using Azure.HabboHotel.GameClients;
using Azure.Messages;
using Azure.Messages.Parsers;
using System.Text;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class About. This class cannot be inherited.
    /// </summary>
    internal sealed class MyInfo : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="About"/> class.
        /// </summary>
        public MyInfo()
        {
            MinRank = 1;
            Description = "Shows information personal.";
            Usage = ":myinfo";
            MinParams = 0;
        }

        public override bool Execute(GameClient client, string[] pms)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
            message.AppendString("infopersonal");
            message.AppendInteger(4);
            message.AppendString("title");
            message.AppendString("Tu información");
            message.AppendString("message");
            var builder = new StringBuilder();
            builder.AppendLine("Mi informaci\x00f3n: ");
            builder.Append(" -Username: " + client.GetHabbo().UserName + "\r");
            builder.Append(" -Cr\x00e9ditos: " + client.GetHabbo().Credits + "\r");
            builder.Append(" -Diamantes:  " + client.GetHabbo().BelCredits + "\r");
            builder.Append(" -Amigos: " + client.GetHabbo().GetMessenger().Friends.Count + "\r");
            builder.Append(" -Respetos: " + client.GetHabbo().Respect + "\r");
            builder.Append(" -Items: " + client.GetHabbo().GetInventoryComponent().TotalItems + "\r");
            builder.Append(" -Puntos: " + client.GetHabbo().AchievementPoints + "\r");
            builder.Append(" -Salas: " + client.GetHabbo().UsersRooms.Count + "\r");
            message.AppendString(builder.ToString());
            message.AppendString("linkUrl");
            message.AppendString("event:");
            message.AppendString("linkTitle");
            message.AppendString("ok");

            client.SendMessage(message);

            return true;
        }
    }
}