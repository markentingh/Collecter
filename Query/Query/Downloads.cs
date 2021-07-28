using System.Collections.Generic;

namespace Query
{
    public static class Downloads
    {
        public enum QueueStatus
        {
            queued = 0,

        }

        public static void UpdateQueueItem(int queueId, QueueStatus status)
        {
            Sql.ExecuteNonQuery("Download_Update", new { queueId , status = (int)status });
        }

        public static void AddToDownloadDistribution(int serverId)
        {
            Sql.ExecuteNonQuery("DownloadDistribution_Add", new { serverId });
        }

        public static List<Models.DownloadQueue> GetDistributionList(int serverId)
        {
            return Sql.Populate<Models.DownloadQueue>("DownloadDistributions_GetList", new { serverId });
        }

        public static bool AddQueueItem(string url, int feedId = 0)
        {
            return Sql.ExecuteScalar<int>("DownloadQueue_Add", new { url, feedId }) == 1;
        }

        public static int CheckQueue()
        {
            return Sql.ExecuteScalar<int>("DownloadQueue_Check");
        }

        public static int Count()
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

        public static int AddServer(ServerType type, string title, string settings = "")
        {
            return Sql.ExecuteScalar<int>("DownloadServer_Add", new { type = (int)type, title, settings });
        }

        public static bool ServerExists(string settings)
        {
            return Sql.ExecuteScalar<int>("DownloadServer_Exists", new { settings }) == 1;
        }

        public static bool GetServerId(string settings)
        {
            return Sql.ExecuteScalar<int>("DownloadServer_GetId", new { settings }) == 1;
        }

        public static List<Models.DownloadServer> GetServerList()
        {
            return Sql.Populate<Models.DownloadServer>("DownloadServers_GetList");
        }
    }
}
