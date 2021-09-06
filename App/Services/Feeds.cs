using System.Text.Json;

namespace Collector.Services
{
    public class Feeds : Service
    {
        public string Add(int categoryId, string title, string url, int intervals = 720, string filter = "")
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            try
            {
                Query.Feeds.Add(categoryId, title, url, filter, intervals);
                return Success();
            }
            catch (LogicException ex)
            {
                return Error(ex.Message);
            }
        }

        public string AddCategory(string title)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            try
            {
                Query.Feeds.AddCategory(title);
                var categories = Query.Feeds.GetCategories();
                return Common.Platform.Feeds.RenderOptions(categories);
            }
            catch (LogicException ex)
            {
                return Error(ex.Message);
            }
        }
    }
}
