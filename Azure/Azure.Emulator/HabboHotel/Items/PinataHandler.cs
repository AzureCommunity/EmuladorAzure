using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.HabboHotel.Rooms;
using System;
using System.Collections.Generic;
using System.Data;

namespace Azure.HabboHotel.Items
{
    /// <summary>
    /// Class PinataHandler.
    /// </summary>
    internal class PinataHandler
    {
        /// <summary>
        /// The pinatas
        /// </summary>
        internal Dictionary<uint, PinataItem> Pinatas;

        /// <summary>
        /// The _table
        /// </summary>
        private DataTable _table;

        /// <summary>
        /// Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void Initialize(IQueryAdapter dbClient)
        {
            dbClient.SetQuery(query: "SELECT * FROM items_pinatas");
            Pinatas = new Dictionary<uint, PinataItem>();
            _table = dbClient.GetTable();
            foreach (DataRow dataRow in _table.Rows)
            {
                var value = new PinataItem(dataRow);
                Pinatas.Add(uint.Parse(dataRow["item_baseid"].ToString()), value);
            }
        }

        /// <summary>
        /// Delivers the random pinata item.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="room">The room.</param>
        /// <param name="item">The item.</param>
        internal void DeliverRandomPinataItem(RoomUser user, Room room, RoomItem item)
        {
            if (room == null || item == null || item.GetBaseItem().InteractionType != Interaction.Pinata || !Pinatas.ContainsKey(item.GetBaseItem().ItemId))
                return;

            PinataItem pinataItem;
            Pinatas.TryGetValue(item.GetBaseItem().ItemId, out pinataItem);

            if (pinataItem == null || pinataItem.Rewards.Count < 1)
                return;

            item.RefreshItem();
            item.BaseItem = pinataItem.Rewards[new Random().Next((pinataItem.Rewards.Count - 1))];

            item.ExtraData = string.Empty;
            room.GetRoomItemHandler().RemoveFurniture(user.GetClient(), item.Id, wasPicked: false);
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Format(format: "UPDATE items_rooms SET base_item='{0}', extra_data='' WHERE id='{1}'", arg0: item.BaseItem, arg1: item.Id));
                queryReactor.RunQuery();
            }

            if (!room.GetRoomItemHandler().SetFloorItem(user.GetClient(), item, item.X, item.Y, newRot: 0, newItem: true, onRoller: false, sendMessage: true))
                user.GetClient().GetHabbo().GetInventoryComponent().AddItem(item);
        }
    }
}