using Azure.Messages;
using System.Collections.Generic;
using System.Data;

namespace Azure.HabboHotel.Users.Inventory
{
    /// <summary>
    /// Class UserClothing.
    /// </summary>
    internal class UserClothing
    {
        /// <summary>
        /// The _user identifier
        /// </summary>
        private readonly uint _userId;

        /// <summary>
        /// The clothing
        /// </summary>
        internal List<string> Clothing;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserClothing"/> class.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal UserClothing(uint userId)
        {
            _userId = userId;
            Clothing = new List<string>();
            DataTable dTable;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT clothing FROM users_clothing WHERE userid = @userid");
                queryReactor.AddParameter("userid", _userId);
                dTable = queryReactor.GetTable();
            }
            foreach (DataRow dRow in dTable.Rows)
                Clothing.Add((string)dRow["clothing"]);
        }

        /// <summary>
        /// Adds the specified clothing.
        /// </summary>
        /// <param name="clothing">The clothing.</param>
        internal void Add(string clothing)
        {
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("INSERT INTO users_clothing (userid,clothing) VALUES (@userid,@clothing)");
                queryReactor.AddParameter("userid", _userId);
                queryReactor.AddParameter("clothing", clothing);
                queryReactor.RunQuery();
            }
            Clothing.Add(clothing);
        }

        /// <summary>
        /// Serializes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void Serialize(ServerMessage message)
        {
            message.StartArray();
            foreach (var clothing1 in Clothing)
            {
                var item1 = Azure.GetGame().GetClothingManager().GetClothesInFurni(clothing1);
                foreach (var clothe in item1.Clothes)
                {
                    message.AppendInteger(clothe);
                }
                message.SaveArray();
            }
            message.EndArray();
            message.StartArray();
            foreach (var clothing2 in Clothing)
            {
                var item2 = Azure.GetGame().GetClothingManager().GetClothesInFurni(clothing2);
                foreach (var clothe in item2.Clothes)
                {
                    message.AppendString(item2.ItemName);
                }
                message.SaveArray();
            }
            message.EndArray();
        }
    }
}