namespace Collector.Services.Dashboard
{
    public class Articles : Service
    {

        public struct AnalyzedArticle
        {
            public string url;
            public string pageTitle;
            public AnalyzedWord[] words;
            public AnalyzedImage[] images;
            public string title;
            public string body;
            public string summary;
            public string publisher;
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
            public int ranking;
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


        public Articles(Core CollectorCore, string[] paths):base(CollectorCore, paths)
        {
        }

        public AnalyzedArticle Analyze(string url)
        {
            AnalyzedArticle analyzed = new AnalyzedArticle();

            return analyzed;
        }

    }
}
