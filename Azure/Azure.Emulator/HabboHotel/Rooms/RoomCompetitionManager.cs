using Azure.HabboHotel.Users;
using Azure.Messages;
using Azure.Messages.Parsers;
using System.Collections.Generic;
using System.Data;

namespace Azure.HabboHotel.Rooms
{
    /// <summary>
    /// Class RoomCompetition.
    /// </summary>
    internal class RoomCompetition
    {
        internal int Id;
        internal string Name;
        internal string[] RequiredFurnis;
        internal Dictionary<uint, RoomData> Entries;
        public RoomCompetition(int id, string name, string requiredFurnis)
        {
            Id = id;
            Name = name;
            RequiredFurnis = requiredFurnis.Split(';');
            Entries = new Dictionary<uint, RoomData>();
            DataTable table;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT * FROM rooms_competitions_entries WHERE competition_id = " + Id);
                table = queryReactor.GetTable();
                if (table == null) return;
                foreach (DataRow row in table.Rows)
                {
                    var room_id = (uint)row["room_id"];
                    var roomData = Azure.GetGame().GetRoomManager().GenerateRoomData(room_id);
                    if (roomData == null) return;
                    roomData.CompetitionStatus = (int)row["status"];
                    roomData.CompetitionVotes = (int)row["votes"];
                    if (Entries.ContainsKey(room_id)) return;
                    Entries.Add(room_id, roomData);
                }
            }
        }
        internal bool HasAllRequiredFurnis(Room room)
        {
            if (room == null) return false;
            foreach (string furni in RequiredFurnis)
            {
                if (!room.GetRoomItemHandler().HasFurniByItemName(furni))
                {
                    return false;
                }
            }
            return true;
        }
        internal ServerMessage AppendEntrySubmitMessage(ServerMessage message, int status, Room room = null)
        {
            message.Init(LibraryParser.OutgoingRequest("CompetitionEntrySubmitResultMessageComposer"));
            message.AppendInteger(Id);
            message.AppendString(Name);
            message.AppendInteger(status);// 0 : roomSent - 1 : send room - 2 : confirm register - 3 : neededFurnis - 4 : doorClosed - 6 : acceptRules
            if (status != 3)
            {
                message.AppendInteger(0);
                message.AppendInteger(0);
            }
            else
            {
                message.StartArray();
                foreach(string furni in RequiredFurnis)
                {
                    message.AppendString(furni);
                    message.SaveArray();
                }
                message.EndArray();
                if (room == null)
                {
                    message.AppendInteger(0);
                }
                else
                {
                    message.StartArray();
                    foreach (string furni in RequiredFurnis)
                    {
                        if (!room.GetRoomItemHandler().HasFurniByItemName(furni))
                        {
                            message.AppendString(furni);
                            message.SaveArray();
                        }
                    }
                    message.EndArray();
                }
            }
            return message;
        }
        internal ServerMessage AppendVoteMessage(ServerMessage message, Habbo user, int status = 0)
        {
            message.Init(LibraryParser.OutgoingRequest("CompetitionVotingInfoMessageComposer"));
            message.AppendInteger(Id);
            message.AppendString(Name);
            message.AppendInteger(status);// 0 : vote - 1 : can't vote - 2 : you need the vote badge
            message.AppendInteger(user.DailyCompetitionVotes);
            return message;
        }
    }
    /// <summary>
    /// Class RoomCompetitionManager.
    /// </summary>
    internal class RoomCompetitionManager
    {
        internal RoomCompetition Competition;
        public RoomCompetitionManager()
        {
            RefreshCompetitions();
        }
        public void RefreshCompetitions()
        {
            Competition = null;
            DataRow row;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT * FROM rooms_competitions WHERE enabled = '1' LIMIT 1");
                row = queryReactor.GetRow();
                if (row == null) return;
                Competition = new RoomCompetition((int)row["id"], (string)row["name"], (string)row["required_furnis"]);
            }
        }
    }
}