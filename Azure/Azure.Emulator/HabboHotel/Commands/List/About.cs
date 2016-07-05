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
            Description = "Muesta informacion del servidor.";
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
            message.AppendString("Azure Emulator (V0.1)"); //Titulo
            message.AppendString("message");
            var info = new StringBuilder();
            info.Append("<h5><b>Azure Emulator Editado por DmACK [Privado]</b><h5></br></br>");
            info.Append("<br />");
            info.Append("<br />");
            /*
             * No sacar Creditos de los editores de azure emulator
             */
            info.AppendFormat("<b>Creditos:</b>");
            info.AppendFormat("-Editores Azure Emulator");
            info.AppendFormat("-Sulake");
            info.AppendFormat("<br />");
            /*
             * Nombres de desarrolladores
             */
            info.AppendFormat("<b>Developers</b>");
            info.AppendFormat("-DmACK (Franco Sanllehi)");
            info.AppendFormat("<br />");
            info.Append("<br />");
            info.Append("<br />");
            info.Append("<br />");
            /*
             * Estadisticas del Hotel
             */
            info.AppendFormat("<b>[Estadisticas del hotel]</b> <br />");
            var userCount = Azure.GetGame().GetClientManager().Clients.Count;
            var roomsCount = Azure.GetGame().GetRoomManager().LoadedRooms.Count;
            info.AppendFormat("<b>Usuarios:</b> {0} en {1}{2}.<br /><br /><br />", userCount, roomsCount,
                (roomsCount == 1) ? " Sala" : " Salas");
            message.AppendString(info.ToString());
            message.AppendString("linkUrl");
            message.AppendString("event:");
            message.AppendString("linkTitle");
            message.AppendString("Cerrar"); // Boton de cerrar
            client.SendMessage(message);

            return true;
        }
    }
}