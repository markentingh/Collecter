using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Collector.Services
{
    public class Subjects : Service
    {
        public Subjects(HttpContext context, Parameters parameters) : base(context, parameters) { }

        public string AddSubjects(string subjects, string hierarchy)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            var subjectList = subjects.Replace(" , ", ",").Replace(", ", ",").Replace(" ,", ",").Split(',');
            var hier = hierarchy != "" ? hierarchy.Replace(" > ", ">").Replace("> ", ">").Replace(" >", ">").Split('>') : new string[] { };
            try
            {
                Common.Platform.Subjects.Add(subjectList, hier);
                
                var parentId = 0;
                if (hierarchy.Length > 0)
                {
                    var parentHier = hier.ToList();
                    var parentTitle = hier[hier.Length - 1];
                    parentHier.RemoveAt(parentHier.Count - 1);
                    var parentBreadcrumb = string.Join(">", parentHier);
                    var subject = Query.Subjects.GetSubjectByTitle(parentTitle, parentBreadcrumb);
                    parentId = subject.subjectId;
                }


                return Inject(Common.Platform.Subjects.RenderList(parentId, false, hier.Length > 0));
            }
            catch (LogicException ex)
            {
                return Error(ex.Message);
            }
        }
        
        public string LoadSubjectsUI(int parentId = 0, bool getHierarchy = false, bool isFirst = false)
        {
            return Inject(Common.Platform.Subjects.RenderList(parentId, getHierarchy, isFirst));
        }
    }
}
