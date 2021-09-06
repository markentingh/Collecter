using System;
using System.Collections.Generic;

namespace Query
{
    public static class Feeds
    {
        public static int Add(int categoryId, string title, string url, string filter = "", int checkIntervals = 720)
        {
            return Sql.ExecuteScalar<int>("Feed_Add", new { categoryId, title, url, filter, checkIntervals });
        }

        public static void LogCheckedLinks(int feedId, int count)
        {
            Sql.ExecuteNonQuery("FeedCheckedLog_Add", new { feedId, count });
        }

        public static void UpdateLastChecked(int feedId)
        {
            Sql.ExecuteNonQuery("Feed_Checked", new { feedId });
        }

        public static List<Models.Feed> GetList()
        {
            return Sql.Populate<Models.Feed>("Feeds_GetList");
        }

        public static List<Models.FeedWithLog> GetListWithLogs(int days = 7, DateTime? dateStart = null)
        {
            return Sql.Populate<Models.FeedWithLog>("Feeds_GetListWithLogs",
                new { days, dateStart = dateStart != null ? dateStart : DateTime.Now.AddDays(-7) });
        }

        public static void AddCategory(string title)
        {
            Sql.ExecuteNonQuery("Feeds_Category_Add", new { title });
        }

        public static List<Models.FeedCategory> GetCategories()
        {
            return Sql.Populate<Models.FeedCategory>("Feeds_Categories_GetList");
        }

        public static List<Models.Feed> Check()
        {
            return Sql.Populate<Models.Feed>("Feeds_Check");
        }
    }
}
