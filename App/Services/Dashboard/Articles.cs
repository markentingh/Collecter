using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

namespace Collector.Services.Dashboard
{
    public class Articles : Service
    {

        public struct AnalyzedArticle
        {
            public string url;
            public string domain;
            public string subject;
            public string pageTitle;
            public AnalyzedTag[] tags;
            public AnalyzedWord[] words;
            public AnalyzedImage[] images;
            public AnalyzedFile[] files;
            public string title;
            public string body;
            public string summary;
            public AnalyzedAuthor author;
            public string publishDate;
        }

        public struct AnalyzedWord
        {
            public string word;
            public AnalyzedWord[] relations;
            public int count;
            public int importance;
            public enumWordType grammar;
        }

        public struct AnalyzedImage
        {
            public string url;
            public int relavance;
        }

        public struct AnalyzedAuthor
        {
            public string name;
            public enumAuthorSex sex;
        }

        public struct AnalyzedTag
        {
            public string name;
            public int count;
            public int[] index;
        }

        public struct AnalyzedFile
        {
            public string filename;
            public string fileType;
        }

        public enum enumWordType
        {
            none = 0,
            person = 1,
            place = 2,
            thing = 3,
            verb = 4,
            adjective = 5
        }

        public enum enumAuthorSex
        {
            female = 0,
            male = 1
        }


        public Articles(Core CollectorCore, string[] paths):base(CollectorCore, paths)
        {
        }

        public AnalyzedArticle Analyze(string url)
        {
            AnalyzedArticle analyzed = new AnalyzedArticle();

            //first, download url
            string htm = "";

            if (1 == 0)
            {
                try
                {
                    using (var http = new HttpClient())
                    {
                        Task<string> response = http.GetStringAsync(url);
                        htm = response.Result;
                    }
                }
                catch (System.Exception ex)
                {
                    //open local file instead
                    htm = File.ReadAllText(S.Server.MapPath("/content/webpages/man-who-shaped-tomorrow-peter-muller-munk.txt"));
                }
            }
            
            htm = File.ReadAllText(S.Server.MapPath("/content/webpages/man-who-shaped-tomorrow-peter-muller-munk.txt"));

            //remove spaces, line breaks, & tabs
            htm = htm.Replace("\n", "").Replace("\r","").Replace("  ", " ").Replace("  ", " ").Replace("	","");

            //separate tags into hierarchy of objects
            var elements = new Utility.DOM.Parser(S, htm);

            return analyzed;
        }

    }
}
