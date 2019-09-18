using Microsoft.AspNetCore.Http;
using Utility.Serialization;

namespace Collector.Services
{
    public class Feeds : Service
    {
        public Feeds(HttpContext context, Parameters parameters) : base(context, parameters)
        {
        }

        public string Add(string title, string url, int intervals = 720, string filter = "")
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            try
            {
                return Serializer.WriteObjectToString(new
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
