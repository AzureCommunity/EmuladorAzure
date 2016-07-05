using System;
using System.Linq;
using System.Threading;
using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Rooms;

namespace Azure.HabboHotel.RoomBots
{
    /// <summary>
    /// Class GenericBot.
    /// </summary>
    internal class GenericBot : BotAI
    {
        /// <summary>
        /// The random
        /// </summary>
        private static readonly Random Random = new Random();

        /// <summary>
        /// The _id
        /// </summary>
        private readonly int _id;

        /// <summary>
        /// The _virtual identifier
        /// </summary>
        private readonly int _virtualId;

        /// <summary>
        /// The _is bartender
        /// </summary>
        private readonly bool _isBartender;

        /// <summary>
        /// The _action count
        /// </summary>
        private int _actionCount;

        /// <summary>
        /// The _speech interval
        /// </summary>
        private int _speechInterval;

        /// <summary>
        /// The _chat timer
        /// </summary>
        private Timer _chatTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericBot"/> class.
        /// </summary>
        /// <param name="roomBot">The room bot.</param>
        /// <param name="virtualId">The virtual identifier.</param>
        /// <param name="botId">The bot identifier.</param>
        /// <param name="type">The type.</param>
        /// <param name="isBartender">if set to <c>true</c> [is bartender].</param>
        /// <param name="speechInterval">The speech interval.</param>
        internal GenericBot(RoomBot roomBot, int virtualId, int botId, AIType type, bool isBartender, int speechInterval)
        {
            _id = botId;
            _virtualId = virtualId;
            _isBartender = isBartender;
            _speechInterval = speechInterval < 2 ? 2000 : speechInterval * 1000;

            // Get random speach
            if (roomBot != null && roomBot.AutomaticChat && roomBot.RandomSpeech != null && roomBot.RandomSpeech.Any()) _chatTimer = new Timer(ChatTimerTick, null, _speechInterval, _speechInterval);
            _actionCount = Random.Next(10, 30 + virtualId);
        }

        /// <summary>
        /// Modifieds this instance.
        /// </summary>
        internal override void Modified()
        {
            if (GetBotData() == null) return;
            if (!GetBotData().AutomaticChat || GetBotData().RandomSpeech == null || !GetBotData().RandomSpeech.Any())
            {
                StopTimerTick();
                return;
            }
            _speechInterval = GetBotData().SpeechInterval < 2 ? 2000 : GetBotData().SpeechInterval * 1000;

            if (_chatTimer == null)
            {
                _chatTimer = new Timer(ChatTimerTick, null, _speechInterval, _speechInterval);
                return;
            }
            _chatTimer.Change(_speechInterval, _speechInterval);
        }

        /// <summary>
        /// Called when [timer tick].
        /// </summary>
        internal override void OnTimerTick()
        {
            if (GetBotData() == null) return;

            if (_actionCount > 0)
            {
                _actionCount--;
                return;
            }
            _actionCount = Random.Next(5, 45);

            switch (GetBotData().WalkingMode.ToLower())
            {
                case "freeroam":
                {
                    var randomPoint = GetRoom().GetGameMap().GetRandomWalkableSquare();
                    if (randomPoint.X == 0 || randomPoint.Y == 0) return;

                    GetRoomUser().MoveTo(randomPoint.X, randomPoint.Y);
                    break;
                }
                case "specified_range":
                {
                    var list = GetRoom().GetGameMap().WalkableList.ToList();
                    if (!list.Any()) return;

                    var randomNumber = new Random(DateTime.Now.Millisecond + _virtualId ^ 2).Next(0, list.Count - 1);
                    GetRoomUser().MoveTo(list[randomNumber].X, list[randomNumber].Y);
                    break;
                }
            }
        }

        /// <summary>
        /// Called when [self enter room].
        /// </summary>
        internal override void OnSelfEnterRoom()
        {
        }

        /// <summary>
        /// Called when [self leave room].
        /// </summary>
        /// <param name="kicked">if set to <c>true</c> [kicked].</param>
        internal override void OnSelfLeaveRoom(bool kicked)
        {
        }

        /// <summary>
        /// Called when [user enter room].
        /// </summary>
        /// <param name="user">The user.</param>
        internal override void OnUserEnterRoom(RoomUser user)
        {
        }

        /// <summary>
        /// Called when [user leave room].
        /// </summary>
        /// <param name="client">The client.</param>
        internal override void OnUserLeaveRoom(GameClient client)
        {
        }

        /// <summary>
        /// Called when [user say].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="message">The message.</param>
        internal override void OnUserSay(RoomUser user, string message)
        {
            if (Gamemap.TileDistance(GetRoomUser().X, GetRoomUser().Y, user.X, user.Y) > 16) return;

            if (!_isBartender) return;

            try
            {
                message = message.Substring(1);
            }
            catch
            {
                return;
            }
            switch (message.ToLower())
            {
                case "ven":
                case "comehere":
                case "come here":
                case "ven aquí":
                case "come":
                    GetRoomUser().Chat(null, "¡Voy!", false, 0, 0);
                    GetRoomUser().MoveTo(user.SquareInFront);
                    return;

                case "sirve":
                case "serve":
                    if (GetRoom().CheckRights(user.GetClient()))
                    {
                        foreach (var current in GetRoom().GetRoomUserManager().GetRoomUsers()) current.CarryItem(Random.Next(1, 38));
                        GetRoomUser().Chat(null, "Vale. Ya teneis todos algo para zampar.", false, 0, 0);
                        return;
                    }
                    return;

                case "agua":
                case "té":
                case "te":
                case "tea":
                case "juice":
                case "water":
                case "zumo":
                    GetRoomUser().Chat(null, "Aquí tienes.", false, 0, 0);
                    user.CarryItem(Random.Next(1, 3));
                    return;

                case "helado":
                case "icecream":
                case "ice cream":
                    GetRoomUser().Chat(null, "Aquí tienes. ¡Que no se te quede pegada la lengua, je je!", false, 0, 0);
                    user.CarryItem(4);
                    return;

                case "rose":
                case "rosa":
                    GetRoomUser().Chat(null, "Aquí tienes... que te vaya bien en tu cita.", false, 0, 0);
                    user.CarryItem(Random.Next(1000, 1002));
                    return;

                case "girasol":
                case "sunflower":
                    GetRoomUser().Chat(null, "Aquí tienes algo muy bonito de la naturaleza.", false, 0, 0);
                    user.CarryItem(1002);
                    return;

                case "flor":
                case "flower":
                    GetRoomUser().Chat(null, "Aquí tienes algo muy bonito de la naturaleza.", false, 0, 0);
                    if (Random.Next(1, 3) == 2)
                    {
                        user.CarryItem(Random.Next(1019, 1024));
                        return;
                    }
                    user.CarryItem(Random.Next(1006, 1010));
                    return;

                case "zanahoria":
                case "zana":
                case "carrot":
                    GetRoomUser().Chat(null, "Aquí tienes una buena verdura. ¡Provecho!", false, 0, 0);
                    user.CarryItem(3);
                    return;

                case "café":
                case "cafe":
                case "capuccino":
                case "coffee":
                case "latte":
                case "mocha":
                case "espresso":
                case "expreso":
                    GetRoomUser().Chat(null, "Aquí tienes tu café. ¡Está espumoso!", false, 0, 0);
                    user.CarryItem(Random.Next(11, 18));
                    return;

                case "fruta":
                case "fruit":
                    GetRoomUser().Chat(null, "Aquí tienes algo sano, fresco y natural. ¡Que lo disfrutes!", false, 0, 0);
                    user.CarryItem(Random.Next(36, 40));
                    return;

                case "naranja":
                case "orange":
                    GetRoomUser().Chat(null, "Aquí tienes algo sano, fresco y natural. ¡Que lo disfrutes!", false, 0, 0);
                    user.CarryItem(38);
                    return;

                case "manzana":
                case "apple":
                    GetRoomUser().Chat(null, "Aquí tienes algo sano, fresco y natural. ¡Que lo disfrutes!", false, 0, 0);
                    user.CarryItem(37);
                    return;

                case "cola":
                case "habbocola":
                case "habbo cola":
                case "coca cola":
                case "cocacola":
                    GetRoomUser().Chat(null, "Aquí tienes un refresco bastante famoso.", false, 0, 0);
                    user.CarryItem(19);
                    return;

                case "pear":
                case "pera":
                    GetRoomUser().Chat(null, "Aquí tienes algo sano, fresco y natural. ¡Que lo disfrutes!", false, 0, 0);
                    user.CarryItem(36);
                    return;

                case "ananá":
                case "pineapple":
                case "piña":
                case "rodaja de piña":
                    GetRoomUser().Chat(null, "Aquí tienes algo sano, fresco y natural. ¡Que lo disfrutes!", false, 0, 0);
                    user.CarryItem(39);
                    return;

                case "puta":
                case "puto":
                case "gilipollas":
                case "metemela":
                case "polla":
                case "pene":
                case "penis":
                case "idiot":
                case "fuck":
                case "bastardo":
                case "idiota":
                case "chupamela":
                case "tonta":
                case "tonto":
                case "mierda":
                    GetRoomUser().Chat(null, "¡No me trates así, eh!", true, 0, 0);
                    return;

                case "lindo":
                case "hermoso":
                case "linda":
                case "guapa":
                case "beautiful":
                case "handsome":
                case "love":
                case "guapo":
                case "i love you":
                case "hermosa":
                case "preciosa":
                case "te amo":
                case "amor":
                case "mi amor":
                    GetRoomUser().Chat(null, "Soy un bot, err... esto se está poniendo incómodo, ¿sabes?", false, 0, 0);
                    return;

                case "xdr":
                    GetRoomUser().Chat(null, "Alabado sea el Diox Playero!", true, 0, 0);
                    return;
                case "tyrex":
                    GetRoomUser().Chat(null, "Please call me God Tyrex !", true, 0, 0);
                    return;
            }
            GetRoomUser().Chat(null, "¿Necesitas algo?", false, 0, 0);
        }

        /// <summary>
        /// Called when [user shout].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="message">The message.</param>
        internal override void OnUserShout(RoomUser user, string message)
        {
            if (_isBartender)
            {
                GetRoomUser()
                    .Chat(null, "A mí no me vengas a gritar. Si quieres que te sirva algo, dímelo bien.", false, 0, 0);
            }
        }

        /// <summary>
        /// Stops the timer tick.
        /// </summary>
        private void StopTimerTick()
        {
            if (_chatTimer == null) return;
            _chatTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _chatTimer.Dispose();
            _chatTimer = null;
        }

        /// <summary>
        /// Chats the timer tick.
        /// </summary>
        /// <param name="o">The o.</param>
        private void ChatTimerTick(object o)
        {
            if (GetBotData() == null || GetRoomUser() == null || GetBotData().WasPicked || GetBotData().RandomSpeech == null ||
                !GetBotData().RandomSpeech.Any())
            {
                StopTimerTick();
                return;
            }

            if(GetRoom() != null && GetRoom().MutedBots)
                return;

            var randomSpeech = GetBotData().GetRandomSpeech(GetBotData().MixPhrases);

            try
            {
                switch (randomSpeech)
                {
                    case ":sit":
                    {
                        var user = GetRoomUser();
                        if (user.RotBody % 2 != 0) user.RotBody--;

                        user.Z = GetRoom().GetGameMap().SqAbsoluteHeight(user.X, user.Y);
                        if (!user.Statusses.ContainsKey("sit"))
                        {
                            user.UpdateNeeded = true;
                            user.Statusses.Add("sit", "0.55");
                        }
                        user.IsSitting = true;
                        return;
                    }
                    case ":stand":
                    {
                        var user = GetRoomUser();
                        if (user.IsSitting)
                        {
                            user.Statusses.Remove("sit");
                            user.IsSitting = false;
                            user.UpdateNeeded = true;
                        }
                        else if (user.IsLyingDown)
                        {
                            user.Statusses.Remove("lay");
                            user.IsLyingDown = false;
                            user.UpdateNeeded = true;
                        }
                        return;
                    }
                }

                if (GetRoom() != null)
                {
                    randomSpeech = randomSpeech.Replace("%user_count%",
                        GetRoom().GetRoomUserManager().GetRoomUserCount().ToString());
                    randomSpeech = randomSpeech.Replace("%item_count%",
                        GetRoom().GetRoomItemHandler().TotalItems.ToString());
                    randomSpeech = randomSpeech.Replace("%floor_item_count%",
                        GetRoom().GetRoomItemHandler().FloorItems.Keys.Count.ToString());
                    randomSpeech = randomSpeech.Replace("%wall_item_count%",
                        GetRoom().GetRoomItemHandler().WallItems.Keys.Count.ToString());

                    if (GetRoom().RoomData != null)
                    {
                        randomSpeech = randomSpeech.Replace("%roomname%", GetRoom().RoomData.Name);
                        randomSpeech = randomSpeech.Replace("%owner%", GetRoom().RoomData.Owner);
                    }
                }
                if (GetBotData() != null) randomSpeech = randomSpeech.Replace("%name%", GetBotData().Name);

                GetRoomUser().Chat(null, randomSpeech, false, 0, 0);
            }
            catch (Exception e)
            {
                Writer.Writer.LogException(e.ToString());
            }
        }
    }
}