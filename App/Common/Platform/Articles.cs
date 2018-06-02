using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Collector.Common.Platform
{
    public static class Articles
    {
        public static string RenderList(int subjectId = 0, int feedId = -1, string search = "", int start = 1, int length = 50)
        {
            var item = new Scaffold("/Views/Articles/list-item.html", Server.Instance.Scaffold);
            var html = new StringBuilder();

            return html.ToString();
        }
    }
}
