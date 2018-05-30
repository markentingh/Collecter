using Microsoft.AspNetCore.Http;

namespace Collector.Services
{
    public class Subjects : Service
    {
        public Subjects(HttpContext context) : base(context)
        {
        }

        public string AddSubjects(string subjects, string hierarchy)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            var subjectList = subjects.Replace(" , ", ",").Replace(", ", ",").Replace(" ,", ",").Split(',');
            var hier = hierarchy.Replace(" > ", ">").Replace("> ", ">").Replace(" >", ">").Split('>');
            try
            {
                Common.Platform.Subjects.AddSubjects(subjectList, hier);
            }
            catch (ServiceErrorException ex)
            {
                return Error(ex.Message);
            }
            return Success();
        }

        public string LoadSubjectsUI(int parentId = 0, bool getHierarchy = false, bool isFirst = false)
        {
            return Inject(Common.Platform.Subjects.RenderSubjectsList(parentId, getHierarchy, isFirst));
        }
    }
}
