using System;
using System.Collections.Generic;

namespace Collector.Query
{
    public class Feeds : global::Query.QuerySql
    {
        public int Add(string title, string url, string filter = "", int checkIntervals = 720)
        {
            return Sql.ExecuteScalar<int>("Feed_Add",
                new Dictionary<string, object>()
                {
                    {"title", title },
                    {"url", url },
                    {"filter", filter },
                    {"checkIntervals", checkIntervals }
                }
            );
        }

        public void LogCheckedLinks(int feedId, int count)
        {
            Sql.ExecuteNonQuery("FeedCheckedLog_Add",
                new Dictionary<string, object>()
                {
                    {"feedId", feedId },
                    {"links", count }
                }
            );
        }

        public void UpdateLastChecked(int feedId)
        {
            Sql.ExecuteNonQuery("Feed_Checked",
                new Dictionary<string, object>()
                {
                    {"feedId", feedId }
                }
            );
        }

        public List<Models.Feed> GetList()
        {
            return Sql.Populate<Models.Feed>("Feeds_GetList");
        }

        public List<Models.FeedWithLog> GetListWithLogs(int days = 7, DateTime? dateStart = null)
        {
            return Sql.Populate<Models.FeedWithLog>("Feeds_GetListWithLogs",
                new Dictionary<string, object>()
                {
                    {"days", days },
                    {"dateStart", dateStart != null ? dateStart : DateTime.Now.AddDays(-7) }
                }
            );
        }

    }
}
