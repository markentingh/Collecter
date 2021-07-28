using System.Text.Json;

namespace Collector.Services
{
    public class Feeds : Service
    {
        public string Add(string title, string url, int intervals = 720, string filter = "")
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            try
            {
                return JsonSerializer.Serialize(new
                    {
                        feedId = Query.Feeds.Add(title, url, filter, intervals)
                    }
                );
            }
            catch (LogicException ex)
            {
                return Error(ex.Message);
            }
        }
    }
}
