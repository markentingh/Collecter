using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utility.Strings;

namespace Collector.Common.Platform
{
    public static class Subjects
    {
        public static int[] Add(string[] subjects, string[] hierarchy)
        {
            var parentId = 0;
            var breadcrumb = "";
            var parentTitle = "";
            var parentBreadcrumb = "";
            if (hierarchy.Length > 0)
            {
                var parentHier = hierarchy.ToList();
                parentTitle = hierarchy[hierarchy.Length - 1];
                parentHier.RemoveAt(parentHier.Count - 1);
                parentBreadcrumb = string.Join(">", parentHier);
                breadcrumb = string.Join(">", hierarchy);
                var subject = Query.Subjects.GetSubjectByTitle(parentTitle, parentBreadcrumb);
                parentId = subject.subjectId;
            }

            var ids = new List<int>();
            foreach (string subject in subjects)
            {
                ids.Add(Query.Subjects.CreateSubject(parentId, 0, 0, subject, breadcrumb));
            }
            return ids.ToArray();
        }

        public static void Move(int subjectId, int newParentId)
        {
            Query.Subjects.Move(subjectId, newParentId);
        }

        #region "Render"
        public static Datasilk.Web.Response RenderList(int parentId = 0, bool getHierarchy = false, bool isFirst = false)
        {
            var inject = new Datasilk.Web.Response() { };

            var html = new StringBuilder();
            var list = new Scaffold("/Views/Subjects/subject.html");
            var item = new Scaffold("/Views/Subjects/list-item.html");
            var subjects = Query.Subjects.GetList("", parentId);
            var indexes = new string[] { };
            if (parentId > 0)
            {
                var details = Query.Subjects.GetSubjectById(parentId);
                if (details == null) {
                    throw new LogicException(LogicErrorCode.Unknown, "Parent subject does not exist");
                }

                //set up subject
                var crumb = details.breadcrumb.Replace(">", " &gt; ");
                if (details.parentId == 0) { crumb = details.title; } else { crumb += " &gt; " + details.title; }
                indexes = details.hierarchy.Split('>');
                list["parentId"] = details.subjectId.ToString();
                list["breadcrumbs"] = crumb;

                if (indexes.Length >= 1 && getHierarchy == true)
                {
                    var hier = details.hierarchy;
                    var bread = details.breadcrumb;
                    if (bread != "") { bread += ">" + details.title; } else { bread = details.title; }
                    var pId = "0";
                    if (hier != "")
                    {
                        var hier2 = hier.Split('>');
                        pId = hier2[hier2.Length - 1];
                    }
                    inject.javascript = "S.subjects.select.show(" + parentId + "," + pId + ",'" + bread + "', 0, true);";

                    //get inject object for parent within hierarchy
                    var parent = RenderList(indexes.Length > 1 ? int.Parse(indexes[indexes.Length - 2]) : 0, indexes.Length > 1 ? true : false);
                    html.Append(parent.html + "\n");
                    inject.javascript += parent.javascript;
                }
            }
            else
            {
                list["parentId"] = "0";
            }


            //set up subject sub-items
            subjects.ForEach((Query.Models.Subject subject) =>
            {
                var breadcrumbs = subject.breadcrumb;
                if (breadcrumbs == "") { breadcrumbs = subject.title; }
                item["subjectId"] = subject.subjectId.ToString();
                item["parentId"] = subject.parentId.ToString();
                item["breadcrumbs"] = subject.breadcrumb.Replace(">", "&gt;") + (subject.breadcrumb != "" ? "&gt;" : "") + subject.title;
                item["title"] = subject.title.Capitalize();
                html.Append(item.Render() + "\n");
            });

            list["subjects-list"] = html.ToString();

            inject.html = list.Render();
            return inject;
        }
        #endregion
    }
}
