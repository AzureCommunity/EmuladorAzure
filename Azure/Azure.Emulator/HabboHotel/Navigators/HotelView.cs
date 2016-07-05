using Azure.Messages;
using System;
using System.Collections.Generic;
using System.Data;

namespace Azure.HabboHotel.Navigators
{
    /// <summary>
    /// Class HotelView.
    /// </summary>
    public class HotelView
    {
        /// <summary>
        /// The hotel view promos indexers
        /// </summary>
        internal List<SmallPromo> HotelViewPromosIndexers = new List<SmallPromo>();
        internal Dictionary<string, string> HotelViewBadges = new Dictionary<string, string>();
        /// <summary>
        /// The furni reward name
        /// </summary>
        internal string FurniRewardName;

        /// <summary>
        /// The furni reward identifier
        /// </summary>
        internal int FurniRewardId;

        /// <summary>
        /// Initializes a new instance of the <see cref="HotelView"/> class.
        /// </summary>
        public HotelView()
        {
            List();
            LoadReward();
            LoadHVBadges();
        }

        /// <summary>
        /// Loads the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>SmallPromo.</returns>
        public static SmallPromo Load(int index)
        {
            DataRow row;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    "SELECT hotelview_promos.`index`,hotelview_promos.header,hotelview_promos.body,hotelview_promos.button,hotelview_promos.in_game_promo,hotelview_promos.special_action,hotelview_promos.image,hotelview_promos.enabled FROM hotelview_promos WHERE hotelview_promos.`index` = @x LIMIT 1");
                queryReactor.AddParameter("x", index);
                row = queryReactor.GetRow();
            }
            return new SmallPromo(index, (string)row[1], (string)row[2], (string)row[3], Convert.ToInt32(row[4]),
                (string)row[5], (string)row[6]);
        }

        /// <summary>
        /// Refreshes the promo list.
        /// </summary>
        public void RefreshPromoList()
        {
            HotelViewPromosIndexers.Clear();
            List();
            LoadReward();
            HotelViewBadges.Clear();
            LoadHVBadges();
        }

        /// <summary>
        /// Smalls the promo composer.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SmallPromoComposer(ServerMessage message)
        {
            message.AppendInteger(HotelViewPromosIndexers.Count);
            foreach (var current in HotelViewPromosIndexers)
                current.Serialize(message);
            return message;
        }

        /// <summary>
        /// Loads the reward.
        /// </summary>
        private void LoadReward()
        {
            DataRow row;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    "SELECT hotelview_rewards_promos.furni_id, hotelview_rewards_promos.furni_name FROM hotelview_rewards_promos WHERE hotelview_rewards_promos.enabled = 1 LIMIT 1");
                row = queryReactor.GetRow();
            }
            if (row == null)
                return;
            FurniRewardId = Convert.ToInt32(row[0]);
            FurniRewardName = Convert.ToString(row[1]);
        }

        /// <summary>
        /// Lists this instance.
        /// </summary>
        private void List()
        {
            DataTable table;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    "SELECT * from hotelview_promos WHERE hotelview_promos.enabled = '1' ORDER BY hotelview_promos.`index` DESC");
                table = queryReactor.GetTable();
            }
            foreach (DataRow dataRow in table.Rows)
                HotelViewPromosIndexers.Add(new SmallPromo(Convert.ToInt32(dataRow[0]), (string)dataRow[1],
                    (string)dataRow[2], (string)dataRow[3], Convert.ToInt32(dataRow[4]), (string)dataRow[5],
                    (string)dataRow[6]));
        }
        private void LoadHVBadges()
        {
            DataTable table;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT * FROM hotelview_badges WHERE enabled = '1'");
                table = queryReactor.GetTable();
            }
            foreach (DataRow dataRow in table.Rows)
                HotelViewBadges.Add((string)dataRow[0], (string)dataRow[1]);
        }
    }
}