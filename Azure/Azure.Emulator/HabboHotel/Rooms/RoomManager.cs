﻿using Azure.Configuration;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.HabboHotel.Events;
using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Navigators;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using Azure.Util;

namespace Azure.HabboHotel.Rooms
{
    /// <summary>
    /// Class RoomManager.
    /// </summary>
    internal class RoomManager
    {
        /// <summary>
        /// The active rooms remove queue
        /// </summary>
        public Queue ActiveRoomsRemoveQueue;

        /// <summary>
        /// The loaded rooms
        /// </summary>
        internal ConcurrentDictionary<uint, Room> LoadedRooms;

        internal readonly ConcurrentDictionary<uint, RoomData> LoadedRoomData; 

        /// <summary>
        /// The _voted rooms add queue
        /// </summary>
        private readonly Queue _votedRoomsAddQueue;

        /// <summary>
        /// The _voted rooms remove queue
        /// </summary>
        private readonly Queue _votedRoomsRemoveQueue;

        /// <summary>
        /// The _active rooms update queue
        /// </summary>
        private readonly Queue _activeRoomsUpdateQueue;

        /// <summary>
        /// The _active rooms add queue
        /// </summary>
        private readonly Queue _activeRoomsAddQueue;

        /// <summary>
        /// The _room models
        /// </summary>
        private readonly HybridDictionary _roomModels;

        /// <summary>
        /// The _voted rooms
        /// </summary>
        private readonly Dictionary<RoomData, int> _votedRooms;

        /// <summary>
        /// The _active rooms
        /// </summary>
        private readonly Dictionary<RoomData, uint> _activeRooms;

        /// <summary>
        /// The _event manager
        /// </summary>
        private readonly EventManager _eventManager;

        private RoomCompetitionManager _competitionManager;

        internal RoomCompetitionManager GetCompetitionManager()
        {
            return _competitionManager;
        }

        /// <summary>
        /// The ball rooms add queue
        /// </summary>
        private readonly Queue ballRoomsAddQueue;

        /// <summary>
        /// The ball rooms remove queue
        /// </summary>
        private readonly Queue ballRoomsRemoveQueue;

        /// <summary>
        /// The _ordered voted rooms
        /// </summary>
        private IEnumerable<KeyValuePair<RoomData, int>> _orderedVotedRooms;

        /// <summary>
        /// The _ordered active rooms
        /// </summary>
        private IEnumerable<KeyValuePair<RoomData, uint>> _orderedActiveRooms;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomManager"/> class.
        /// </summary>
        internal RoomManager()
        {
            LoadedRooms = new ConcurrentDictionary<uint, Room>();
            _roomModels = new HybridDictionary();
            LoadedRoomData = new ConcurrentDictionary<uint, RoomData>();
            _votedRooms = new Dictionary<RoomData, int>();
            _activeRooms = new Dictionary<RoomData, uint>();
            _votedRoomsRemoveQueue = new Queue();
            _votedRoomsAddQueue = new Queue();
            ActiveRoomsRemoveQueue = new Queue();
            _activeRoomsUpdateQueue = new Queue();
            _activeRoomsAddQueue = new Queue();
            _eventManager = new EventManager();
        }
        internal void LoadCompetitionManager()
        {
            _competitionManager = new RoomCompetitionManager();
        }

        /// <summary>
        /// Gets the loaded rooms count.
        /// </summary>
        /// <value>The loaded rooms count.</value>
        internal int LoadedRoomsCount
        {
            get { return LoadedRooms.Count; }
        }

        /// <summary>
        /// Queues the ball add.
        /// </summary>
        /// <param name="data">The data.</param>
        internal void QueueBallAdd(Room data)
        {
            lock (ballRoomsAddQueue.SyncRoot)
            {
                ballRoomsAddQueue.Enqueue(data);
            }
        }

        /// <summary>
        /// Queues the ball remove.
        /// </summary>
        /// <param name="data">The data.</param>
        internal void QueueBallRemove(Room data)
        {
            lock (ballRoomsRemoveQueue.SyncRoot)
            {
                ballRoomsRemoveQueue.Enqueue(data);
            }
        }

        /// <summary>
        /// Gets the active rooms.
        /// </summary>
        /// <returns>KeyValuePair&lt;RoomData, System.UInt32&gt;[].</returns>
        internal KeyValuePair<RoomData, uint>[] GetActiveRooms()
        {
            return _orderedActiveRooms == null ? null : _orderedActiveRooms.ToArray();
        }

        /// <summary>
        /// Gets the voted rooms.
        /// </summary>
        /// <returns>KeyValuePair&lt;RoomData, System.Int32&gt;[].</returns>
        internal KeyValuePair<RoomData, int>[] GetVotedRooms()
        {
            return _orderedVotedRooms == null ? null : _orderedVotedRooms.ToArray();
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="roomId">The room identifier.</param>
        /// <returns>RoomModel.</returns>
        internal RoomModel GetModel(string model, uint roomId)
        {
            if (model == "custom" && _roomModels.Contains(string.Format("custom_{0}", roomId))) return (RoomModel)_roomModels[string.Format("custom_{0}", roomId)];
            if (_roomModels.Contains(model)) return (RoomModel)_roomModels[model];
            return null;
        }

        /// <summary>
        /// Generates the nullable room data.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <returns>RoomData.</returns>
        internal RoomData GenerateNullableRoomData(uint roomId)
        {
            if (GenerateRoomData(roomId) != null) return GenerateRoomData(roomId);
            var roomData = new RoomData();
            roomData.FillNull(roomId);
            return roomData;
        }

        /// <summary>
        /// Generates the room data.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <returns>RoomData.</returns>
        internal RoomData GenerateRoomData(uint roomId)
        {
            if (LoadedRoomData.ContainsKey(roomId))
            {
                LoadedRoomData[roomId].LastUsed = DateTime.Now;
                return LoadedRoomData[roomId];
            }

            var roomData = new RoomData();

            if (LoadedRooms.ContainsKey(roomId))
                return GetRoom(roomId).RoomData;

            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Format("SELECT * FROM rooms_data WHERE id = {0} LIMIT 1", roomId));

                DataRow dataRow = queryReactor.GetRow();
                if (dataRow == null)
                    return null;

                roomData.Fill(dataRow);
            }

            LoadedRoomData.TryAdd(roomId, roomData);

            return roomData;
        }

        /// <summary>
        /// Gets the event rooms.
        /// </summary>
        /// <returns>KeyValuePair&lt;RoomData, System.UInt32&gt;[].</returns>
        internal KeyValuePair<RoomData, uint>[] GetEventRooms()
        {
            return _eventManager.GetRooms();
        }

        /// <summary>
        /// Loads the room.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Room.</returns>
        internal Room LoadRoom(uint id)
        {
            if (LoadedRooms.ContainsKey(id))
                return GetRoom(id);

            var roomData = GenerateRoomData(id);
            if (roomData == null)
                return null;
            if (LoadedRooms.ContainsKey(id))
                return GetRoom(id);

            var room = new Room();
            LoadedRooms.AddOrUpdate(id, room, (key, value) => room);
            room.Start(roomData);

            Out.WriteLine(string.Format("Room #{0} was loaded", id), "Azure.Room.Manager", ConsoleColor.DarkCyan);

            room.InitBots();
            room.InitPets();
            return room;
        }

        internal void RemoveRoomData(uint id)
        {
            RoomData dataJunk;
            LoadedRoomData.TryRemove(id, out dataJunk);
        }

        /// <summary>
        /// Fetches the room data.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="dRow">The d row.</param>
        /// <returns>RoomData.</returns>
        internal RoomData FetchRoomData(uint roomId, DataRow dRow)
        {
            if (LoadedRoomData.ContainsKey(roomId))
            {
                LoadedRoomData[roomId].LastUsed = DateTime.Now;
                return LoadedRoomData[roomId];
            }
            var roomData = new RoomData();
            roomData.Fill(dRow);
            LoadedRoomData.TryAdd(roomId, roomData);
            return roomData;
        }

        /// <summary>
        /// Gets the room.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <returns>Room.</returns>
        internal Room GetRoom(uint roomId)
        {
            Room result;
            return LoadedRooms.TryGetValue(roomId, out result) ? result : null;
        }

        /// <summary>
        /// Creates the room.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="name">The name.</param>
        /// <param name="desc">The desc.</param>
        /// <param name="model">The model.</param>
        /// <param name="category">The category.</param>
        /// <param name="maxVisitors">The maximum visitors.</param>
        /// <param name="tradeState">State of the trade.</param>
        /// <returns>RoomData.</returns>
        internal RoomData CreateRoom(GameClient session, string name, string desc, string model, int category,
                                     int maxVisitors, int tradeState)
        {
            if (!_roomModels.Contains(model))
            {
                session.SendNotif(Azure.GetLanguage().GetVar("user_room_model_error"));

                return null;
            }

            uint roomId;
            using (var dbClient = Azure.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO rooms_data (roomtype,caption,description,owner,model_name,category,users_max,trade_state) VALUES ('private',@caption,@desc,@Username,@model,@cat,@usmax,@tstate)");
                dbClient.AddParameter("caption", name);
                dbClient.AddParameter("desc", desc);
                dbClient.AddParameter("Username", session.GetHabbo().UserName);
                dbClient.AddParameter("model", model);
                dbClient.AddParameter("cat", category);
                dbClient.AddParameter("usmax", maxVisitors);
                dbClient.AddParameter("tstate", tradeState.ToString());
                roomId = (uint)dbClient.InsertQuery();
            }
            var data = GenerateRoomData(roomId);
            if (data == null) return null;

            session.GetHabbo().UsersRooms.Add(data);
            return data;
        }

        /// <summary>
        /// Initializes the voted rooms.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void InitVotedRooms(IQueryAdapter dbClient)
        {
            dbClient.SetQuery(
                "SELECT * FROM rooms_data WHERE score > 0 AND roomtype = 'private' ORDER BY score DESC LIMIT 40");
            var table = dbClient.GetTable();
            foreach (
                var data in
                    from DataRow dataRow in table.Rows select FetchRoomData(Convert.ToUInt32(dataRow["id"]), dataRow))
                QueueVoteAdd(data);
        }

        /// <summary>
        /// Loads the new model.
        /// </summary>
        /// <param name="model">The model.</param>
        internal void LoadNewModel(string model)
        {
            if (_roomModels.Contains(model)) _roomModels.Remove(model);

            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    "SELECT id,door_x,door_y,door_z,door_dir,heightmap,public_items,club_only,poolmap FROM rooms_models WHERE id = @model");
                queryReactor.AddParameter("model", model);
                var table = queryReactor.GetTable();
                if (table == null) return;
                foreach (DataRow row in table.Rows)
                {
                    var staticFurniMap = (string)row["public_items"];
                    _roomModels.Add(model,
                        new RoomModel((int)row["door_x"], (int)row["door_y"], (double)row["door_z"],
                            (int)row["door_dir"], (string)row["heightmap"], staticFurniMap,
                            Azure.EnumToBool(row["club_only"].ToString()), (string)row["poolmap"]));
                }
            }
        }

        /// <summary>
        /// Loads the models.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        /// <param name="loadedModel">The loaded model.</param>
        internal void LoadModels(IQueryAdapter dbClient, out uint loadedModel)
        {
            LoadModels(dbClient);
            loadedModel = (uint)_roomModels.Count;
        }

        /// <summary>
        /// Loads the models.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void LoadModels(IQueryAdapter dbClient)
        {
            _roomModels.Clear();
            dbClient.SetQuery(
                "SELECT id,door_x,door_y,door_z,door_dir,heightmap,public_items,club_only,poolmap FROM rooms_models");
            var table = dbClient.GetTable();
            if (table == null) return;
            foreach (DataRow dataRow in table.Rows)
            {
                var key = (string)dataRow["id"];
                if (key.StartsWith("model_floorplan_")) continue;
                var staticFurniMap = (string)dataRow["public_items"];
                _roomModels.Add(key,
                    new RoomModel((int)dataRow["door_x"], (int)dataRow["door_y"], (double)dataRow["door_z"],
                        (int)dataRow["door_dir"], (string)dataRow["heightmap"], staticFurniMap,
                        Azure.EnumToBool(dataRow["club_only"].ToString()), (string)dataRow["poolmap"]));
            }
            dbClient.SetQuery("SELECT roomid,door_x,door_y,door_z,door_dir,heightmap FROM rooms_models_customs");
            var dataCustom = dbClient.GetTable();

            if (dataCustom == null) return;

            foreach (DataRow row in dataCustom.Rows)
            {
                var modelName = string.Format("custom_{0}", row["roomid"]);
                _roomModels.Add(modelName,
                    new RoomModel((int)row["door_x"], (int)row["door_y"], (double)row["door_z"],
                        (int)row["door_dir"],
                        (string)row["heightmap"], "", false, ""));
            }
        }

        /// <summary>
        /// Called when [cycle].
        /// </summary>
        internal void OnCycle()
        {
            try
            {
                var flag = WorkActiveRoomsAddQueue();
                var flag2 = WorkActiveRoomsRemoveQueue();
                var flag3 = WorkActiveRoomsUpdateQueue();
                if (flag || flag2 || flag3) SortActiveRooms();
                var flag4 = WorkVotedRoomsAddQueue();
                var flag5 = WorkVotedRoomsRemoveQueue();
                if (flag4 || flag5) SortVotedRooms();

                Azure.GetGame().RoomManagerCycleEnded = true;
            }
            catch (Exception ex)
            {
                Logging.LogThreadException(ex.ToString(), "RoomManager.OnCycle Exception --> Not inclusive");
            }
        }

        /// <summary>
        /// Queues the vote add.
        /// </summary>
        /// <param name="data">The data.</param>
        internal void QueueVoteAdd(RoomData data)
        {
            lock (_votedRoomsAddQueue.SyncRoot)
            {
                _votedRoomsAddQueue.Enqueue(data);
            }
        }

        /// <summary>
        /// Queues the vote remove.
        /// </summary>
        /// <param name="data">The data.</param>
        internal void QueueVoteRemove(RoomData data)
        {
            lock (_votedRoomsRemoveQueue.SyncRoot)
            {
                _votedRoomsRemoveQueue.Enqueue(data);
            }
        }

        /// <summary>
        /// Queues the active room update.
        /// </summary>
        /// <param name="data">The data.</param>
        internal void QueueActiveRoomUpdate(RoomData data)
        {
            lock (_activeRoomsUpdateQueue.SyncRoot)
            {
                _activeRoomsUpdateQueue.Enqueue(data);
            }
        }

        /// <summary>
        /// Queues the active room add.
        /// </summary>
        /// <param name="data">The data.</param>
        internal void QueueActiveRoomAdd(RoomData data)
        {
            lock (_activeRoomsAddQueue.SyncRoot)
            {
                _activeRoomsAddQueue.Enqueue(data);
            }
        }

        /// <summary>
        /// Queues the active room remove.
        /// </summary>
        /// <param name="data">The data.</param>
        internal void QueueActiveRoomRemove(RoomData data)
        {
            lock (ActiveRoomsRemoveQueue.SyncRoot)
            {
                ActiveRoomsRemoveQueue.Enqueue(data);
            }
        }

        /// <summary>
        /// Removes all rooms.
        /// </summary>
        internal void RemoveAllRooms()
        {
            foreach (var current in LoadedRooms.Values)
                Azure.GetGame().GetRoomManager().UnloadRoom(current, "RemoveAllRooms void called");

            Out.WriteLine("RoomManager Destroyed", "Azure.RoomManager", ConsoleColor.DarkYellow);
        }

        /// <summary>
        /// Unloads the room.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <param name="reason">The reason.</param>
        internal void UnloadRoom(Room room, string reason)
        {
            if (room == null || room.RoomData == null || room.Disposed) return;
            room.Disposed = true;

            if (Azure.GetGame().GetNavigator().PrivateCategories.Contains(room.RoomData.Category))
            {
                ((FlatCat) Azure.GetGame().GetNavigator().PrivateCategories[room.RoomData.Category]).UsersNow -=
                    room.UserCount;
            }
            room.RoomData.UsersNow = 0;
            var state = "open";

            if (room.RoomData.State == 1) state = "locked";
            else if (room.RoomData.State > 1) state = "password";

            var roomId = room.RoomId;

            try
            {

                using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery(
                        "UPDATE rooms_data SET caption = @caption, description = @description, password = @password, category = " +
                        room.RoomData.Category + ", state = '" + state +
                        "', tags = @tags, users_now = '0', users_max = " +
                        room.RoomData.UsersMax + ", allow_pets = '" + Azure.BoolToEnum(room.RoomData.AllowPets) +
                        "', allow_pets_eat = '" +
                        Azure.BoolToEnum(room.RoomData.AllowPetsEating) + "', allow_walkthrough = '" +
                        Azure.BoolToEnum(room.RoomData.AllowWalkThrough) +
                        "', hidewall = '" + Azure.BoolToEnum(room.RoomData.HideWall) + "', floorthick = " +
                        room.RoomData.FloorThickness +
                        ", wallthick = " + room.RoomData.WallThickness + ", mute_settings='" + room.RoomData.WhoCanMute +
                        "', kick_settings='" + room.RoomData.WhoCanKick + "',ban_settings='" + room.RoomData.WhoCanBan +
                        "', walls_height = '" + room.RoomData.WallHeight +
                        "', chat_type = @chat_t,chat_balloon = @chat_b,chat_speed = @chat_s,chat_max_distance = @chat_m,chat_flood_protection = @chat_f, trade_state = '" +
                        room.RoomData.TradeState + "' WHERE id = " + roomId);
                    queryReactor.AddParameter("caption", room.RoomData.Name);
                    queryReactor.AddParameter("description", room.RoomData.Description);
                    queryReactor.AddParameter("password", room.RoomData.PassWord);
                    queryReactor.AddParameter("tags", string.Join(",", room.RoomData.Tags));
                    queryReactor.AddParameter("chat_t", room.RoomData.ChatType);
                    queryReactor.AddParameter("chat_b", room.RoomData.ChatBalloon);
                    queryReactor.AddParameter("chat_s", room.RoomData.ChatSpeed);
                    queryReactor.AddParameter("chat_m", room.RoomData.ChatMaxDistance);
                    queryReactor.AddParameter("chat_f", room.RoomData.ChatFloodProtection);
                    queryReactor.RunQuery();
                }
            }
            catch (Exception e)
            {
                Writer.Writer.LogException(e.ToString());
            }

            Room junkRoom;
            LoadedRooms.TryRemove(room.RoomId, out junkRoom);
            junkRoom = null;

            Out.WriteLine(string.Format("Room #{0} was unloaded, reason: " + reason, room.RoomId),
                "Azure.Room.Manager", ConsoleColor.DarkGray);

            if (room.GetRoomUserManager() != null && room.GetRoomUserManager().UserList != null)
            {
                using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    foreach (var current in room.GetRoomUserManager().UserList.Values.Where(current => current != null))
                    {
                        if (current.IsPet)
                        {
                            if (current.PetData == null) continue;
                            queryReactor.SetQuery("UPDATE bots SET x=@x, y=@y, z=@z WHERE id=@id LIMIT 1;");
                            queryReactor.AddParameter("x", current.X);
                            queryReactor.AddParameter("y", current.Y);
                            queryReactor.AddParameter("z", current.Z);
                            queryReactor.AddParameter("id", current.PetData.PetId);
                            queryReactor.RunQuery();

                            current.BotAI.Dispose();
                        }
                        else if (current.IsBot)
                        {
                            if (current.BotData == null) continue;
                            queryReactor.SetQuery(
                                "UPDATE bots SET x=@x, y=@y, z=@z, name=@name, motto=@motto, look=@look, rotation=@rotation, dance=@dance WHERE id=@id LIMIT 1;");
                            queryReactor.AddParameter("name", current.BotData.Name);
                            queryReactor.AddParameter("motto", current.BotData.Motto);
                            queryReactor.AddParameter("look", current.BotData.Look);
                            queryReactor.AddParameter("rotation", current.BotData.Rot);
                            queryReactor.AddParameter("dance", current.BotData.DanceId);
                            queryReactor.AddParameter("x", current.X);
                            queryReactor.AddParameter("y", current.Y);
                            queryReactor.AddParameter("z", current.Z);
                            queryReactor.AddParameter("id", current.BotData.BotId);
                            queryReactor.RunQuery();

                            if (current.BotAI == null) continue;
                            current.BotAI.Dispose();
                        }
                        else
                        {
                            if (current.GetClient() == null) continue;
                            room.GetRoomUserManager().RemoveUserFromRoom(current.GetClient(), true, false);
                            current.GetClient().CurrentRoomUserId = -1;
                        }
                    }
                }
            }

            var queryChunk = new QueryChunk();
            queryChunk.AddParameter("room", roomId);
            var i = 0u;
            foreach (var msg in room.RoomData.RoomChat)
            {
                msg.Save(queryChunk, i);
                i++;
            }

            queryChunk.Execute(Azure.GetDatabaseManager().GetQueryReactor());
            queryChunk.Dispose();

            room.Destroy();
            room = null;
        }

        /// <summary>
        /// Sorts the active rooms.
        /// </summary>
        private void SortActiveRooms()
        {
            _orderedActiveRooms = _activeRooms.OrderByDescending(t => t.Value).Take(40);
        }

        /// <summary>
        /// Sorts the voted rooms.
        /// </summary>
        private void SortVotedRooms()
        {
            _orderedVotedRooms = _votedRooms.OrderByDescending(t => t.Value).Take(40);
        }

        /// <summary>
        /// Works the active rooms update queue.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool WorkActiveRoomsUpdateQueue()
        {
            if (_activeRoomsUpdateQueue.Count <= 0) return false;
            lock (_activeRoomsUpdateQueue.SyncRoot)
            {
                while (_activeRoomsUpdateQueue.Count > 0)
                {
                    var roomData = (RoomData)_activeRoomsUpdateQueue.Dequeue();
                    if (roomData == null || roomData.ModelName.Contains("snowwar")) continue;
                    if (!_activeRooms.ContainsKey(roomData)) _activeRooms.Add(roomData, roomData.UsersNow);
                    else _activeRooms[roomData] = roomData.UsersNow;
                }
            }
            return true;
        }

        /// <summary>
        /// Works the active rooms add queue.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool WorkActiveRoomsAddQueue()
        {
            if (_activeRoomsAddQueue.Count <= 0) return false;
            lock (_activeRoomsAddQueue.SyncRoot)
            {
                while (_activeRoomsAddQueue.Count > 0)
                {
                    var roomData = (RoomData)_activeRoomsAddQueue.Dequeue();
                    if (!_activeRooms.ContainsKey(roomData) && !roomData.ModelName.Contains("snowwar")) _activeRooms.Add(roomData, roomData.UsersNow);
                }
            }
            return true;
        }

        /// <summary>
        /// Works the active rooms remove queue.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool WorkActiveRoomsRemoveQueue()
        {
            if (ActiveRoomsRemoveQueue.Count <= 0) return false;
            lock (ActiveRoomsRemoveQueue.SyncRoot)
            {
                while (ActiveRoomsRemoveQueue.Count > 0)
                {
                    var key = (RoomData)ActiveRoomsRemoveQueue.Dequeue();
                    _activeRooms.Remove(key);
                }
            }
            return true;
        }

        /// <summary>
        /// Works the voted rooms add queue.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool WorkVotedRoomsAddQueue()
        {
            if (_votedRoomsAddQueue.Count <= 0) return false;
            lock (_votedRoomsAddQueue.SyncRoot)
            {
                while (_votedRoomsAddQueue.Count > 0)
                {
                    var roomData = (RoomData)_votedRoomsAddQueue.Dequeue();
                    if (!_votedRooms.ContainsKey(roomData)) _votedRooms.Add(roomData, roomData.Score);
                    else _votedRooms[roomData] = roomData.Score;
                }
            }
            return true;
        }

        /// <summary>
        /// Works the voted rooms remove queue.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool WorkVotedRoomsRemoveQueue()
        {
            if (_votedRoomsRemoveQueue.Count <= 0) return false;
            lock (_votedRoomsRemoveQueue.SyncRoot)
            {
                while (_votedRoomsRemoveQueue.Count > 0)
                {
                    var key = (RoomData)_votedRoomsRemoveQueue.Dequeue();
                    _votedRooms.Remove(key);
                }
            }
            return true;
        }
    }
}