using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Collector.Pages
{
    public class Topic : Page
    {
        public Topic(Core CollectorCore) : base(CollectorCore)
        {
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //load dashboard interface
            var dashboard = new Partials.Dashboard(S);

            //add custom menu to dashboard
            dashboard.AddMenuItem("btnaddsection", "+ New Section", "");
            dashboard.AddMenuItem("btnsavechanges", "Save Changes", "");

            //load variables for page content
            var accordion = new Partials.Accordion(S);
            var query = new Query.Topics(S.Server.sqlConnection);
            var service = new Services.Topics(S);
            var html = new StringBuilder();
            var topic = query.GetDetails(int.Parse(path[1]));
            var topicPath = "/content/topics/" + topic.hierarchy.Replace(">", "/") + "/";
            var topicFile = topicPath + topic.topicId + ".json";

            //render topic summary list item into an accordion
            html.Append(accordion.Render("Topic", "topic-summary", service.LoadTopicListItem(topic).Render()) + "\n");

            //render topic media
            html.Append(accordion.Render("Images", "topic-media", service.GetTopicMediaList(topic)) + "\n");

            //load topic sections from cache or JSON file
            var sections = service.GetTopicSectionsFromJSON(topicFile);

            //add page resources
            dashboard.AddScript("/js/utility/dropzone.js");
            dashboard.AddScript("/js/utility/marked.js");
            dashboard.AddScript("/js/utility/autosize.min.js");
            dashboard.AddScript("/js/pages/topics/topic.js");
            dashboard.AddCSS("/css/utility/dropzone.css");
            dashboard.AddCSS("/css/pages/topics/topic.css");
            dashboard.scripts += scripts + 
                "<script language=\"javascript\">" +
                    "S.topic.load(" + topic.topicId + ");" + 
                "</script>";

            //render each topic section
            var i = 0;
            foreach (var section in sections)
            {
                i++;
                var sect = service.GetTopicEditableAccordion(topic.title, section.title, section.content, "section", "section" + i, topic.topicId, i, true, i == 1 ? true : false, true);
                html.Append(sect.html);
                scripts += sect.javascript;
            }

            //finally, render page
            return dashboard.Render(path, html.ToString(), metadata);
        }
    }
}
