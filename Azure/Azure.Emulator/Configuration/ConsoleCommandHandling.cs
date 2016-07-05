using Azure.HabboHotel;
using Azure.Messages;
using Azure.Messages.Parsers;
using System;
using System.Threading.Tasks;

namespace Azure.Configuration
{
    /// <summary>
    /// Class ConsoleCommandHandling.
    /// </summary>
    internal class ConsoleCommandHandling
    {
        /// <summary>
        /// The is waiting
        /// </summary>
        internal static bool IsWaiting = false;

        /// <summary>
        /// The _matrix
        /// </summary>
        private static bool _matrix = false;

        /// <summary>
        /// The _matrix task
        /// </summary>
        private static Task _matrixTask;

        /// <summary>
        /// Gets the game.
        /// </summary>
        /// <returns>Game.</returns>
        internal static Game GetGame()
        {
            return Azure.GetGame();
        }

        /// <summary>
        /// Invokes the command.
        /// </summary>
        /// <param name="inputData">The input data.</param>
        internal static void InvokeCommand(string inputData)
        {
            if (string.IsNullOrEmpty(inputData) && Logging.DisabledState) return;
            Console.WriteLine();
            try
            {
                if (inputData == null) return;
                var strArray = inputData.Split(' ');
                switch (strArray[0])
                {
                    case "shutdown":
                    case "close":
                    case "apagar": // Apagamos el Emu
                    case "desligar":
                        Logging.DisablePrimaryWriting(true);
                        Out.WriteLine("Shutdown Initalized", "Azure.Life", ConsoleColor.DarkYellow);
                        Azure.PerformShutDown(false);
                        Console.WriteLine();
                        return;

                    case "restart":
                        Logging.LogMessage(string.Format("Server Restarting at {0}", DateTime.Now));
                        Logging.DisablePrimaryWriting(true);
                        Out.WriteLine("Restart Initialized", "Azure.Life", ConsoleColor.DarkYellow);
                        Azure.PerformShutDown(true);
                        Console.WriteLine();
                        return;

                    case "GamerLive":
                        int width, height;
                        int[] y;

                        if (_matrixTask == null)
                        {
                            Console.BackgroundColor = ConsoleColor.Black;
                            Matrix.Initialize(out width, out height, out y);
                            _matrix = true;

                            _matrixTask = new Task(() =>
                            {
                                while (_matrix)
                                {
                                    Matrix.Counter = Matrix.Counter++;
                                    Matrix.UpdateAllColumns(width, height, y);
                                    if (Matrix.Counter > (3 * Matrix.Interval)) Matrix.Counter = 0;
                                }

                                Console.BackgroundColor = ConsoleColor.White;
                                Console.Clear();
                                Program.StartConsoleWindow();
                                Console.WriteLine();
                                Out.WriteLine("Party ended :(", "EasterEgg.Xdr", ConsoleColor.DarkGray);
                            });
                            _matrixTask.Start();
                        }
                        else
                        {
                            _matrix = false;
                            _matrixTask = null;
                        }
                        return;

                    case "flush":
                    case "reload":
                    case "atualizar":
                    case "recargar":
                        if (strArray.Length >= 2) break;
                        Console.WriteLine("Especifica un parametro. escribe 'help' o 'ayuda' para saber mas sobre los comandos de consola");
                        Console.WriteLine();
                        return;

                    case "alerta":
                        {
                            var str = inputData.Substring(6);
                            var message = new ServerMessage(LibraryParser.OutgoingRequest("BroadcastNotifMessageComposer"));
                            message.AppendString(str);
                            message.AppendString(string.Empty);
                            GetGame().GetClientManager().QueueBroadcaseMessage(message);
                            Console.WriteLine("[{0}] was sent!", str);
                            return;
                        }
                    case "clear":
                        Console.Clear();
                        return;

                    case "help":
                        Console.WriteLine("apagar       - Para Apagar el emulador");
                        Console.WriteLine("clear        - Para limpiar texto del emulador");
                        Console.WriteLine("alerta (msg) - Para enviar mensaje a todo el Hotel");
                        Console.WriteLine("recargar");
                        Console.WriteLine("   - catalogo");
                        Console.WriteLine("   - modeldata");
                        Console.WriteLine("   - bans");
                        Console.WriteLine("   - packets (reload packets ids)");
                        Console.WriteLine("   - Filtro");
                        Console.WriteLine();
                        return;

                    case "ayuda":
                        Console.WriteLine("apagar       - Para Apagar el emulador");
                        Console.WriteLine("clear        - Para limpiar texto del emulador");
                        Console.WriteLine("alerta (msg) - Para enviar mensaje a todo el Hotel");
                        Console.WriteLine("recargar");
                        Console.WriteLine("   - catalogo");
                        Console.WriteLine("   - modeldata");
                        Console.WriteLine("   - bans");
                        Console.WriteLine("   - packets (reload packets ids)");
                        Console.WriteLine("   - Filtro");
                        Console.WriteLine();
                        return;

                    default:
                        UnknownCommand(inputData);
                        break;
                }
                switch (strArray[1])
                {
                    case "database":
                        Azure.GetDatabaseManager().Destroy();
                        Console.WriteLine("Database destroyed");
                        Console.WriteLine();
                        break;

                    case "packets":
                        LibraryParser.ReloadDictionarys();
                        Console.WriteLine("> Packets recargados...");
                        Console.WriteLine();
                        return;

                    case "catalog":
                    case "shop":
                    case "catalogo":
                        FurniDataParser.SetCache();
                        using (var adapter = Azure.GetDatabaseManager().GetQueryReactor()) GetGame().GetCatalog().Initialize(adapter);
                        FurniDataParser.Clear();

                        GetGame()
                            .GetClientManager()
                            .QueueBroadcaseMessage(
                                new ServerMessage(LibraryParser.OutgoingRequest("PublishShopMessageComposer")));
                        Console.WriteLine("Catalogo recargado.");
                        Console.WriteLine();
                        break;

                    case "modeldata":
                        using (var adapter2 = Azure.GetDatabaseManager().GetQueryReactor()) GetGame().GetRoomManager().LoadModels(adapter2);
                        Console.WriteLine("Modelos de sala recargados.");
                        Console.WriteLine();
                        break;

                    case "bans":
                        using (var adapter3 = Azure.GetDatabaseManager().GetQueryReactor()) GetGame().GetBanManager().LoadBans(adapter3);
                        Console.WriteLine("Baneos recargados");
                        Console.WriteLine();
                        break;

                    case "filtro":
                        Security.Filter.Reload();
                        Security.BlackWordsManager.Reload();
                        Console.WriteLine("Filtro de malas palabras recargado");
                        break;

                    default:
                        UnknownCommand(inputData);
                        Console.WriteLine();
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Unknowns the command.
        /// </summary>
        /// <param name="command">The command.</param>
        private static void UnknownCommand(string command)
        {
        }
    }
}