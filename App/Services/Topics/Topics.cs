using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;


namespace Collector.Services
{
    public class Topics : Service
    {
        public Topics(Core CollectorCore) : base(CollectorCore)
        {
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
        public string CreateTopicFromBreadcrumb(string breadcrumb = "", string title = "", string description = "", string search = "", string location = "", string media = "", int sort = 0)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
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

        public List<Query.Models.TopicSection> GetTopicSectionsFromJSON(string topicFile)
        {
            if (File.Exists(S.Server.MapPath(topicFile)))
            {
                return (List<Query.Models.TopicSection>)S.Util.Serializer.ReadObject(S.Server.LoadFileFromCache(topicFile), typeof(List<Query.Models.TopicSection>));
            }
            return new List<Query.Models.TopicSection>();
        }

        public structInject GetTopicEditableAccordion(string topicTitle, string title, string content, string groupName, string className, int topicId, int index, bool whiteBg = false, bool expanded = true, bool editMode = false, bool isNew = false)
        {
            var inject = new structInject();
            var accordion = new Partials.Accordion(S);

            //register javascript events for the editable form
            inject.javascript += "<script language=\"javascript\">" +
                    (!editMode ? (content != "" ? "S.topic.buttons.previewTopic('" + className + "');" : "") : "") +
                    "$('." + className + "-btn-edit').on('click', function(){S.topic.buttons.editTopic('" + className + "');});" +
                    "$('." + className + "-btn-preview').on('click ', function(){S.topic.buttons.previewTopic('" + className + "');});" +
                    "$('." + className + "-btn-newsection').on('click', function(){S.topic.buttons.addSection('" + groupName + "'," + index + ");});" +
                    "$('." + className + "-btn-remove').on('click', function(){S.topic.buttons.removeSection('" + groupName + "'," + index + ");});" +
                    "$('.topic-section input, .topic-section textarea').off().on('keydown', function(e){S.topic.texteditor.keyDown(e.target);});" +
                    "$('.btn-savechanges').off().on('click', function(){S.topic.buttons.saveChanges('" + groupName + "');});" +
                    "S.topic.texteditor.autoSize();" +
                "</script>";

            //render an accordion with two tabs, one for editing a topic sectionl, the other for previewing
            inject.html = accordion.Render(title, className,
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
                            "<div class=\"row column\"><input type=\"text\" class=\"txt-title\" value=\"" + title.Replace("\"", "\\\"") + "\"></div>" +
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
            return inject;
        }

        public string GetTopicMediaList(Query.Models.Topic topic, bool listOnly = false)
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
                    //"<form enctype=\"multipart/form-data\" method=\"post\">" + 
                    "<div class=\"upload-list\"></div>" +
                        "<div class=\"buttons\">" +
                            "<a href=\"javascript:\" class=\"button green btn-upload left\">Upload Images</a>" +
                            "<a href=\"javascript:\" class=\"button btn-select-all-images left\">Select All / None</a>" +
                            "<a href=\"javascript:\" class=\"button btn-delete-selected-images red left\">Delete Selected</a>" +
                            "<a href=\"javascript:\" class=\"button blue btn-gallery-toggle right\">View As Gallery</a>" +
                        "</div>" +
                    "<div class=\"dropzone\">" +
                    "<span class=\"drop-here\">Drop Images Here</span>" +
                    "</div>");//</form>");
            }
            return htm.ToString();
        }

        public string NewTopicSection(int topicId, bool after, string title, string content, int index)
        {
            var inject = new structInject();
            if (!CheckSecurity()) {
                inject.html = AccessDenied();
                return Inject(inject);
            }

            var query = new Query.Topics(S.Server.sqlConnection);
            var topic = query.GetDetails(topicId);
            if(topic != null)
            {
                //get topic sections from JSON file
                var sections = GetTopicSectionsFromJSON(topic.path + topic.filename);
                var section = new Query.Models.TopicSection();

                //get next incremented ID
                var id = 0;
                if(sections.Count > 0)
                {
                    id = (sections.Aggregate((a, b) => a.id > b.id ? a : b).id) + 1;
                }
                if (id == 0)
                {
                    id = sections.Count;
                    if (id == 0) { id = 1; }
                }

                section.title = title;
                section.content = content;
                section.type = "markdown";
                if (after)
                {
                    if (index == sections.Count)
                    {
                        sections.Add(section);
                    }
                    else
                    {
                        sections.Insert(index + 1, section);
                    }
                    inject.inject = 3; //append after
                }
                else
                {
                    sections.Insert(index, section);
                }
                
                //save JSON to file
                SaveTopicList(topic, sections);

                //get topic content editor UI
                var ui = GetTopicEditableAccordion(topic.title, title, content, "section", "section" + id, topicId, id, true, true, true, true);
                inject.html = ui.html;
                inject.javascript += ui.javascript;
            }
            else { inject.html = "topic does not exist"; }
            

            return Inject(inject);
        }

        private void SaveTopicList(Query.Models.Topic topic, List<Query.Models.TopicSection> sections)
        {
            S.Util.Serializer.WriteObjectToFile(sections, S.Server.MapPath("/content/topics/" + topic.hierarchy.Replace(">", "/") + "/" + topic.topicId + ".json"));
        }

        public void SaveTopic(Query.Models.Topic topic, string json)
        {
            //save changes for loaded topic
            List<Query.Models.TopicSection> data = (List<Query.Models.TopicSection>)S.Util.Serializer.ReadObject(json, typeof(List<Query.Models.TopicSection>));
            SaveTopicList(topic, data);
        }

        public string Upload()
        {
            if (Files.Count > 0)
            {
                var query = new Query.Topics(S.Server.sqlConnection);
                var topic = query.GetDetails(int.Parse(S.Request.Query["topicId"]));
                if(topic != null)
                {
                    string folder = S.Request.Query["folder"];
                    var path = "/wwwroot/topics/" + topic.hierarchy.Replace(">", "/") + "/";
                    var ext = ""; string name = ""; string filename = ""; string filenew = "";
                    var image = new Utility.Images(S);

                    //format folder
                    if (folder == null) { folder = ""; }
                    if (folder != "") { folder += "/"; }
                    folder = folder.Replace("/", "");

                    foreach (IFormFile file in Files)
                    {
                        filename = S.Util.Str.replaceAll(ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"'), "",
                            new string[] { "-", "_", "!", "@", "#", "$", "%", "&", "*", "+", "=", ",", "?", " " });

                        ext = S.Util.Str.getFileExtension(filename).ToLower();
                        name = filename.ToLower().Replace("." + ext, "");
                        if (name.Length > 20) { name = name.Substring(0, 20); }

                        switch (ext)
                        {
                            case "jpg":
                            case "jpeg":
                            case "png":
                            case "gif":
                                if (!Directory.Exists(S.Server.MapPath(path)))
                                {
                                    //create directory
                                    Directory.CreateDirectory(S.Server.MapPath(path));
                                }

                                //save original photo to disk
                                filenew = name + "." + ext;
                                var filepath = S.Server.MapPath(path + filenew);

                                if (File.Exists(filepath))
                                {
                                    //delete existing file
                                    try
                                    {
                                        File.Delete(filepath);
                                    }
                                    catch (Exception) { }
                                }
                                try
                                {
                                    //save photo to disk
                                    using (var fs = new FileStream(filepath, FileMode.OpenOrCreate))
                                    {
                                        file.CopyTo(fs);
                                        fs.Close();
                                    }
                                }
                                catch (Exception) { }

                                if (File.Exists(S.Server.MapPath(path + filenew)))
                                {
                                    //save multpile thumbnail versions of the image
                                    var img = image.Load(path, filenew);
                                    image.Shrink(filepath, S.Server.MapPath(path + "sm_" + filenew), 150);
                                    if (img.width >= 800)
                                    {
                                        image.Shrink(filepath, S.Server.MapPath(path + "med_" + filenew), 800);
                                    }
                                    if (img.width >= 1920)
                                    {
                                        image.Shrink(filepath, S.Server.MapPath(path + "lg_" + filenew), 1920);
                                    }

                                    if (File.Exists(S.Server.MapPath(path + "sm_" + filenew)))
                                    {
                                        //save photo to database
                                        var media = topic.media.Split(',').ToList();
                                        if(media[0] == "") { media = new List<string>(); }
                                        if (!media.Contains(filenew))
                                        {
                                            media.Add(filenew);
                                            topic.media = string.Join(",",media.ToArray());

                                            //save to database
                                            query.UpdateMediaForTopic(topic.topicId, topic.media);
                                        }
                                    }
                                }
                                break;
                        }

                    }
                }
                
            }
            return Success();
        }

        public string SaveUpload(int topicId)
        {
            //executed after all images are uploaded
            var query = new Query.Topics(S.Server.sqlConnection);
            var topic = query.GetDetails(topicId);
            var inject = new structInject();
            inject.javascript = "$('.topic-media .dropzone').removeClass('uploaded');";
            inject.inject = 0; //replace
            inject.html = GetTopicMediaList(topic, true);
            return Inject(inject);
        }
        #endregion
    }
}
