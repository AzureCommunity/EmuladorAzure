using System;
using System.Text;
using Azure.HabboHotel.GameClients;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class About. This class cannot be inherited.
    /// </summary>
    internal sealed class About : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="About"/> class.
        /// </summary>
        public About()
        {
            MinRank = 1;
            Description = "Shows information about the server.";
            Usage = ":about";
            MinParams = 0;
        }

        public override bool Execute(GameClient client, string[] pms)
        {
            var message =
                new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));

            message.AppendString("Azure");
            message.AppendInteger(4);
            message.AppendString("title");
            // Respect Azure Emulator and don't remove the developers credits!
            message.AppendString("¡GamerLive.cL Fixed!");
            // Respect Azure Emulator and don't remove the developers credits!
            message.AppendString("message");
            var info = new StringBuilder();
            // Respect Azure Emulator and don't remove the developers credits!
            info.Append("<h5><b>Emulador editado por Fanco Sanllehi [Privado]</b><h5></br></br>");
            // Respect Azure Emulator and don't remove the developers credits!
            info.Append("<br />");
            info.Append("<br />");
            // Respect Azure Emulator and don't remove the developers credits!
            info.AppendFormat("<b>Creditos</b> <br />-Editores Azure Emulator/>-Sulake /> <br /> ");
            // Respect Azure Emulator and don't remove the fixers credits!
            info.AppendFormat("<b>[Estadisticas del hotel]</b> <br />");
            var userCount = Azure.GetGame().GetClientManager().Clients.Count;
            // Respect Azure Emulator and don't remove the developers credits!
            // Respect Azure Emulator and don't remove the developers credits!
            var roomsCount = Azure.GetGame().GetRoomManager().LoadedRooms.Count;
            info.AppendFormat("<b>Usuarios:</b> {0} en {1}{2}.<br /><br /><br />", userCount, roomsCount,
                (roomsCount == 1) ? " Sala" : " Salas");
            message.AppendString(info.ToString());
            message.AppendString("linkUrl");
            message.AppendString("event:");
            message.AppendString("linkTitle");
            message.AppendString("ok");
            client.SendMessage(message);

            return true;
        }
    }
}