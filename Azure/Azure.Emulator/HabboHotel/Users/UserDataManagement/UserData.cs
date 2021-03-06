using Azure.HabboHotel.Achievements;
using Azure.HabboHotel.Items;
using Azure.HabboHotel.Pets;
using Azure.HabboHotel.RoomBots;
using Azure.HabboHotel.Rooms;
using Azure.HabboHotel.Users.Badges;
using Azure.HabboHotel.Users.Inventory;
using Azure.HabboHotel.Users.Messenger;
using Azure.HabboHotel.Users.Relationships;
using Azure.HabboHotel.Users.Subscriptions;
using System.Collections.Generic;

namespace Azure.HabboHotel.Users.UserDataManagement
{
    /// <summary>
    /// Class UserData.
    /// </summary>
    internal class UserData
    {
        /// <summary>
        /// The user identifier
        /// </summary>
        internal uint UserId;

        /// <summary>
        /// The achievements
        /// </summary>
        internal Dictionary<string, UserAchievement> Achievements;

        /// <summary>
        /// The talents
        /// </summary>
        internal Dictionary<int, UserTalent> Talents;

        /// <summary>
        /// The favourited rooms
        /// </summary>
        internal List<uint> FavouritedRooms;

        /// <summary>
        /// The ignores
        /// </summary>
        internal List<uint> Ignores;

        /// <summary>
        /// The tags
        /// </summary>
        internal List<string> Tags;

        /// <summary>
        /// The subscriptions
        /// </summary>
        internal Subscription Subscriptions;

        /// <summary>
        /// The badges
        /// </summary>
        internal List<Badge> Badges;

        /// <summary>
        /// The inventory
        /// </summary>
        internal List<UserItem> Inventory;

        /// <summary>
        /// The effects
        /// </summary>
        internal List<AvatarEffect> Effects;

        /// <summary>
        /// The friends
        /// </summary>
        internal Dictionary<uint, MessengerBuddy> Friends;

        /// <summary>
        /// The requests
        /// </summary>
        internal Dictionary<uint, MessengerRequest> Requests;

        /// <summary>
        /// The rooms
        /// </summary>
        internal HashSet<RoomData> Rooms;

        /// <summary>
        /// The pets
        /// </summary>
        internal Dictionary<uint, Pet> Pets;

        /// <summary>
        /// The quests
        /// </summary>
        internal Dictionary<uint, int> Quests;

        /// <summary>
        /// The user
        /// </summary>
        internal Habbo User;

        /// <summary>
        /// The bots
        /// </summary>
        internal Dictionary<uint, RoomBot> Bots;

        /// <summary>
        /// The relations
        /// </summary>
        internal Dictionary<int, Relationship> Relations;

        /// <summary>
        /// The suggested polls
        /// </summary>
        internal HashSet<uint> SuggestedPolls;

        /// <summary>
        /// The mini mail count
        /// </summary>
        internal uint MiniMailCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserData"/> class.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="achievements">The achievements.</param>
        /// <param name="talents">The talents.</param>
        /// <param name="favouritedRooms">The favourited rooms.</param>
        /// <param name="ignores">The ignores.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="sub">The sub.</param>
        /// <param name="badges">The badges.</param>
        /// <param name="inventory">The inventory.</param>
        /// <param name="effects">The effects.</param>
        /// <param name="friends">The friends.</param>
        /// <param name="requests">The requests.</param>
        /// <param name="rooms">The rooms.</param>
        /// <param name="pets">The pets.</param>
        /// <param name="quests">The quests.</param>
        /// <param name="user">The user.</param>
        /// <param name="bots">The bots.</param>
        /// <param name="relations">The relations.</param>
        /// <param name="suggestedPolls">The suggested polls.</param>
        /// <param name="miniMailCount">The mini mail count.</param>
        public UserData(uint userId, Dictionary<string, UserAchievement> achievements,
            Dictionary<int, UserTalent> talents, List<uint> favouritedRooms, List<uint> ignores, List<string> tags,
            Subscription sub, List<Badge> badges, List<UserItem> inventory, List<AvatarEffect> effects,
            Dictionary<uint, MessengerBuddy> friends, Dictionary<uint, MessengerRequest> requests,
            HashSet<RoomData> rooms, Dictionary<uint, Pet> pets, Dictionary<uint, int> quests, Habbo user,
            Dictionary<uint, RoomBot> bots, Dictionary<int, Relationship> relations, HashSet<uint> suggestedPolls,
            uint miniMailCount)
        {
            UserId = userId;
            Achievements = achievements;
            Talents = talents;
            FavouritedRooms = favouritedRooms;
            Ignores = ignores;
            Tags = tags;
            Subscriptions = sub;
            Badges = badges;
            Inventory = inventory;
            Effects = effects;
            Friends = friends;
            Requests = requests;
            Rooms = rooms;
            Pets = pets;
            Quests = quests;
            User = user;
            Bots = bots;
            Relations = relations;
            SuggestedPolls = suggestedPolls;
            MiniMailCount = miniMailCount;
        }
    }
}