using System;
using System.Collections.Generic;

namespace Collector.Query
{
    public class Topics : global::Query.QuerySql
    {
        public int CreateTopic(int subjectId, double geolat, double geolong, string title, string location, string summary, string media)
        {
            return Sql.ExecuteScalar<int>(
                "Topic_Create",
                new Dictionary<string, object>()
                {
                    {"subjectId", subjectId },
                    {"geolat", geolat },
                    {"geolong", geolong },
                    {"title", title },
                    {"location", location },
                    {"summary", summary },
                    {"media", media }
                }
            );
        }

        public int CreateTopicFromBreadcrumb(string breadcrumb, string subjectTitle, double geolat, double geolong, string title, string location, string summary, string media)
        {
            return Sql.ExecuteScalar<int>(
                "Topic_CreateFromBreadcrumb",
                new Dictionary<string, object>()
                {
                    {"breadcrumb", breadcrumb },
                    {"subject", subjectTitle },
                    {"geolat", geolat },
                    {"geolong", geolong },
                    {"title", title },
                    {"location", location },
                    {"summary", summary },
                    {"media", media }
                }
            );
        }

        public enum SortBy
        {
            dateAsc = 1,
            dateDesc = 2
        }

        public List<Models.Topic> GetList(int start, int length, string subjectIds, string search, DateTime dateStart, DateTime dateEnd, SortBy sortBy)
        {
            return Sql.Populate<Models.Topic>(
                "Topics_GetList",
                new Dictionary<string, object>()
                {
                    {"start", start },
                    {"length", length },
                    {"subjectIds", subjectIds },
                    {"search", search },
                    {"dateStart", dateStart },
                    {"dateEnd", dateEnd },
                    {"orderBy", sortBy }
                }
            );
        }

        public Models.Topic GetDetails(int topicId)
        {
            var list = Sql.Populate<Models.Topic>(
                "Topic_GetDetails",
                new Dictionary<string, object>()
                {
                    {"topicId", topicId }
                }
            );
            if(list.Count > 0) {
                var item = list[0];
                item.path = "/content/topics/" + item.hierarchy.Replace(">", "/") + "/"; ;
                item.filename = item.topicId + ".json";
                return item;
            }
            return null;
        }

        public void UpdateMediaForTopic(int topicId, string media)
        {
            Sql.ExecuteNonQuery("Topic_UpdateMedia",
                new Dictionary<string, object>()
                {
                    {"topicId", topicId },
                    {"media", media }
                }
            );
        }
    }
}


/* 
    @subjectIds nvarchar(MAX),
	@search nvarchar(MAX),
	@dateStart nvarchar(50),
	@dateEnd nvarchar(50),
	@orderby int = 1,
	@start int = 1,
	@length int = 50

    */
