using Azure.Configuration;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Azure.HabboHotel.Roles
{
    /// <summary>
    /// Class RoleManager.
    /// </summary>
    internal class RoleManager
    {
        /// <summary>
        /// The _rights
        /// </summary>
        private readonly Dictionary<string, uint> _rights;

        /// <summary>
        /// The _sub rights
        /// </summary>
        private readonly Dictionary<string, int> _subRights;

        /// <summary>
        /// The _CMD rights
        /// </summary>
        private readonly Dictionary<string, string> _cmdRights;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleManager"/> class.
        /// </summary>
        internal RoleManager()
        {
            this._rights = new Dictionary<string, uint>();
            this._subRights = new Dictionary<string, int>();
            this._cmdRights = new Dictionary<string, string>();
        }

        /// <summary>
        /// Loads the rights.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void LoadRights(IQueryAdapter dbClient)
        {
            this.ClearRights();
            dbClient.SetQuery("SELECT command,rank FROM server_fuses;");
            DataTable table = dbClient.GetTable();
            if (table != null)
            {
                foreach (DataRow dataRow in table.Rows)
                {
                    if (!this._cmdRights.ContainsKey((string)dataRow[0]))
                    {
                        this._cmdRights.Add((string)dataRow[0], (string)dataRow[1]);
                    }
                    else
                    {
                        Logging.LogException(string.Format("Duplicate Fuse Command \"{0}\" found", dataRow[0]));
                    }
                }
            }
            dbClient.SetQuery("SELECT * FROM server_fuserights");
            DataTable table2 = dbClient.GetTable();
            if (table2 == null)
            {
                return;
            }
            foreach (DataRow dataRow2 in table2.Rows)
            {
                if ((int)dataRow2[3] == 0)
                {
                    if (!this._rights.ContainsKey((string)dataRow2[0]))
                    {
                        this._rights.Add((string)dataRow2[0], Convert.ToUInt32(dataRow2[1]));
                    }
                    else
                    {
                        Logging.LogException(string.Format("Unknown Subscription Fuse \"{0}\" found", dataRow2[0]));
                    }
                }
                else
                {
                    if ((int)dataRow2[3] > 0)
                    {
                        this._subRights.Add((string)dataRow2[0], (int)dataRow2[3]);
                    }
                    else
                    {
                        Logging.LogException(string.Format("Unknown fuse type \"{0}\" found", dataRow2[3]));
                    }
                }
            }
        }

        /// <summary>
        /// Ranks the got command.
        /// </summary>
        /// <param name="rankId">The rank identifier.</param>
        /// <param name="cmd">The command.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool RankGotCommand(uint rankId, string cmd)
        {
            if (!this._cmdRights.ContainsKey(cmd))
            {
                return false;
            }
            if (!this._cmdRights[cmd].Contains(";"))
            {
                return rankId >= uint.Parse(this._cmdRights[cmd]);
            }

            string[] cmdranks = this._cmdRights[cmd].Split(';');
            return cmdranks.Any(rank => rank.Contains(Convert.ToString(rankId))) || this._cmdRights[cmd].Contains(Convert.ToString(rankId));
        }

        /// <summary>
        /// Ranks the has right.
        /// </summary>
        /// <param name="rankId">The rank identifier.</param>
        /// <param name="fuse">The fuse.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool RankHasRight(uint rankId, string fuse)
        {
            return this.ContainsRight(fuse) && rankId >= this._rights[fuse];
        }

        /// <summary>
        /// Determines whether the specified sub has vip.
        /// </summary>
        /// <param name="sub">The sub.</param>
        /// <param name="fuse">The fuse.</param>
        /// <returns><c>true</c> if the specified sub has vip; otherwise, <c>false</c>.</returns>
        internal bool HasVip(int sub, string fuse)
        {
            return this._subRights.ContainsKey(fuse) && this._subRights[fuse] == sub;
        }

        /// <summary>
        /// Gets the rights for rank.
        /// </summary>
        /// <param name="rankId">The rank identifier.</param>
        /// <returns>List&lt;System.String&gt;.</returns>
        internal List<string> GetRightsForRank(uint rankId)
        {
            var list = new List<string>();
            foreach (KeyValuePair<string, uint> current in this._rights.Where(current => rankId >= current.Value && !list.Contains(current.Key)))
            {
                list.Add(current.Key);
            }
            return list;
        }

        /// <summary>
        /// Determines whether the specified right contains right.
        /// </summary>
        /// <param name="right">The right.</param>
        /// <returns><c>true</c> if the specified right contains right; otherwise, <c>false</c>.</returns>
        internal bool ContainsRight(string right)
        {
            return this._rights.ContainsKey(right);
        }

        /// <summary>
        /// Clears the rights.
        /// </summary>
        internal void ClearRights()
        {
            this._rights.Clear();
            this._cmdRights.Clear();
            this._subRights.Clear();
        }
    }
}