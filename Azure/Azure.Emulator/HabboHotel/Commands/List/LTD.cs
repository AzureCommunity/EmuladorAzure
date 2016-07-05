using Azure.Configuration;
using Azure.HabboHotel.GameClients;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class LTD. This class cannot be inherited.
    /// </summary>
    internal sealed class LTD : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LTD"/> class.
        /// </summary>
        public LTD()
        {
            MinRank = 9;
            Description = "Refresca todo item, seccion. para un new rare";
            Usage = ":ltd";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            using (var adapter = Azure.GetDatabaseManager().GetQueryReactor())
            {
                FurniDataParser.SetCache();
                Azure.GetGame().GetItemManager().LoadItems(adapter);
                Azure.GetGame().GetCatalog().Initialize(adapter);
                Azure.GetGame().Reloaditems();
                FurniDataParser.Clear();
            }
            Azure.GetGame()
                .GetClientManager()
                .QueueBroadcaseMessage(
                    new ServerMessage(LibraryParser.OutgoingRequest("PublishShopMessageComposer")));
            var message = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
            message.AppendString("ninja_promo_LTD");
            message.AppendInteger(4);
            message.AppendString("title");
            message.AppendString("Nuevo Rare Limitado");
            message.AppendString("message");
            message.AppendString("<i><h1>¿Que es?</h1>, ¡un Nuevo rare limitado! echale un vistazo en la tienda<br>Descubre que rare es y compralo aprovecha ante que se acabe.</br>");
            message.AppendString("linkUrl");
            message.AppendString("event:catalog/open/ultd_furni");
            message.AppendString("linkTitle");
            message.AppendString("Ver el Furni");

            Azure.GetGame().GetClientManager().QueueBroadcaseMessage(message);
            return true;
        }
    }
}