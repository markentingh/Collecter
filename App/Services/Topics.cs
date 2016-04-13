using System;
using System.Collections.Generic;
using System.Linq;

namespace Collector.Services
{
    public class Topics : Service
    {

        public Topics(Core CollectorCore, string[] paths):base(CollectorCore, paths)
        {
        }

        public string LoadTopicsUI(int start=1, int length=50, string subjectIds = "", string search = "", int sort = 0, DateTime dateStart = new DateTime(), DateTime dateEnd = new DateTime())
        {
            var htm = "";
            var reader = new SqlReader();
            var d1 = dateStart;
            var d2 = dateEnd;
            if (d1.Year == 1)
            {
                d1 = DateTime.Today.AddYears(-100);
            }
            if (d2.Year == 1)
            {
                d2 = DateTime.Today.AddYears(100);
            }
            reader.ReadFromSqlClient(
                S.Sql.ExecuteReader("EXEC GetTopics @start=" + start + ", @length=" + length + ", @subjectIds='" + subjectIds + "', @search='" + search + "', " + 
                                    "@orderby=" + sort + ", @dateStart=" + reader.ConvertDateTime(d1) + ", @dateEnd=" + reader.ConvertDateTime(d2)));
            if (reader.Rows.Count > 0)
            {
                while (reader.Read())
                {
                    htm += GetTopicListItem(S.Sql.Decode(reader.Get("title")), reader.GetInt("topicId"), reader.Get("breadcrumb"),
                                        reader.Get("hierarchy"), reader.GetInt("subjectId"), reader.Get("subjectTitle"));
                }
            }
            else
            {
                htm = "<div class=\"topic\"><div class=\"notopics\">You have not created any topics yet.</div></div>";
            }
            return htm;
        }

        public string GetTopicListItem(string title, int topicId, string breadcrumb, string hierarchy, int subjectId, string subjectTitle)
        {
            var htm = "<div class=\"row topic\"><div class=\"title\"><a href=\"/dashboard/topics/edit?topic=" + topicId + "\">" + S.Sql.Decode(title) + "</a></div>";
            if (breadcrumb.Length > 0)
            {
                //show subject breadcrumb
                var bread = S.Sql.Decode(breadcrumb).Split('>');
                var hier = S.Sql.Decode(hierarchy).Split('>');
                var crumb = "";
                var hasSubject = false;
                for (var b = 0; b < bread.Length; b++)
                {
                    crumb += (crumb != "" ? " > " : "") + "<a href=\"dashboard/subjects?id=" + hier[b] + "\">" + bread[b] + "</a>";
                    if (int.Parse(hier[b]) == subjectId) { hasSubject = true; }
                }
                if (hasSubject == false)
                {
                    crumb += (crumb != "" ? " > " : "") + "<a href=\"dashboard/subjects?id=" + subjectId + "\">" + S.Sql.Decode(subjectTitle) + "</a>";
                }
                htm += "<div class=\"subject\">" + crumb + "</div>";
            }

            htm += "</div>";
            return htm;
        }

        public string AddTopic(string breadcrumb = "", string title = "", string description = "", string search = "", int sort = 0)
        {
            if(breadcrumb == "") { return LoadTopicsUI(1, 50, "", search, sort); }
            var hier = breadcrumb.ToLower().Replace(" > ", ">").Replace("> ", ">").Replace(" >", ">").Split('>');
            var parentTitle = "";
            var parentBreadcrumb = "";
            if (hier.Length > 0)
            {
                var parentHier = hier.ToList();
                parentTitle = hier[hier.Length - 1];
                parentHier.RemoveAt(parentHier.Count - 1);
                parentBreadcrumb = string.Join(">", parentHier);
            }
            S.Sql.ExecuteNonQuery("EXEC AddTopicByBreadcrumb @title='" + S.Sql.Encode(title) + "', @summary='" + S.Sql.Encode(description) + "', @subject='" + parentTitle + "', @breadcrumb='" + parentBreadcrumb + "'");
            return LoadTopicsUI(1, 50, "", search, sort);
        }
    }
}
