using System;

namespace Query.Models
{
    public class DownloadQueue
    {
        public int qid { get; set; }
        public int domainId { get; set; }
        public int feedId { get; set; }
        public int status { get; set; }
        public string url { get; set; }
        public string domain { get; set; }
        public DateTime datecreated { get; set; }
    }
}
