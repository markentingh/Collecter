using System.Linq;

namespace Query
{
    public static class Downloads
    {
        public enum QueueStatus
        {
            queued = 0,
            pulled = 1, //when pulled from the queue to download
            downloaded = 2
        }

        public static void UpdateQueueItem(int qId, QueueStatus status = QueueStatus.downloaded)
        {
            Sql.ExecuteNonQuery("Download_Update", new { qId , status = (int)status });
        }

        public static int AddQueueItems(string urls, string domain, int feedId = 0)
        {
            return Sql.ExecuteScalar<int>("DownloadQueue_Add", new { urls, domain, feedId });
        }

        public static Models.DownloadQueue CheckQueue(int domaindelay = 5)
        {
            var list = Sql.Populate<Models.DownloadQueue>("DownloadQueue_Check", new { domaindelay });
            return list.FirstOrDefault();
        }

        public static int Count()
        {
            return Sql.ExecuteScalar<int>("Downloads_GetCount");
        }
    }
}
