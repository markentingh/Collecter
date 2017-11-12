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
            html.Append(accordion.Render("Images", "topic-media", GetTopicMediaList(topic)) + "\n");

            //load topic sections from cache or JSON file
            var sections = GetTopicSectionsFromJSON(topicFile);

            //render each topic section
            var i = 0;
            foreach (var section in sections)
            {
                i++;
                html.Append(GetTopicEditableAccordion(topic.title, section.title, section.content, "section", "section" + i, topic.topicId, i, true, i == 1 ? true : false, true));
            }

            //add page resources
            dashboard.scripts += scripts;
            dashboard.AddScript("/js/utility/dropzone.js");
            dashboard.AddScript("/js/utility/marked.js");
            dashboard.AddScript("/js/utility/autosize.min.js");
            dashboard.AddScript("/js/pages/topics/topic.js");
            dashboard.AddCSS("/css/utility/dropzone.css");
            dashboard.AddCSS("/css/pages/topics/topic.css");

            //finally, render page
            return dashboard.Render(path, html.ToString(), metadata);
        }

        private List<Query.Models.TopicSection> GetTopicSectionsFromJSON(string topicFile)
        {
            if (File.Exists(S.Server.MapPath(topicFile)))
            {
                return (List<Query.Models.TopicSection>)S.Util.Serializer.ReadObject(S.Server.LoadFileFromCache(topicFile), typeof(List<Query.Models.TopicSection>));
            }
            return new List<Query.Models.TopicSection>();
        }

        private string GetTopicEditableAccordion(string topicTitle, string title, string content, string groupName, string className, int topicId, int index, bool whiteBg = false, bool expanded = true, bool editMode = false, bool isNew = false)
        {
            var accordion = new Partials.Accordion(S);

            //register javascript events for the editable form
            scripts += "<script language=\"javascript\">" +  
                    (!editMode ? (content != "" ? "S.topic.buttons.previewTopic('" + className + "');" : "") : "") +
                    "$('." + className + "-btn-edit').on('click', function(){S.topic.buttons.editTopic('" + className + "');});" +
                    "$('." + className + "-btn-preview').on('click ', function(){S.topic.buttons.previewTopic('" + className + "');});" +
                    "$('." + className + "-btn-newsection').on('click', function(){S.topic.buttons.addSection('"+ groupName + "'," + index + ");});" +
                    "$('." + className + "-btn-remove').on('click', function(){S.topic.buttons.removeSection('" + groupName + "'," + index + ");});" +
                    "$('.topic-section input, .topic-section textarea').off().on('keydown', function(e){S.topic.texteditor.keyDown(e.target);});" +
                    "$('.btn-savechanges').off().on('click', function(){S.topic.buttons.saveChanges('" + groupName + "');});" +
                    "S.topic.texteditor.autoSize();" + 
                "</script>";

            //render an accordion with two tabs, one for editing a topic sectionl, the other for previewing
            return accordion.Render(title, className,
                    "<div class=\"topic-section id-section" + index + "\">" + 
                        //live preview
                        "<div class=\"preview\"" + (editMode ? " style=\"display:none\"" : "") + ">" +
                            "<div class=\"nopreview\"" + (content == "" ? "" : " style=\"display:none;\"") + ">" + 
                                "<div class=\"title\">Start writing in new section for the topic '" + topicTitle + "'.</div>" +
                                "<a href=\"javascript:\" class=\"button green " + className + "-btn-edit\">Edit Section</a>" + 
                            "</div>" +
                            "<div class=\"ispreview\"" + (content != "" ? "" : " style=\"display:none;\"") + ">" +
                                "<div class=\"section-contents\">" + "</div>" + //content + "</div>" +
                                "<div class=\"buttons\">" +
                                    "<a href=\"javascript:\" class=\"button green left " + className + "-btn-edit\">Edit</a>" +
                                    "<a href=\"javascript:\" class=\"button blue left " + className + "-btn-newsection\">+ New Section</a>" +
                                    "<a href=\"javascript:\" class=\"button green right btn-savechanges\" style=\"display:none;\">Save Changes</a>" +
                                "<a href=\"javascript:\" class=\"button left " + className + "-btn-remove\">Remove</a>" +
                                "</div>" +
                            "</div>" +
                        "</div>" +
                        //editable form used for markdown
                        "<div class=\"edit\"" + (editMode ? "" : " style=\"display:none\"") + ">" +
                            "<div class=\"row column label\">Section Title</div>" +
                            "<div class=\"row column\"><input type=\"text\" class=\"txt-title\" value=\"" + title.Replace("\"","\\\"") + "\"></div>" +
                            "<div class=\"row column label\">Content <span class=\"right\"><a href=\"https://guides.github.com/features/mastering-markdown/\" target=\"_blank\">Markdown</a></span></div>" +
                            "<div class=\"row column\"><textarea class=\"txt-content\">" + content + "</textarea></div>" +
                            "<div class=\"row column buttons\">" +
                                "<a href=\"javascript:\" class=\"button green left " + className + "-btn-preview\">Preview</a>" +
                                "<a href=\"javascript:\" class=\"button blue left " + className + "-btn-newsection\">+ New Section</a>" +
                                "<a href=\"javascript:\" class=\"button green left btn-savechanges\" style=\"display:none;\">Save Changes</a>" +
                                "<a href=\"javascript:\" class=\"button left " + className + "-btn-remove\">Remove</a>" +
                            "</div>" +
                        "</div>" +
                    "</div>"
                , expanded, whiteBg);
        }

        private string GetTopicMediaList(Query.Models.Topic topic, bool listOnly = false)
        {
            //display list of images
            var media = topic.media.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var htm = new StringBuilder();
            if (listOnly == false)
            {
                htm.Append("<div class=\"media-list\">");
            }
            for (var x = 0; x < media.Length; x++)
            {
                if (media[x] != "")
                {
                    htm.Append(
                        "<div class=\"img img-" + x + "\">" +
                            "<div class=\"chk\"><input type=\"checkbox\"/></div>" +
                            "<img src=\"/topics/" + topic.hierarchy.Replace(">", "/") + "/sm_" + media[x] + "\"/></div>");
                }
            }
            if (listOnly == false)
            {
                htm.Append("</div>" +
                    "<div class=\"img-details\">" +

                    "</div>");

                //add drop zone for uploading images
                htm.Append(
                    "<div class=\"upload-list\"></div>" +
                        "<div class=\"buttons\">" +
                            "<a href=\"javascript:\" class=\"button green btn-upload left\">Upload Images</a>" +
                            "<a href=\"javascript:\" class=\"button btn-select-all-images left\">Select All / None</a>" +
                            "<a href=\"javascript:\" class=\"button btn-delete-selected-images red left\">Delete Selected</a>" +
                            "<a href=\"javascript:\" class=\"button blue btn-gallery-toggle right\">View As Gallery</a>" +
                        "</div>" +
                    "<div class=\"dropzone\">" +
                    "<span class=\"drop-here\">Drop Images Here</span>" +
                    "</div>");
            }
            return htm.ToString();
        }
    }
}
