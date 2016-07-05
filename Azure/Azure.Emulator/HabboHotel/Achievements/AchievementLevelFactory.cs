using Azure.Database.Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Azure.HabboHotel.Achievements
{
    /// <summary>
    /// Class AchievementLevelFactory.
    /// </summary>
    internal class AchievementLevelFactory
    {
        /// <summary>
        /// Gets the achievement levels.
        /// </summary>
        /// <param name="achievements">The achievements.</param>
        /// <param name="dbClient">The database client.</param>
        internal static void GetAchievementLevels(out Dictionary<string, Achievement> achievements, IQueryAdapter dbClient)
        {
            achievements = new Dictionary<string, Achievement>();
            dbClient.SetQuery("SELECT * FROM achievements_data");
            DataTable table = dbClient.GetTable();
            foreach (DataRow dataRow in table.Rows)
            {
                uint id = Convert.ToUInt32(dataRow["id"]);
                var category = (string)dataRow["category"];
                var text = (string)dataRow["group_name"];
                var level = (int)dataRow["level"];
                var rewardPixels = (int)dataRow["reward_pixels"];
                var rewardPoints = (int)dataRow["reward_points"];
                var requirement = (int)dataRow["progress_needed"];
                var level2 = new AchievementLevel(level, rewardPixels, rewardPoints, requirement);
                if (!achievements.ContainsKey(text))
                {
                    var achievement = new Achievement(id, text, category);
                    achievement.AddLevel(level2);
                    achievements.Add(text, achievement);
                }
                else
                    achievements[text].AddLevel(level2);
            }
        }
    }
}