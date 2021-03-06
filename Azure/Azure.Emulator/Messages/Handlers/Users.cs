﻿using Azure.Configuration;
using Azure.HabboHotel.Quests;
using Azure.HabboHotel.Quests.Composer;
using Azure.HabboHotel.Rooms;
using Azure.HabboHotel.Users;
using Azure.HabboHotel.Users.Badges;
using Azure.HabboHotel.Users.Relationships;
using Azure.Messages.Parsers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Azure.Messages.Handlers
{
    /// <summary>
    /// Class GameClientMessageHandler.
    /// </summary>
    partial class GameClientMessageHandler
    {
        /// <summary>
        /// Sends the bully report.
        /// </summary>
        public void SendBullyReport()
        {
            var reportedId = Request.GetUInteger();
            Azure.GetGame()
                .GetModerationTool()
                .SendNewTicket(Session, 104, 9, reportedId, "", new List<string>());

            Response.Init(LibraryParser.OutgoingRequest("BullyReportSentMessageComposer"));
            Response.AppendInteger(0);
            SendResponse();
        }

        /// <summary>
        /// Opens the bully reporting.
        /// </summary>
        public void OpenBullyReporting()
        {
            Response.Init(LibraryParser.OutgoingRequest("OpenBullyReportMessageComposer"));
            Response.AppendInteger(0);
            SendResponse();
        }

        /// <summary>
        /// Opens the quests.
        /// </summary>
        public void OpenQuests()
        {
            Azure.GetGame().GetQuestManager().GetList(Session, Request);
        }

        /// <summary>
        /// Retrieves the citizenship.
        /// </summary>
        internal void RetrieveCitizenship()
        {
            GetResponse().Init(LibraryParser.OutgoingRequest("CitizenshipStatusMessageComposer"));
            GetResponse().AppendString(Request.GetString());
            GetResponse().AppendInteger(4);
            GetResponse().AppendInteger(4);
        }

        /// <summary>
        /// Loads the club gifts.
        /// </summary>
        internal void LoadClubGifts()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;
            //var i = 0;
            //var i2 = 0;
            Session.GetHabbo().GetSubscriptionManager().GetSubscription();
            var serverMessage = new ServerMessage();
            serverMessage.Init(LibraryParser.OutgoingRequest("LoadCatalogClubGiftsMessageComposer"));
            serverMessage.AppendInteger(0); // i
            serverMessage.AppendInteger(0); // i2
            serverMessage.AppendInteger(1);
        }

        /// <summary>
        /// Chooses the club gift.
        /// </summary>
        internal void ChooseClubGift()
        {
            if (Session == null || Session.GetHabbo() == null)
                return;
            Request.GetString();
        }

        /// <summary>
        /// Gets the user tags.
        /// </summary>
        internal void GetUserTags()
        {
            var room = Azure.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);
            if (room == null)
                return;
            var roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(Request.GetUInteger());
            if (roomUserByHabbo == null || roomUserByHabbo.IsBot)
                return;
            Response.Init(LibraryParser.OutgoingRequest("UserTagsMessageComposer"));
            Response.AppendInteger(roomUserByHabbo.GetClient().GetHabbo().Id);
            Response.AppendInteger(roomUserByHabbo.GetClient().GetHabbo().Tags.Count);
            foreach (string current in roomUserByHabbo.GetClient().GetHabbo().Tags)
                Response.AppendString(current);
            SendResponse();

            if (Session != roomUserByHabbo.GetClient())
                return;
            if (Session.GetHabbo().Tags.Count >= 5)
                Azure.GetGame()
                    .GetAchievementManager()
                    .ProgressUserAchievement(roomUserByHabbo.GetClient(), "ACH_UserTags", 5, false);
        }

        /// <summary>
        /// Gets the user badges.
        /// </summary>
        internal void GetUserBadges()
        {
            var room = Azure.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);
            if (room == null)
                return;
            var roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(Request.GetUInteger());
            if (roomUserByHabbo == null || roomUserByHabbo.IsBot)
                return;
            if (roomUserByHabbo.GetClient() == null)
                return;

            Session.GetHabbo().LastSelectedUser = roomUserByHabbo.UserId;
            Response.Init(LibraryParser.OutgoingRequest("UserBadgesMessageComposer"));
            Response.AppendInteger(roomUserByHabbo.GetClient().GetHabbo().Id);

            Response.StartArray();
            foreach (
                var badge in
                    roomUserByHabbo.GetClient()
                        .GetHabbo()
                        .GetBadgeComponent()
                        .BadgeList.Values.Cast<Badge>()
                        .Where(badge => badge.Slot > 0).Take(5))
            {
                Response.AppendInteger(badge.Slot);
                Response.AppendString(badge.Code);

                Response.SaveArray();
            }

            Response.EndArray();
            SendResponse();
        }

        /// <summary>
        /// Gives the respect.
        /// </summary>
        internal void GiveRespect()
        {
            var room = Azure.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);
            if (room == null || Session.GetHabbo().DailyRespectPoints <= 0)
                return;
            var roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(Request.GetUInteger());
            if (roomUserByHabbo == null || roomUserByHabbo.GetClient().GetHabbo().Id == Session.GetHabbo().Id ||
                roomUserByHabbo.IsBot)
                return;
            Azure.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SocialRespect, 0u);
            Azure.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_RespectGiven", 1, false);
            Azure.GetGame()
                .GetAchievementManager()
                .ProgressUserAchievement(roomUserByHabbo.GetClient(), "ACH_RespectEarned", 1, false);

            {
                Session.GetHabbo().DailyRespectPoints--;
                roomUserByHabbo.GetClient().GetHabbo().Respect++;
                using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                    queryReactor.RunFastQuery("UPDATE users_stats SET respect = respect + 1 WHERE id = " + roomUserByHabbo.GetClient().GetHabbo().Id + " LIMIT 1;UPDATE users_stats SET daily_respect_points = daily_respect_points - 1 WHERE id= " + Session.GetHabbo().Id + " LIMIT 1");
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("GiveRespectsMessageComposer"));
                serverMessage.AppendInteger(roomUserByHabbo.GetClient().GetHabbo().Id);
                serverMessage.AppendInteger(roomUserByHabbo.GetClient().GetHabbo().Respect);
                room.SendMessage(serverMessage);

                var thumbsUp = new ServerMessage();
                thumbsUp.Init(LibraryParser.OutgoingRequest("RoomUserActionMessageComposer"));
                thumbsUp.AppendInteger(
                    room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().UserName).VirtualId);
                thumbsUp.AppendInteger(7);
                room.SendMessage(thumbsUp);
            }
        }

        /// <summary>
        /// Applies the effect.
        /// </summary>
        internal void ApplyEffect()
        {
            var effectId = Request.GetInteger();
            var roomUserByHabbo =
                Azure.GetGame()
                    .GetRoomManager()
                    .GetRoom(Session.GetHabbo().CurrentRoomId)
                    .GetRoomUserManager()
                    .GetRoomUserByHabbo(Session.GetHabbo().UserName);
            if (!roomUserByHabbo.RidingHorse)
                Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(effectId);
        }

        /// <summary>
        /// Enables the effect.
        /// </summary>
        internal void EnableEffect()
        {
            var currentRoom = Session.GetHabbo().CurrentRoom;
            if (currentRoom == null)
                return;
            var roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            var num = Request.GetInteger();
            if (roomUserByHabbo.RidingHorse)
                return;
            if (num == 0)
            {
                Session.GetHabbo()
                    .GetAvatarEffectsInventoryComponent()
                    .StopEffect(Session.GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect);
                return;
            }
            Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateEffect(num);
        }

        /// <summary>
        /// Mutes the user.
        /// </summary>
        internal void MuteUser()
        {
            var num = Request.GetUInteger();
            Request.GetUInteger();
            var num2 = Request.GetUInteger();
            var currentRoom = Session.GetHabbo().CurrentRoom;
            if (currentRoom == null || (currentRoom.RoomData.WhoCanBan == 0 && !currentRoom.CheckRights(Session, true, false)) ||
                (currentRoom.RoomData.WhoCanBan == 1 && !currentRoom.CheckRights(Session)) || Session.GetHabbo().Rank < Convert.ToUInt32(Azure.GetDbConfig().DbData["ambassador.minrank"]))
                return;
            var roomUserByHabbo = currentRoom.GetRoomUserManager()
                .GetRoomUserByHabbo(Azure.GetHabboById(num).UserName);
            if (roomUserByHabbo == null)
                return;
            if (roomUserByHabbo.GetClient().GetHabbo().Rank >= Session.GetHabbo().Rank)
                return;
            if (currentRoom.MutedUsers.ContainsKey(num))
            {
                if (currentRoom.MutedUsers[num] >= (ulong)Azure.GetUnixTimeStamp())
                    return;
                currentRoom.MutedUsers.Remove(num);
            }
            currentRoom.MutedUsers.Add(num,
                uint.Parse(
                    ((Azure.GetUnixTimeStamp()) + unchecked(checked(num2 * 60u))).ToString()));

            roomUserByHabbo.GetClient()
                .SendNotif(string.Format(Azure.GetLanguage().GetVar("room_owner_has_mute_user"), num2));
        }

        /// <summary>
        /// Gets the user information.
        /// </summary>
        internal void GetUserInfo()
        {
            GetResponse().Init(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
            GetResponse().AppendInteger(-1);
            GetResponse().AppendString(Session.GetHabbo().Look);
            GetResponse().AppendString(Session.GetHabbo().Gender.ToLower());
            GetResponse().AppendString(Session.GetHabbo().Motto);
            GetResponse().AppendInteger(Session.GetHabbo().AchievementPoints);
            SendResponse();
            GetResponse().Init(LibraryParser.OutgoingRequest("AchievementPointsMessageComposer"));
            GetResponse().AppendInteger(Session.GetHabbo().AchievementPoints);
            SendResponse();
        }

        /// <summary>
        /// Gets the balance.
        /// </summary>
        internal void GetBalance()
        {
            if (Session == null || Session.GetHabbo() == null) return;

            Session.GetHabbo().UpdateCreditsBalance();
            Session.GetHabbo().UpdateSeasonalCurrencyBalance();
        }

        /// <summary>
        /// Gets the subscription data.
        /// </summary>
        internal void GetSubscriptionData()
        {
            Session.GetHabbo().SerializeClub();
        }

        /// <summary>
        /// Loads the settings.
        /// </summary>
        internal void LoadSettings()
        {
            var preferences = Session.GetHabbo().Preferences;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("LoadVolumeMessageComposer"));

            serverMessage.AppendIntegersArray(preferences.Volume, ',', 3, 0, 100);

            serverMessage.AppendBool(preferences.PreferOldChat);
            serverMessage.AppendBool(preferences.IgnoreRoomInvite);
            serverMessage.AppendBool(preferences.DisableCameraFollow);
            serverMessage.AppendInteger(3); // collapse friends (3 = no)
            serverMessage.AppendInteger(0); //bubble
            this.Session.SendMessage(serverMessage);
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        internal void SaveSettings()
        {
            var num = Request.GetInteger();
            var num2 = Request.GetInteger();
            var num3 = Request.GetInteger();
            Session.GetHabbo().Preferences.Volume = num + "," + num2 + "," + num3;
            Session.GetHabbo().Preferences.Save();
        }

        /// <summary>
        /// Sets the chat preferrence.
        /// </summary>
        internal void SetChatPreferrence()
        {
            bool enable = Request.GetBool();
            Session.GetHabbo().Preferences.PreferOldChat = enable;
            Session.GetHabbo().Preferences.Save();
        }

        internal void SetInvitationsPreference()
        {
            bool enable = Request.GetBool();
            Session.GetHabbo().Preferences.IgnoreRoomInvite = enable;
            Session.GetHabbo().Preferences.Save();
        }
        internal void SetRoomCameraPreferences()
        {
            bool enable = Request.GetBool();
            Session.GetHabbo().Preferences.DisableCameraFollow = enable;
            Session.GetHabbo().Preferences.Save();
        }

        /// <summary>
        /// Gets the badges.
        /// </summary>
        internal void GetBadges()
        {
            Session.SendMessage(Session.GetHabbo().GetBadgeComponent().Serialize());
        }

        /// <summary>
        /// Updates the badges.
        /// </summary>
        internal void UpdateBadges()
        {
            Session.GetHabbo().GetBadgeComponent().ResetSlots();
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery(string.Format("UPDATE users_badges SET badge_slot = 0 WHERE user_id = {0}",
                    Session.GetHabbo().Id));
            for (var i = 0; i < 5; i++)
            {
                var slot = Request.GetInteger();
                var code = Request.GetString();
                if (code.Length == 0) continue;
                if (!Session.GetHabbo().GetBadgeComponent().HasBadge(code) || slot < 1 || slot > 5) return;
                Session.GetHabbo().GetBadgeComponent().GetBadge(code).Slot = slot;
                using (var queryreactor2 = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryreactor2.SetQuery("UPDATE users_badges SET badge_slot = " + slot +
                                           " WHERE badge_id = @badge AND user_id = " + Session.GetHabbo().Id);
                    queryreactor2.AddParameter("badge", code);
                    queryreactor2.RunQuery();
                }
            }
            Azure.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.ProfileBadge, 0u);
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UserBadgesMessageComposer"));
            serverMessage.AppendInteger(Session.GetHabbo().Id);

            serverMessage.StartArray();
            foreach (
                var badge in
                    Session.GetHabbo()
                        .GetBadgeComponent()
                        .BadgeList.Values.Cast<Badge>()
                        .Where(badge => badge.Slot > 0))
            {
                serverMessage.AppendInteger(badge.Slot);
                serverMessage.AppendString(badge.Code);

                serverMessage.SaveArray();
            }

            serverMessage.EndArray();
            if (Session.GetHabbo().InRoom &&
                Azure.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId) != null)
            {
                Azure.GetGame()
                    .GetRoomManager()
                    .GetRoom(Session.GetHabbo().CurrentRoomId)
                    .SendMessage(serverMessage);
                return;
            }
            Session.SendMessage(serverMessage);
        }

        /// <summary>
        /// Gets the achievements.
        /// </summary>
        internal void GetAchievements()
        {
            Azure.GetGame().GetAchievementManager().GetList(Session, Request);
        }

        /// <summary>
        /// Prepares the campaing.
        /// </summary>
        internal void PrepareCampaing()
        {
            var text = Request.GetString();
            Response.Init(LibraryParser.OutgoingRequest("SendCampaignBadgeMessageComposer"));
            Response.AppendString(text);
            Response.AppendBool(Session.GetHabbo().GetBadgeComponent().HasBadge(text));
            SendResponse();
        }

        /// <summary>
        /// Loads the profile.
        /// </summary>
        internal void LoadProfile()
        {
            var userId = Request.GetUInteger();
            Request.GetBool();

            var habbo = Azure.GetHabboById(userId);
            if (habbo == null)
            {
                this.Session.SendNotif(Azure.GetLanguage().GetVar("user_not_found"));
                return;
            }
            var createTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(habbo.CreateDate);

            Response.Init(LibraryParser.OutgoingRequest("UserProfileMessageComposer"));
            Response.AppendInteger(habbo.Id);
            Response.AppendString(habbo.UserName);
            Response.AppendString(habbo.Look);
            Response.AppendString(habbo.Motto);
            Response.AppendString(createTime.ToString("dd/MM/yyyy"));
            Response.AppendInteger(habbo.AchievementPoints);
            Response.AppendInteger(GetFriendsCount(userId));
            Response.AppendBool(habbo.Id != Session.GetHabbo().Id &&
                                Session.GetHabbo().GetMessenger().FriendshipExists(habbo.Id));
            Response.AppendBool(habbo.Id != Session.GetHabbo().Id &&
                                !Session.GetHabbo().GetMessenger().FriendshipExists(habbo.Id) &&
                                Session.GetHabbo().GetMessenger().RequestExists(habbo.Id));
            Response.AppendBool(Azure.GetGame().GetClientManager().GetClientByUserId(habbo.Id) != null);
            var groups = Azure.GetGame().GetGroupManager().GetUserGroups(habbo.Id);
            Response.AppendInteger(groups.Count);
            foreach (var @group in groups.Select(groupUs => Azure.GetGame().GetGroupManager().GetGroup(groupUs.GroupId))
                )
                if (@group != null)
                {
                    Response.AppendInteger(@group.Id);
                    Response.AppendString(@group.Name);
                    Response.AppendString(@group.Badge);
                    Response.AppendString(Azure.GetGame().GetGroupManager().GetGroupColour(@group.Colour1, true));
                    Response.AppendString(Azure.GetGame().GetGroupManager().GetGroupColour(@group.Colour2, false));
                    Response.AppendBool(@group.Id == habbo.FavouriteGroup);
                    Response.AppendInteger(-1);
                    Response.AppendBool(@group.HasForum);
                }
                else
                {
                    Response.AppendInteger(1);
                    Response.AppendString("THIS GROUP IS INVALID");
                    Response.AppendString("");
                    Response.AppendString("");
                    Response.AppendString("");
                    Response.AppendBool(false);
                    Response.AppendInteger(-1);
                    Response.AppendBool(false);
                }

            if (Azure.GetGame().GetClientManager().GetClientByUserId(habbo.Id) == null)
                Response.AppendInteger((Azure.GetUnixTimeStamp() - habbo.PreviousOnline));
            else
                Response.AppendInteger((Azure.GetUnixTimeStamp() - habbo.LastOnline));

            Response.AppendBool(true);
            SendResponse();
            Response.Init(LibraryParser.OutgoingRequest("UserBadgesMessageComposer"));
            Response.AppendInteger(habbo.Id);
            Response.StartArray();

            foreach (
                var badge in habbo.GetBadgeComponent().BadgeList.Values.Cast<Badge>().Where(badge => badge.Slot > 0))
            {
                Response.AppendInteger(badge.Slot);
                Response.AppendString(badge.Code);

                Response.SaveArray();
            }

            Response.EndArray();
            SendResponse();
        }

        /// <summary>
        /// Changes the look.
        /// </summary>
        internal void ChangeLook()
        {
            var text = Request.GetString().ToUpper();
            var text2 = Request.GetString();
            text2 = Azure.FilterFigure(text2);

            Azure.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.ProfileChangeLook, 0u);
            Session.GetHabbo().Look = text2;
            Session.GetHabbo().Gender = text.ToLower();
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Format("UPDATE users SET look = @look, gender = @gender WHERE id = {0}",
                    Session.GetHabbo().Id));
                queryReactor.AddParameter("look", text2);
                queryReactor.AddParameter("gender", text);
                queryReactor.RunQuery();
            }
            Azure.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_AvatarLooks", 1, false);
            if (Session.GetHabbo().Look.Contains("ha-1006"))
                Azure.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.WearHat, 0u);
            Session.GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("UpdateAvatarAspectMessageComposer"));
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Look);
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Gender.ToUpper());
            Session.GetMessageHandler().SendResponse();
            Session.GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
            Session.GetMessageHandler().GetResponse().AppendInteger(-1);
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Look);
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Gender.ToLower());
            Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Motto);
            Session.GetMessageHandler().GetResponse().AppendInteger(Session.GetHabbo().AchievementPoints);
            Session.GetMessageHandler().SendResponse();
            if (!Session.GetHabbo().InRoom)
                return;
            var currentRoom = Session.GetHabbo().CurrentRoom;
            if (currentRoom == null)
                return;
            var roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
                return;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
            serverMessage.AppendInteger(roomUserByHabbo.VirtualId); //BUGG
            //serverMessage.AppendInt32(-1);
            serverMessage.AppendString(Session.GetHabbo().Look);
            serverMessage.AppendString(Session.GetHabbo().Gender.ToLower());
            serverMessage.AppendString(Session.GetHabbo().Motto);
            serverMessage.AppendInteger(Session.GetHabbo().AchievementPoints);
            currentRoom.SendMessage(serverMessage);

            if (Session.GetHabbo().GetMessenger() != null) Session.GetHabbo().GetMessenger().OnStatusChanged(true);
        }

        /// <summary>
        /// Changes the motto.
        /// </summary>
        internal void ChangeMotto()
        {
            var text = Request.GetString();
            if (text == Session.GetHabbo().Motto)
                return;
            Session.GetHabbo().Motto = text;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Format("UPDATE users SET motto = @motto WHERE id = '{0}'",
                    Session.GetHabbo().Id));
                queryReactor.AddParameter("motto", text);
                queryReactor.RunQuery();
            }
            Azure.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.ProfileChangeMotto, 0u);
            if (Session.GetHabbo().InRoom)
            {
                var currentRoom = Session.GetHabbo().CurrentRoom;
                if (currentRoom == null)
                    return;
                var roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (roomUserByHabbo == null)
                    return;
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                serverMessage.AppendInteger(roomUserByHabbo.VirtualId); //BUGG
                //serverMessage.AppendInt32(-1);
                serverMessage.AppendString(Session.GetHabbo().Look);
                serverMessage.AppendString(Session.GetHabbo().Gender.ToLower());
                serverMessage.AppendString(Session.GetHabbo().Motto);
                serverMessage.AppendInteger(Session.GetHabbo().AchievementPoints);
                currentRoom.SendMessage(serverMessage);
            }
            Azure.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_Motto", 1, false);
        }

        /// <summary>
        /// Gets the wardrobe.
        /// </summary>
        internal void GetWardrobe()
        {
            GetResponse().Init(LibraryParser.OutgoingRequest("LoadWardrobeMessageComposer"));
            GetResponse().AppendInteger(0);
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    string.Format("SELECT slot_id, look, gender FROM users_wardrobe WHERE user_id = {0}",
                        Session.GetHabbo().Id));
                var table = queryReactor.GetTable();
                if (table == null)
                    GetResponse().AppendInteger(0);
                else
                {
                    GetResponse().AppendInteger(table.Rows.Count);
                    foreach (DataRow dataRow in table.Rows)
                    {
                        GetResponse().AppendInteger(Convert.ToUInt32(dataRow["slot_id"]));
                        GetResponse().AppendString((string)dataRow["look"]);
                        GetResponse().AppendString(dataRow["gender"].ToString().ToUpper());
                    }
                }
                SendResponse();
            }
        }

        /// <summary>
        /// Saves the wardrobe.
        /// </summary>
        internal void SaveWardrobe()
        {
            var num = Request.GetUInteger();
            var text = Request.GetString();
            var text2 = Request.GetString();

            text = Azure.FilterFigure(text);

            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Concat(new object[]
                {
                    "SELECT null FROM users_wardrobe WHERE user_id = ",
                    Session.GetHabbo().Id,
                    " AND slot_id = ",
                    num
                }));
                queryReactor.AddParameter("look", text);
                queryReactor.AddParameter("gender", text2.ToUpper());
                if (queryReactor.GetRow() != null)
                {
                    queryReactor.SetQuery(string.Concat(new object[]
                    {
                        "UPDATE users_wardrobe SET look = @look, gender = @gender WHERE user_id = ",
                        Session.GetHabbo().Id,
                        " AND slot_id = ",
                        num,
                        ";"
                    }));
                    queryReactor.AddParameter("look", text);
                    queryReactor.AddParameter("gender", text2.ToUpper());
                    queryReactor.RunQuery();
                }
                else
                {
                    queryReactor.SetQuery(string.Concat(new object[]
                    {
                        "INSERT INTO users_wardrobe (user_id,slot_id,look,gender) VALUES (",
                        Session.GetHabbo().Id,
                        ",",
                        num,
                        ",@look,@gender)"
                    }));
                    queryReactor.AddParameter("look", text);
                    queryReactor.AddParameter("gender", text2.ToUpper());
                    queryReactor.RunQuery();
                }
            }
            Azure.GetGame()
                .GetQuestManager()
                .ProgressUserQuest(Session, QuestType.ProfileChangeLook);
        }

        /// <summary>
        /// Gets the pets inventory.
        /// </summary>
        internal void GetPetsInventory()
        {
            if (Session.GetHabbo().GetInventoryComponent() == null)
                return;
            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
        }

        /// <summary>
        /// Gets the bots inventory.
        /// </summary>
        internal void GetBotsInventory()
        {
            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializeBotInventory());
            SendResponse();
        }

        /// <summary>
        /// Checks the name.
        /// </summary>
        internal void CheckName()
        {
            var text = Request.GetString();
            if (text.ToLower() == Session.GetHabbo().UserName.ToLower())
            {
                Response.Init(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                Response.AppendInteger(0);
                Response.AppendString(text);
                Response.AppendInteger(0);
                SendResponse();
                return;
            }
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT username FROM users WHERE Username=@name LIMIT 1");
                queryReactor.AddParameter("name", text);
                var @string = queryReactor.GetString();
                var array = text.ToLower().ToCharArray();
                const string source = "abcdefghijklmnopqrstuvwxyz1234567890.,_-;:?!@áéíóúÁÉÍÓÚñÑÜüÝý ";
                var array2 = array;
                if (array2.Any(c => !source.Contains(char.ToLower(c))))
                {
                    Response.Init(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    Response.AppendInteger(4);
                    Response.AppendString(text);
                    Response.AppendInteger(0);
                    SendResponse();
                    return;
                }
                if (text.ToLower().Contains("mod") || text.ToLower().Contains("m0d") || text.Contains(" ") ||
                    text.ToLower().Contains("admin"))
                {
                    Response.Init(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    Response.AppendInteger(4);
                    Response.AppendString(text);
                    Response.AppendInteger(0);
                    SendResponse();
                }
                else if (text.Length > 15)
                {
                    Response.Init(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    Response.AppendInteger(3);
                    Response.AppendString(text);
                    Response.AppendInteger(0);
                    SendResponse();
                }
                else if (text.Length < 3)
                {
                    Response.Init(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    Response.AppendInteger(2);
                    Response.AppendString(text);
                    Response.AppendInteger(0);
                    SendResponse();
                }
                else if (string.IsNullOrWhiteSpace(@string))
                {
                    Response.Init(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    Response.AppendInteger(0);
                    Response.AppendString(text);
                    Response.AppendInteger(0);
                    SendResponse();
                }
                else
                {
                    queryReactor.SetQuery("SELECT tag FROM users_tags ORDER BY RAND() LIMIT 3");
                    var table = queryReactor.GetTable();
                    Response.Init(LibraryParser.OutgoingRequest("NameChangedUpdatesMessageComposer"));
                    Response.AppendInteger(5);
                    Response.AppendString(text);
                    Response.AppendInteger(table.Rows.Count);
                    foreach (DataRow dataRow in table.Rows)
                        Response.AppendString(string.Format("{0}{1}", text, dataRow[0]));
                    SendResponse();
                }
            }
        }

        /// <summary>
        /// Changes the name.
        /// </summary>
        internal void ChangeName()
        {
            var text = Request.GetString();
            var userName = Session.GetHabbo().UserName;

            {
                using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery("SELECT username FROM users WHERE Username=@name LIMIT 1");
                    queryReactor.AddParameter("name", text);
                    var @String = queryReactor.GetString();

                    if (!string.IsNullOrWhiteSpace(String) &&
                        !String.Equals(userName, text, StringComparison.CurrentCultureIgnoreCase))
                        return;
                    queryReactor.SetQuery("UPDATE rooms_data SET owner = @newowner WHERE owner = @oldowner");
                    queryReactor.AddParameter("newowner", text);
                    queryReactor.AddParameter("oldowner", Session.GetHabbo().UserName);
                    queryReactor.RunQuery();

                    queryReactor.SetQuery(
                        "UPDATE users SET Username = @newname, last_name_change = @timestamp WHERE id = @userid");
                    queryReactor.AddParameter("newname", text);
                    queryReactor.AddParameter("timestamp", Azure.GetUnixTimeStamp() + 43200);
                    queryReactor.AddParameter("userid", Session.GetHabbo().UserName);
                    queryReactor.RunQuery();

                    Session.GetHabbo().LastChange = Azure.GetUnixTimeStamp() + 43200;
                    Session.GetHabbo().UserName = text;
                    Response.Init(LibraryParser.OutgoingRequest("UpdateUsernameMessageComposer"));
                    Response.AppendInteger(0);
                    Response.AppendString(text);
                    Response.AppendInteger(0);
                    SendResponse();
                    Response.Init(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                    Response.AppendInteger(-1);
                    Response.AppendString(Session.GetHabbo().Look);
                    Response.AppendString(Session.GetHabbo().Gender.ToLower());
                    Response.AppendString(Session.GetHabbo().Motto);
                    Response.AppendInteger(Session.GetHabbo().AchievementPoints);
                    SendResponse();
                    Session.GetHabbo().CurrentRoom.GetRoomUserManager().UpdateUser(userName, text);
                    if (Session.GetHabbo().CurrentRoom != null)
                    {
                        Response.Init(LibraryParser.OutgoingRequest("UserUpdateNameInRoomMessageComposer"));
                        Response.AppendInteger(Session.GetHabbo().Id);
                        Response.AppendInteger(Session.GetHabbo().CurrentRoom.RoomId);
                        Response.AppendString(text);
                    }
                    foreach (var current in Session.GetHabbo().UsersRooms)
                    {
                        current.Owner = text;
                        current.SerializeRoomData(Response, Session, false, true);
                        var room = Azure.GetGame().GetRoomManager().GetRoom(current.Id);
                        if (room != null)
                            room.RoomData.Owner = text;
                    }
                    foreach (var current2 in Session.GetHabbo().GetMessenger().Friends.Values)
                        if (current2.Client != null)
                            foreach (
                                var current3 in
                                    current2.Client.GetHabbo()
                                        .GetMessenger()
                                        .Friends.Values.Where(current3 => current3.UserName == userName))
                            {
                                current3.UserName = text;
                                current3.Serialize(Response, Session);
                            }
                }
            }
        }

        /// <summary>
        /// Gets the relationships.
        /// </summary>
        internal void GetRelationships()
        {
            var userId = Request.GetUInteger();
            var habboForId = Azure.GetHabboById(userId);
            if (habboForId == null)
                return;
            var rand = new Random();
            habboForId.Relationships = (
                from x in habboForId.Relationships
                orderby rand.Next()
                select x).ToDictionary(item => item.Key,
                    item => item.Value);
            var num = habboForId.Relationships.Count(x => x.Value.Type == 1);
            var num2 = habboForId.Relationships.Count(x => x.Value.Type == 2);
            var num3 = habboForId.Relationships.Count(x => x.Value.Type == 3);
            Response.Init(LibraryParser.OutgoingRequest("RelationshipMessageComposer"));
            Response.AppendInteger(habboForId.Id);
            Response.AppendInteger(habboForId.Relationships.Count);
            foreach (var current in habboForId.Relationships.Values)
            {
                var habboForId2 = Azure.GetHabboById(Convert.ToUInt32(current.UserId));
                if (habboForId2 == null)
                {
                    Response.AppendInteger(0);
                    Response.AppendInteger(0);
                    Response.AppendInteger(0);
                    Response.AppendString("Placeholder");
                    Response.AppendString("hr-115-42.hd-190-1.ch-215-62.lg-285-91.sh-290-62");
                }
                else
                {
                    Response.AppendInteger(current.Type);
                    Response.AppendInteger((current.Type == 1) ? num : ((current.Type == 2) ? num2 : num3));
                    Response.AppendInteger(current.UserId);
                    Response.AppendString(habboForId2.UserName);
                    Response.AppendString(habboForId2.Look);
                }
            }
            SendResponse();
        }

        /// <summary>
        /// Sets the relationship.
        /// </summary>
        internal void SetRelationship()
        {
            var num = Request.GetUInteger();
            var num2 = Request.GetInteger();

            {
                using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    if (num2 == 0)
                    {
                        queryReactor.SetQuery(
                            "SELECT id FROM users_relationships WHERE user_id=@id AND target=@target LIMIT 1");
                        queryReactor.AddParameter("id", Session.GetHabbo().Id);
                        queryReactor.AddParameter("target", num);
                        var integer = queryReactor.GetInteger();
                        queryReactor.SetQuery(
                            "DELETE FROM users_relationships WHERE user_id=@id AND target=@target LIMIT 1");
                        queryReactor.AddParameter("id", Session.GetHabbo().Id);
                        queryReactor.AddParameter("target", num);
                        queryReactor.RunQuery();
                        if (Session.GetHabbo().Relationships.ContainsKey(integer))
                            Session.GetHabbo().Relationships.Remove(integer);
                    }
                    else
                    {
                        queryReactor.SetQuery(
                            "SELECT id FROM users_relationships WHERE user_id=@id AND target=@target LIMIT 1");
                        queryReactor.AddParameter("id", Session.GetHabbo().Id);
                        queryReactor.AddParameter("target", num);
                        var integer2 = queryReactor.GetInteger();
                        if (integer2 > 0)
                        {
                            queryReactor.SetQuery(
                                "DELETE FROM users_relationships WHERE user_id=@id AND target=@target LIMIT 1");
                            queryReactor.AddParameter("id", Session.GetHabbo().Id);
                            queryReactor.AddParameter("target", num);
                            queryReactor.RunQuery();
                            if (Session.GetHabbo().Relationships.ContainsKey(integer2))
                                Session.GetHabbo().Relationships.Remove(integer2);
                        }
                        queryReactor.SetQuery(
                            "INSERT INTO users_relationships (user_id, target, type) VALUES (@id, @target, @type)");
                        queryReactor.AddParameter("id", Session.GetHabbo().Id);
                        queryReactor.AddParameter("target", num);
                        queryReactor.AddParameter("type", num2);
                        var num3 = (int)queryReactor.InsertQuery();
                        Session.GetHabbo().Relationships.Add(num3, new Relationship(num3, (int)num, num2));
                    }
                    var clientByUserId = Azure.GetGame().GetClientManager().GetClientByUserId(num);
                    Session.GetHabbo().GetMessenger().UpdateFriend(num, clientByUserId, true);
                }
            }
        }

        /// <summary>
        /// Starts the quest.
        /// </summary>
        public void StartQuest()
        {
            Azure.GetGame().GetQuestManager().ActivateQuest(Session, Request);
        }

        /// <summary>
        /// Stops the quest.
        /// </summary>
        public void StopQuest()
        {
            Azure.GetGame().GetQuestManager().CancelQuest(Session, Request);
        }

        /// <summary>
        /// Gets the current quest.
        /// </summary>
        public void GetCurrentQuest()
        {
            Azure.GetGame().GetQuestManager().GetCurrentQuest(Session, Request);
        }

        /// <summary>
        /// Starts the seasonal quest.
        /// </summary>
        public void StartSeasonalQuest()
        {
            RoomData roomData;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                var quest = Azure.GetGame().GetQuestManager().GetQuest(Request.GetUInteger());
                if (quest == null)
                    return;
                queryReactor.RunFastQuery(string.Concat(new object[]
                {
                    "REPLACE INTO users_quests_data(user_id,quest_id) VALUES (",
                    Session.GetHabbo().Id,
                    ", ",
                    quest.Id,
                    ")"
                }));
                queryReactor.RunFastQuery(string.Concat(new object[]
                {
                    "UPDATE users_stats SET quest_id = ",
                    quest.Id,
                    " WHERE id = ",
                    Session.GetHabbo().Id
                }));
                Session.GetHabbo().CurrentQuestId = quest.Id;
                Session.SendMessage(QuestStartedComposer.Compose(Session, quest));
                Azure.GetGame().GetQuestManager().ActivateQuest(Session, Request);
                queryReactor.SetQuery("SELECT id FROM rooms_data WHERE state='open' ORDER BY users_now DESC LIMIT 1");
                var @string = queryReactor.GetString();
                roomData = Azure.GetGame().GetRoomManager().GenerateRoomData(uint.Parse(@string));
            }
            if (roomData != null)
            {
                roomData.SerializeRoomData(Response, Session, true, false);
                Session.GetMessageHandler().PrepareRoomForUser(roomData.Id, "");
                return;
            }
            this.Session.SendNotif(Azure.GetLanguage().GetVar("start_quest_need_room"));
        }

        /// <summary>
        /// Receives the nux gifts.
        /// </summary>
        public void ReceiveNuxGifts()
        {
            if (!ExtraSettings.NEW_users_gifts_ENABLED)
            {
                this.Session.SendNotif(Azure.GetLanguage().GetVar("nieuwe_gebruiker_kado_error_1"));
                return;
            }
            if (Session.GetHabbo().NuxPassed)
            {
                this.Session.SendNotif(Azure.GetLanguage().GetVar("nieuwe_gebruiker_kado_error_2"));
                return;
            }

            var item = Session.GetHabbo().GetInventoryComponent().AddNewItem(0, ExtraSettings.NEW_USER_GIFT_YTTV2_ID, "", 0, true, false, 0, 0);
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);

            Session.GetHabbo().BelCredits += 25;
            Session.GetHabbo().UpdateSeasonalCurrencyBalance();
            if (item != null)
                Session.GetHabbo().GetInventoryComponent().SendNewItems(item.Id);

            using (var dbClient = Azure.GetDatabaseManager().GetQueryReactor())
                if (Session.GetHabbo().VIP)
                    dbClient.RunFastQuery(
                        string.Format(
                            "UPDATE users SET vip = '1', vip_expire = DATE_ADD(vip_expire, INTERVAL 1 DAY), nux_passed = '1' WHERE id = {0}",
                            Session.GetHabbo().Id));
                else
                    dbClient.RunFastQuery(
                        string.Format(
                            "UPDATE users SET vip = '1', vip_expire = DATE_ADD(NOW(), INTERVAL 1 DAY), nux_passed = '1' WHERE id = {0}",
                            Session.GetHabbo().Id));

            Session.GetHabbo().NuxPassed = true;
            Session.GetHabbo().VIP = true;
        }

        /// <summary>
        /// Accepts the nux gifts.
        /// </summary>
        public void AcceptNuxGifts()
        {
            if (ExtraSettings.NEW_users_gifts_ENABLED == false || Request.GetInteger() != 0)
                return;

            var nuxGifts = new ServerMessage(LibraryParser.OutgoingRequest("NuxListGiftsMessageComposer"));
            nuxGifts.AppendInteger(3); //Cantidad

            nuxGifts.AppendInteger(0);
            nuxGifts.AppendInteger(0);
            nuxGifts.AppendInteger(1); //Cantidad
            // ahora nuevo bucle
            nuxGifts.AppendString("");
            nuxGifts.AppendString("nux/gift_yttv2.png");
            nuxGifts.AppendInteger(1); //cantidad
            //Ahora nuevo bucle...
            nuxGifts.AppendString("yttv2");
            nuxGifts.AppendString("");

            nuxGifts.AppendInteger(2);
            nuxGifts.AppendInteger(1);
            nuxGifts.AppendInteger(1);
            nuxGifts.AppendString("");
            nuxGifts.AppendString("nux/gift_diamonds.png");
            nuxGifts.AppendInteger(1);
            nuxGifts.AppendString("nux_gift_diamonds");
            nuxGifts.AppendString("");

            nuxGifts.AppendInteger(3);
            nuxGifts.AppendInteger(1);
            nuxGifts.AppendInteger(1);
            nuxGifts.AppendString("");
            nuxGifts.AppendString("nux/gift_vip1day.png");
            nuxGifts.AppendInteger(1);
            nuxGifts.AppendString("nux_gift_vip_1_day");
            nuxGifts.AppendString("");

            Session.SendMessage(nuxGifts);
        }

        /// <summary>
        /// Talentses this instance.
        /// </summary>
        /// <exception cref="System.NullReferenceException"></exception>
        internal void Talents()
        {
            var trackType = Request.GetString();
            var talents = Azure.GetGame().GetTalentManager().GetTalents(trackType, -1);
            var failLevel = -1;
            if (talents == null)
                return;
            Response.Init(LibraryParser.OutgoingRequest("TalentsTrackMessageComposer"));
            Response.AppendString(trackType);
            Response.AppendInteger(talents.Count);
            foreach (var current in talents)
            {
                Response.AppendInteger(current.Level);
                var nm = (failLevel == -1) ? 1 : 0;
                Response.AppendInteger(nm);
                var talents2 = Azure.GetGame().GetTalentManager().GetTalents(trackType, current.Id);
                Response.AppendInteger(talents2.Count);
                foreach (var current2 in talents2)
                {
                    if (current2.GetAchievement() == null)
                        throw new NullReferenceException(
                            string.Format("The following talent achievement can't be found: {0}",
                                current2.AchievementGroup));

                    var num = (failLevel != -1 && failLevel < current2.Level)
                        ? 0
                        : (Session.GetHabbo().GetAchievementData(current2.AchievementGroup) == null)
                            ? 1
                            : (Session.GetHabbo().GetAchievementData(current2.AchievementGroup).Level >=
                               current2.AchievementLevel)
                                ? 2
                                : 1;
                    Response.AppendInteger(current2.GetAchievement().Id);
                    Response.AppendInteger(0);
                    Response.AppendString(string.Format("{0}{1}", current2.AchievementGroup, current2.AchievementLevel));
                    Response.AppendInteger(num);
                    Response.AppendInteger((Session.GetHabbo().GetAchievementData(current2.AchievementGroup) != null)
                        ? Session.GetHabbo().GetAchievementData(current2.AchievementGroup).Progress
                        : 0);
                    Response.AppendInteger((current2.GetAchievement() == null)
                        ? 0
                        : current2.GetAchievement().Levels[current2.AchievementLevel].Requirement);
                    if (num != 2 && failLevel == -1)
                        failLevel = current2.Level;
                }
                Response.AppendInteger(0);
                if (current.Type == "citizenship" && current.Level == 4)
                {
                    Response.AppendInteger(2);
                    Response.AppendString("HABBO_CLUB_VIP_7_DAYS");
                    Response.AppendInteger(7);
                    Response.AppendString(current.Prize);
                    Response.AppendInteger(0);
                }
                else
                {
                    Response.AppendInteger(1);
                    Response.AppendString(current.Prize);
                    Response.AppendInteger(0);
                }
            }
            SendResponse();
        }

        /// <summary>
        /// Completes the safety quiz.
        /// </summary>
        internal void CompleteSafetyQuiz()
        {
            Azure.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_SafetyQuizGraduate", 1, false);
        }

        /// <summary>
        /// Hotels the view countdown.
        /// </summary>
        internal void HotelViewCountdown()
        {
            string time = Request.GetString();
            DateTime date;
            DateTime.TryParse(time, out date);
            TimeSpan diff = date - DateTime.Now;
            Response.Init(LibraryParser.OutgoingRequest("HotelViewCountdownMessageComposer"));
            Response.AppendString(time);
            Response.AppendInteger(Convert.ToInt32(diff.TotalSeconds));
            SendResponse();
            Console.WriteLine(diff.TotalSeconds);
        }

        /// <summary>
        /// Hotels the view dailyquest.
        /// </summary>
        internal void HotelViewDailyquest()
        {
        }

        internal void FindMoreFriends()
        {
            var allRooms = Azure.GetGame().GetRoomManager().GetActiveRooms();
            Random rnd = new Random();
            var randomRoom = allRooms[rnd.Next(allRooms.Length)].Key;
            var success = new ServerMessage(LibraryParser.OutgoingRequest("FindMoreFriendsSuccessMessageComposer"));
            if (randomRoom == null)
            {
                success.AppendBool(false);
                Session.SendMessage(success);
                return;
            }
            success.AppendBool(true);
            Session.SendMessage(success);
            var roomFwd = new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
            roomFwd.AppendInteger(randomRoom.Id);
            Session.SendMessage(roomFwd);
        }
        internal void HotelViewRequestBadge()
        {
            string name = Request.GetString();
            var hotelViewBadges = Azure.GetGame().GetHotelView().HotelViewBadges;
            if (!hotelViewBadges.ContainsKey(name))
                return;
            var badge = hotelViewBadges[name];
            Session.GetHabbo().GetBadgeComponent().GiveBadge(badge, true, Session, true);
        }
        internal void GetCameraPrice()
        {
            GetResponse().Init(LibraryParser.OutgoingRequest("SetCameraPriceMessageComposer"));
            GetResponse().AppendInteger(0);//credits
            GetResponse().AppendInteger(10);//duckets
            SendResponse();
        }
        internal void GetHotelViewHallOfFame()
        {
            string code = Request.GetString();
            GetResponse().Init(LibraryParser.OutgoingRequest("HotelViewHallOfFameMessageComposer"));
            GetResponse().AppendString(code);
            var Rankings = Azure.GetGame().GetHallOfFame().Rankings.Where(e => e.Competition == code);
            GetResponse().StartArray();
            int rank = 1;
            foreach (HallOfFameElement element in Rankings)
            {
                Habbo user = Azure.GetHabboById(element.UserId);
                if (user == null) continue;
                GetResponse().AppendInteger(user.Id);
                GetResponse().AppendString(user.UserName);
                GetResponse().AppendString(user.Look);
                GetResponse().AppendInteger(rank);
                GetResponse().AppendInteger(element.Score);
                rank++;
                GetResponse().SaveArray();
            }
            GetResponse().EndArray();
            SendResponse();
        }
        internal void FriendRequestListLoad()
        {
        }
    }
}