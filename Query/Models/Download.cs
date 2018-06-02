using System;

namespace Collector.Query.Models
{
    public class DownloadQueue
    {
        public int qid { get; set; }
        public int? rndid { get; set; }
        public int? feedId { get; set; }
        public int? serverId { get; set; }
        public int status { get; set; }
        public string url { get; set; }
        public DateTime? datecreated { get; set; }
    }

    public class DownloadServer
    {
        public int serverId { get; set; }
        public int type { get; set; }
        public string title { get; set; }
        public string settings { get; set; }
    }
}
