using System;

namespace Collector.Query.Models
{
    public class Feed
    {
        public int feedId { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public int? checkIntervals { get; set; }
        public DateTime? lastChecked { get; set; }
        public string filter { get; set; }
    }

    public class FeedWithLog: Feed
    {
        public short? loglinks { get; set; }
        public DateTime? logdatechecked { get; set; }
    }


}
