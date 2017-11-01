using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace Collector.Services
{
    public class Subjects : Service
    {
        public Subjects(Core CollectorCore) : base(CollectorCore)
        {
        }

        public string AddSubjects(string subjects, string hierarchy, bool loadUI = false)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            var query = new Query.Subjects(S.Server.sqlConnection);
            var subjectList = subjects.Replace(" , ", ",").Replace(", ", ",").Replace(" ,", ",").Split(',');
            var parentId = 0;
            var breadcrumb = "";
            if (hierarchy != "")
            {
                var hier = hierarchy.Replace(" > ", ">").Replace("> ", ">").Replace(" >", ">").Split('>');
                var parentTitle = "";
                var parentBreadcrumb = "";
                if (hier.Length > 0)
                {
                    var parentHier = hier.ToList();
                    parentTitle = hier[hier.Length - 1];
                    parentHier.RemoveAt(parentHier.Count - 1);
                    parentBreadcrumb = string.Join(">", parentHier);
                    breadcrumb = string.Join(">", hier);
                }
                var subject = query.GetSubjectByTitle(parentTitle, parentBreadcrumb);
                parentId = subject.subjectId;
            }
            foreach (string subject in subjectList)
            {
                query.CreateSubject(parentId, 0, 0, subject, breadcrumb);
            }
            if (loadUI == true)
            {
                //create UI for selected subject's list
                if (parentId > 0)
                {
                    var inject = InjectSubjectsUI(parentId, true, true);
                    return inject.html;
                }
                else
                {
                    var inject = InjectSubjectsUI(parentId);
                    return inject.html;
                }
            }

            return Success();
        }

        public string LoadSubjectsUI(int parentId = 0, bool getHierarchy = false, bool isFirst = false)
        {
            return Inject(InjectSubjectsUI(parentId, getHierarchy, isFirst));
        }
        
        public structInject InjectSubjectsUI(int parentId = 0, bool getHierarchy = false, bool isFirst = false)
        {
            var inject = new structInject() { };
            if (!CheckSecurity()) { inject.html = AccessDenied(); return inject; }

            var html = new StringBuilder();
            var query = new Query.Subjects(S.Server.sqlConnection);
            var list = new Scaffold(S, "/Services/Subjects/subject.html");
            var item = new Scaffold(S, "/Services/Subjects/list-item.html");
            var subjects = query.GetList("", parentId);
            var indexes = new string[] { };
            if (parentId > 0)
            {
                var details = query.GetSubjectById(parentId);
                if (details == null) { inject.html = Error(); return inject; }

                //set up subject
                var crumb = details.breadcrumb.Replace(">", "&gt;");
                if(details.parentId == 0) { crumb = details.title; } else { crumb += " &gt; " + details.title; }
                indexes = details.hierarchy.Split('>');
                list.Data["parentId"] = details.subjectId.ToString();
                list.Data["no-words"] = details.haswords == false ? "1" : "";
                list.Data["breadcrumbs"] = crumb;

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
                    inject.javascript = "S.subjects.buttons.selectSubject(" + parentId + "," + pId + ",'" + bread + "', 0, true);";

                    //get inject object for parent within hierarchy
                    var parent = InjectSubjectsUI(indexes.Length > 1 ? int.Parse(indexes[indexes.Length - 2]) : 0, indexes.Length > 1 ? true : false);
                    html.Append(parent.html + "\n");
                    inject.javascript += parent.javascript;
                }
            }
            else
            {
                list.Data["parentId"] = "0";
            }
            

            //set up subject sub-items
            subjects.ForEach((Query.Models.Subject subject) =>
            {
                var breadcrumbs = subject.breadcrumb;
                if(breadcrumbs == "") { breadcrumbs = subject.title; }
                item.Data["subjectId"] = subject.subjectId.ToString();
                item.Data["parentId"] = subject.parentId.ToString();
                item.Data["breadcrumbs"] = subject.breadcrumb.Replace(">", "&gt;");
                item.Data["title"] = S.Util.Str.Capitalize(subject.title);
                html.Append(item.Render() + "\n");
            });
            
            list.Data["subjects-list"] = html.ToString();

            inject.html = list.Render();
            return inject;
        }
    }
}
