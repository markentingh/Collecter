using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Text;

namespace Collector.Services
{
    public class Topics : Service
    {

        public Topics(Core CollectorCore, string[] paths):base(CollectorCore, paths)
        {
        }

        private struct topicInfo
        {
            public int topicId;
            public int subjectId;
            public string title;
            public string subjectTitle;
            public string hierarchy;
            public string breadcrumb;
            public string topicPath;
            public DateTime datecreated;
        }

        private struct sectionInfo
        {
            public string title;
            public string content;
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

        public string LoadTopicsEditorUI(int topicId)
        {
            var htm = "";
            var reader = new SqlReader();
            reader.ReadFromSqlClient(S.Sql.ExecuteReader("EXEC GetTopic @topicId=" + topicId));
            if(reader.Rows.Count > 0)
            {
                var topic = new topicInfo();
                int i = 1;
                reader.Read();
                topic.topicId = topicId;
                topic.title = S.Sql.Decode(reader.Get("title"));
                topic.subjectId = reader.GetInt("subjectId");
                topic.subjectTitle = S.Sql.Decode(reader.Get("subjectTitle"));
                topic.hierarchy = S.Sql.Decode(reader.Get("hierarchy"));
                topic.breadcrumb = S.Sql.Decode(reader.Get("breadcrumb"));
                topic.datecreated = reader.GetDateTime("datecreated");
                topic.topicPath = "/content/topics/" + topic.hierarchy.Replace(">", "/") + "/";
                
                //show topic summary
                htm += RenderAccordion("Topic", "topic-summary", GetTopicListItem(topic.title, topicId, topic.breadcrumb, topic.hierarchy, topic.subjectId, topic.subjectTitle));

                //open topic json file
                if(File.Exists(S.Server.MapPath(topic.topicPath + topicId.ToString() + ".json")) == true)
                {
                    List<sectionInfo> sections = (List<sectionInfo>)S.Util.Serializer.ReadObject(File.ReadAllText(S.Server.MapPath(topic.topicPath + topicId.ToString() + ".json")), typeof(List<sectionInfo>));
                    foreach(sectionInfo section in sections)
                    {
                        htm += GetTopicEditableAccordion(topic.title, section.title, section.content, "section", "section" + i, topicId, true);
                        i++;
                    }
                }
                else
                {
                    //create first section
                    htm += GetTopicEditableAccordion(topic.title, topic.title, "", "section", "section1", topicId, true);
                }
                S.Page.Items.Add("topic", S.Util.Serializer.WriteObjectAsString(topic));
            }
            return htm;
        }

        private string GetTopicEditableAccordion(string topicTitle, string title, string content, string groupName, string className, int topicId, bool whiteBg = false, bool expanded = true, bool editMode = false)
        {
            //register javascript events for the editable form
            S.Page.RegisterJS(
                    "topicsection" + className, (!editMode ? (content != "" ? "S.topics.edit.buttons.previewTopic('" + className + "');" : "") : "") +
                    "$('#" + className + "-btn-edit, #" + className + "-btn-edit2').on('click', function(){S.topics.edit.buttons.editTopic('" + className + "');});" +
                    "$('#" + className + "-btn-preview').on('click', function(){S.topics.edit.buttons.previewTopic('" + className + "');});");
            S.Page.RegisterJS(
                    "topickeydown", "$('.topic-section input, .topic-section textarea').off().on('keydown', function(e){S.topics.edit.texteditor.keyDown(e.target);});" +
                    "$('.btn-savechanges').off().on('click', function(){S.topics.edit.buttons.saveChanges('" + groupName + "');});" +
                    "S.topics.edit.texteditor.autoSize();");


            //render an accordion with two tabs, one for editing a topic sectionl, the other for previewing
            return RenderAccordion(title, className,
                    "<div class=\"topic-section\">" + 
                        "<div class=\"preview\"" + (editMode ? " style=\"display:none\"" : "") + ">" +
                            "<div class=\"nopreview\"" + (content == "" ? "" : " style=\"display:none;\"") + ">" + 
                                "<div class=\"title\">Start writing in new section for the topic '" + topicTitle + "'.</div>" +
                                "<a href=\"javascript:\" class=\"button green\" id=\"" + className + "-btn-edit\">Edit Section</a>" + 
                            "</div>" +
                            "<div class=\"ispreview\"" + (content != "" ? "" : " style=\"display:none;\"") + ">" +
                                "<div class=\"section-contents\">" + "</div>" + //content + "</div>" +
                                "<div class=\"buttons\">" +
                                    "<a href=\"javascript:\" class=\"button green left\" id=\"" + className + "-btn-edit2\">Edit</a>" +
                                    "<a href=\"javascript:\" class=\"button blue left\" id=\"" + className + "-btn-newsection2\">+ New Section</a>" +
                                    "<a href=\"javascript:\" class=\"button green right btn-savechanges\" style=\"display:none;\">Save Changes</a>" +
                                "</div>" +
                            "</div>" +
                        "</div>" +
                        "<div class=\"edit\"" + (editMode ? "" : " style=\"display:none\"") + ">" +
                            "<div class=\"row column label\">Section Title</div>" +
                            "<div class=\"row column\"><input type=\"text\" id=\"" + className  + "-title\" value=\"" + title.Replace("\"","\\\"") + "\"></div>" +
                            "<div class=\"row column label\">Content <span class=\"right\"><a href=\"https://guides.github.com/features/mastering-markdown/\" target=\"_blank\">Markdown</a></span></div>" +
                            "<div class=\"row column\"><textarea id=\"" + className + "-content\">" + content + "</textarea></div>" +
                            "<div class=\"row column buttons\">" +
                                "<a href=\"javascript:\" class=\"button green left\" id=\"" + className + "-btn-preview\">Preview</a>" +
                                "<a href=\"javascript:\" class=\"button blue left\" id=\"" + className + "-btn-newsection\">+ New Section</a>" +
                                "<a href=\"javascript:\" class=\"button green right btn-savechanges\" style=\"display:none;\">Save Changes</a>" +
                            "</div>" +
                        "</div>" +
                    "</div>"
                , expanded, whiteBg);
        }

        public Inject NewTopicSection(string element, string topicTitle, string title, string content, string groupName, string className, int topicId, bool whiteBg = false, bool expanded = true, bool editMode = false)
        {
            var response = new Inject();
            response.inject = enumInjectTypes.before;
            response.element = element;
            response.html = GetTopicEditableAccordion(topicTitle, title, content, groupName, className, topicId, whiteBg, expanded, editMode);
            response.js = CompileJs();
            return response;
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

        public void SaveChanges(string json)
        {
            topicInfo topic = (topicInfo)S.Util.Serializer.ReadObject(S.Page.Items.Item("topic"), typeof(topicInfo));
            if (!S.Util.IsEmpty(topic))
            {
                //save changes for loaded topic
                List<sectionInfo> sections = (List<sectionInfo>)S.Util.Serializer.ReadObject(json, typeof(List<sectionInfo>));
                S.Util.Serializer.SaveToFile(sections, S.Server.MapPath("/content/topics/" + topic.hierarchy.Replace(">", "/") + "/" + topic.topicId + ".json"));
            }
        }
    }
}
