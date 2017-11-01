using System;
using System.Text;

namespace Collector.Pages
{
    public class Home : Page
    {
        public Home(Core CollectorCore) : base(CollectorCore)
        {
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            var html = new StringBuilder();
            html.Append(Redirect("/login"));
            return base.Render(path, html.ToString(), metadata);
        }
    }
}
