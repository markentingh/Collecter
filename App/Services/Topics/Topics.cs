using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Collector.Services
{
    public class Topics : Service
    {
        public Topics(Core CollectorCore) : base(CollectorCore)
        {
        }

        public string CreateTopicFromBreadcrumb(string breadcrumb = "", string title = "", string description = "", string search = "", string location = "", string media = "", int sort = 0)
        {
            if (breadcrumb == "") { return Error(); }
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
            var query = new Query.Topics(S.Server.sqlConnection);
            var id = query.CreateTopicFromBreadcrumb(parentBreadcrumb, parentTitle, 0, 0, title, location, description, media);

            return "success|" + id.ToString();
        }

        #region "Topics"

        public string LoadTopicsUI(int start = 1, int length = 50, string subjectIds = "", string search = "", int sort = 0, DateTime dateStart = new DateTime(), DateTime dateEnd = new DateTime())
        {
            return Inject(InjectTopicsUI(start, length, subjectIds, search, sort));
        }
        
        public structInject InjectTopicsUI(int start=1, int length=50, string subjectIds = "", string search = "", int sort = 0, DateTime dateStart = new DateTime(), DateTime dateEnd = new DateTime())
        {
            var inject = new structInject() { };
            if (!CheckSecurity()) { inject.html = AccessDenied(); return inject; }

            var html = new StringBuilder();
            var query = new Query.Topics(S.Server.sqlConnection);

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

            var topics = query.GetList(start, length, subjectIds, search, d1, d2, (Query.Topics.SortBy)sort);

            if(topics.Count == 0) {
                inject.html = Error();
                return inject;
            }

            for(var x = 0; x < topics.Count; x++)
            {
                //add topic to list
                html.Append(LoadTopicListItem(topics[x]).Render() + "\n");
            }

            inject.html = html.ToString();
            return inject;
        }

        public Scaffold LoadTopicListItem(Query.Models.Topic topic)
        {
            var item = new Scaffold(S, "/Services/Topics/list-item.html");
            item.Data["topicId"] = topic.topicId.ToString();
            item.Data["title"] = topic.title;
            if (topic.breadcrumb != null && topic.breadcrumb != "")
            {
                //show subject breadcrumb
                var bread = topic.breadcrumb.Split('>');
                var hier = topic.hierarchy.Split('>');
                var crumb = "";
                var hasSubject = false;
                for (var b = 0; b < bread.Length; b++)
                {
                    crumb += (crumb != "" ? " > " : "") + "<a href=\"/subjects?id=" + hier[b] + "\">" + bread[b] + "</a>";
                    if (int.Parse(hier[b]) == topic.subjectId) { hasSubject = true; }
                }
                if (hasSubject == false)
                {
                    crumb += (crumb != "" ? " > " : "") + "<a href=\"/subjects?id=" + topic.subjectId + "\">" + topic.subjectTitle + "</a>";
                }
                item.Data["breadcrumbs"] = "1";
                item.Data["crumb"] = crumb;
            }
            else
            {
                //hide subject breadcrumb
                item.Data["breadcrumbs"] = "";
            }
            return item;
        }

        #endregion

        #region "Topic"


        #endregion
    }
}
