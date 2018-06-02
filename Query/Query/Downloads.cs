using System.Collections.Generic;

namespace Collector.Query
{
    public class Downloads : global::Query.QuerySql
    {
        public enum QueueStatus
        {
            queued = 0,

        }
        public void UpdateQueueItem(int queueId, QueueStatus status)
        {
            Sql.ExecuteNonQuery("Download_Update",
                new Dictionary<string, object>()
                {
                    {"qid", queueId },
                    {"status", (int)status }
                }
            );
        }

        public void AddToDownloadDistribution(int serverId)
        {
            Sql.ExecuteNonQuery("DownloadDistribution_Add",
                new Dictionary<string, object>()
                {
                    {"serverId", serverId }
                }
            );
        }

        public List<Models.DownloadQueue> GetDistributionList(int serverId)
        {
            return Sql.Populate<Models.DownloadQueue>(
                "DownloadDistributions_GetList",
                new Dictionary<string, object>()
                {
                    {"serverId", serverId }
                }
            );
        }

        public bool AddQueueItem(string url, int feedId = 0)
        {
            return Sql.ExecuteScalar<int>("DownloadQueue_Add",
                new Dictionary<string, object>()
                {
                    {"url", url },
                    {"feedId", feedId }
                }
            ) == 1;
        }

        public int CheckQueue()
        {
            return Sql.ExecuteScalar<int>("DownloadQueue_Check");
        }

        public int Count()
        {
            return Sql.ExecuteScalar<int>("Downloads_GetCount");
        }

        public enum ServerType
        {
            Windows = 1,
            Linux = 2,
            RaspberryPi = 3,
            Docker = 4
        }

        public int AddServer(ServerType type, string title, string settings = "")
        {
            return Sql.ExecuteScalar<int>("DownloadServer_Add",
                new Dictionary<string, object>()
                {
                    {"type", (int)type },
                    {"title", title },
                    {"settings", settings }
                }
            );
        }

        public bool ServerExists(string settings)
        {
            return Sql.ExecuteScalar<int>("DownloadServer_Exists",
                new Dictionary<string, object>()
                {
                    {"settings", settings }
                }
            ) == 1;
        }

        public bool GetServerId(string settings)
        {
            return Sql.ExecuteScalar<int>("DownloadServer_GetId",
                new Dictionary<string, object>()
                {
                    {"settings", settings }
                }
            ) == 1;
        }

        public List<Models.DownloadServer> GetServerList()
        {
            return Sql.Populate<Models.DownloadServer>("DownloadServers_GetList");
        }
    }
}
