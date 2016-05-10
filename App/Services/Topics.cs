using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNet.Http;
using Microsoft.Net.Http.Headers;

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
            public string[] media;
            public DateTime datecreated;
        }

        private struct sectionInfo
        {
            public string title;
            public string content;
            public int id;
        }

        private topicInfo _topic = new topicInfo();

        #region "List"

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

        #endregion

        #region "Edit"

        private topicInfo Topic
        {
            get {
                if (_topic.topicId == 0)
                {
                    _topic = (topicInfo)S.Util.Serializer.ReadObject(S.Page.Items.Item("topic"), typeof(topicInfo));
                }
                return _topic;
            }
            set {
                if(S.Page.Items.IndexOf("topic") >= 0){
                    S.Page.Items.Remove("topic");
                }
                S.Page.Items.Add("topic", S.Util.Serializer.WriteObjectAsString(value));
                
            }
        }

        private List<sectionInfo> OpenTopicJson()
        {
            if (File.Exists(S.Server.MapPath(Topic.topicPath + Topic.topicId.ToString() + ".json")) == true)
            {
                return (List<sectionInfo>)S.Util.Serializer.ReadObject(S.Server.OpenFile(Topic.topicPath + Topic.topicId.ToString() + ".json"), typeof(List<sectionInfo>));
            }
            return new List<sectionInfo>();
        }

        public string LoadTopicsEditorUI(int topicId)
        {
            var htm = new StringBuilder();
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
                topic.media = S.Sql.Decode(reader.Get("media")).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                //save topic to view-state
                Topic = topic;

                //show topic summary
                htm.Append(RenderAccordion("Topic", "topic-summary", GetTopicListItem(topic.title, topicId, topic.breadcrumb, topic.hierarchy, topic.subjectId, topic.subjectTitle)));

                //show topic media (photos & videos)
                htm.Append(RenderAccordion("Media", "topic-media", GetTopicMediaList()));

                //open topic json file
                List<sectionInfo> sections = OpenTopicJson();
                if(sections.Count > 0) { 
                    foreach (sectionInfo section in sections)
                    {
                        htm.Append(GetTopicEditableAccordion(topic.title, section.title, section.content, "section", "section" + i, topicId, i, true, (i == 1 ? true : false)));
                        i++;
                    }
                }
                else
                {
                    //create first section
                    htm.Append(GetTopicEditableAccordion(topic.title, topic.title, "", "section", "section1", topicId, 1, true));
                }
                
            }
            return htm.ToString();
        }

        private string GetTopicEditableAccordion(string topicTitle, string title, string content, string groupName, string className, int topicId, int index, bool whiteBg = false, bool expanded = true, bool editMode = false, bool isNew = false)
        {
            //register javascript events for the editable form
            S.Page.RegisterJS("topicsection" + className, 
                (!editMode ? (content != "" ? "S.topics.edit.buttons.previewTopic('" + className + "');" : "") : "") +
                    "$('." + className + "-btn-edit').on('click', function(){S.topics.edit.buttons.editTopic('" + className + "');});" +
                    "$('." + className + "-btn-preview').on('click ', function(){S.topics.edit.buttons.previewTopic('" + className + "');});" +
                    "$('." + className + "-btn-newsection').on('click', function(){S.topics.edit.buttons.addSection('"+ groupName + "'," + index + ");});" +
                    "$('." + className + "-btn-remove').on('click', function(){S.topics.edit.buttons.removeSection('" + groupName + "'," + index + ");});");

            S.Page.RegisterJS("topickeydown", 
                    "$('.topic-section input, .topic-section textarea').off().on('keydown', function(e){S.topics.edit.texteditor.keyDown(e.target);});" +
                    "$('.btn-savechanges').off().on('click', function(){S.topics.edit.buttons.saveChanges('" + groupName + "');});" +
                    "S.topics.edit.texteditor.autoSize();");


            //render an accordion with two tabs, one for editing a topic sectionl, the other for previewing
            return RenderAccordion(title, className,
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

        public Inject NewTopicSection(string element, bool after, string title, string content, int index, int count)
        {
            List<sectionInfo> sections = OpenTopicJson();
            //get next incremented ID
            int id = (sections.Aggregate((a, b) => a.id > b.id ? a : b).id) + 1;
            string className = "section" + id;
            var response = new Inject();

            //create new section in the json file
            var section = new sectionInfo();
            section.title = title;
            section.content = content;
            if (after)
            {
                if (index + 1 > count)
                {
                    sections.Add(section);
                }
                else {
                    sections.Insert(index + 1, section);
                }
            }
            else
            {
                sections.Insert(index, section);
            }
            
            SaveTopicList(sections);

            //generate accordion for new topic section
            response.inject = after ? enumInjectTypes.after : enumInjectTypes.before;
            response.element = element;
            response.html = GetTopicEditableAccordion(Topic.title, title, content, "section", className, Topic.topicId, id, true, true, true, true);
            response.js = CompileJs();
            return response;
        }

        private void SaveTopicList(List<sectionInfo> sections)
        {
            S.Util.Serializer.SaveToFile(sections, S.Server.MapPath("/content/topics/" + Topic.hierarchy.Replace(">", "/") + "/" + Topic.topicId + ".json"));
        }

        public void SaveTopic(string json)
        {
            //save changes for loaded topic
            List<sectionInfo> data = (List<sectionInfo>)S.Util.Serializer.ReadObject(json, typeof(List<sectionInfo>));
            var sections = OpenTopicJson();
            var i = 0;
            sectionInfo s;
            foreach (var d in data)
            {
                //update each section based on data
                i = sections.FindIndex(c => c.id == d.id);
                if(i >= 0)
                {
                    s = sections[i];
                    s.title = d.title;
                    s.content = d.content;
                    sections[i] = s;
                }
                else
                {
                    //create new section
                    s = new sectionInfo();
                    s.title = d.title;
                    s.content = d.content;
                    s.id = sections.Count > 0 ? (sections.Aggregate((a, b) => a.id > b.id ? a : b).id) + 1 : 1; //increment ID
                    sections.Add(s);
                }
            }
            SaveTopicList(sections);
        }

        public Inject RemoveSection(int id)
        {
            //find section based on id
            var inject = new Inject();
            List<sectionInfo> sections = OpenTopicJson();
            if(sections.Count > 0)
            {
                sections.RemoveAt(sections.FindIndex(s => s.id == id));
                SaveTopicList(sections);
            }
            else
            {

            }
            inject.js = CompileJs();
            return inject;
        }

        #endregion

        #region "Media"

        private string GetTopicMediaList(bool listOnly = false)
        {
            //display list of images
            var htm = new StringBuilder();
            if(listOnly == false)
            {
                htm.Append("<div class=\"media-list\">");
            }
            for (var x = 0; x < Topic.media.Length; x++)
            {
                if(Topic.media[x] != "")
                {
                    htm.Append("<div class=\"img img-" + x + "\"><img src=\"/topics/" + Topic.hierarchy.Replace(">", "/") + "/sm_" + Topic.media[x] + "\"/></div>");
                }
            }
            if (listOnly == false)
                {
                htm.Append("</div>");

                //add drop zone for uploading images
                htm.Append(
                    "<div class=\"upload-list\"></div>" +
                        "<div class=\"buttons\">" +
                            "<a href=\"javascript:\" class=\"button green btn-upload left\">Upload Images</a>" +
                            "<a href=\"javascript:\" class=\"button btn-select-all-images left\">Select All / None</a>" +
                            "<a href=\"javascript:\" class=\"button btn-delete-selected-images left\">Delete Selected</a>" +
                            "<a href=\"javascript:\" class=\"button blue btn-gallery-toggle right\">Gallery</a>" +
                        "</div>" +
                    "<div class=\"dropzone\">" +
                    "<span class=\"drop-here\">Drop Images Here</span>" +
                    "</div>");
            }
            return htm.ToString();
        }

        public WebRequest Upload()
        {
            WebRequest wr = new WebRequest();
            if (Files.Count > 0)
            {
                string folder = S.Request.Query["folder"];
                if (folder == null) { folder = ""; }
                if (folder != "") { folder += "/"; }
                string path = "/wwwroot/topics/" + Topic.hierarchy.Replace(">", "/") + "/";
                folder = folder.Replace("/", "");
                string ext = ""; string name = ""; string filename = ""; string filenew = ""; 
                Utility.Images image = new Utility.Images(S);

                foreach (IFormFile file in Files)
                {
                    filename = S.Util.Str.replaceAll(ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"'), "",
                        new string[] { "-", "_", "!", "@", "#", "$", "%", "&", "*", "+", "=", ",", "?", " " });

                    ext = S.Util.Str.getFileExtension(filename).ToLower();
                    name = filename.ToLower().Replace(ext,"");
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
                            if(File.Exists(S.Server.MapPath(path + filenew)))
                            {
                                try
                                {
                                    File.Delete(S.Server.MapPath(path + filenew));
                                }catch(Exception ex){ }
                                
                            }
                            try
                            {
                                file.SaveAs(S.Server.MapPath(path + filenew));
                            }catch(Exception ex){ }

                            if(File.Exists(S.Server.MapPath(path + filenew)))
                            {
                                //save thumbnail version of image
                                image.Shrink(path + filenew, path + "sm_" + filenew, 150);

                                if (File.Exists(S.Server.MapPath(path + "sm_" + filenew)))
                                {

                                    //get photo dimensions
                                    //Utility.structImage photo = image.Load(path, filenew);

                                    //save photo to database
                                    var topic = Topic;
                                    var media = topic.media.ToList();
                                    if (!media.Contains(filenew))
                                    {
                                        media.Add(filenew);
                                        topic.media = media.ToArray();

                                        //save topic to view-state
                                        Topic = topic;

                                        //save to database
                                        S.Sql.ExecuteNonQuery("UPDATE Topics SET media='" + S.Sql.Encode(string.Join(",", topic.media)) + "' WHERE topicId=" + topic.topicId);
                                    }
                                }
                            }
                            break;
                    }

                }
            }
            return wr;
        }

        public Inject SaveUpload()
        {
            //executed after all images are uploaded
            var inject = new Inject();
            S.Page.RegisterJS("saveup", "$('.topic-media .dropzone').removeClass('uploaded');");
            inject.element = ".topic-media .media-list";
            inject.inject = enumInjectTypes.replace;
            inject.html = GetTopicMediaList(true);
            inject.js = CompileJs();
            return inject;
        }
        #endregion
    }
}
