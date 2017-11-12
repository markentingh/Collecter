using System;

namespace Collector.Query.Models
{
    public class Topic
    {
        public int topicId { get; set; } // int
        public double geolat { get; set; } // float
        public double geolong { get; set; } // float
        public DateTime datecreated { get; set; } // datetime
        public string title { get; set; } // nvarchar(250)
        public string location { get; set; } // nvarchar(250)
        public string summary { get; set; } // nvarchar(max)
        public string media { get; set; } // nvarchar(max)
        public string breadcrumb { get; set; } // nvarchar(500)
        public string hierarchy { get; set; } // nvarchar(50)
        public int subjectId { get; set; } // int
        public string subjectTitle { get; set; } // nvarchar(50)
        public string path;
        public string filename;
    }

    public class TopicSection
    {
        public string title;
        public string content;
        public int id;
        public string type; // markdown, map, gallery, group
    }
}
