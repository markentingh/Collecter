using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Collector.Utility.DOM;

namespace Collector.Services.Dashboard
{
    public class Articles : Service
    {

        public struct AnalyzedArticle
        {
            public string rawHtml;
            public string url;
            public string domain;
            public string subject;
            public string pageTitle;
            public AnalyzedTags tags;
            public List<AnalyzedTag> tagNames;
            public List<AnalyzedWord> words;
            public List<AnalyzedImage> images;
            public List<AnalyzedFile> files;
            public string title;
            public string body;
            public string summary;
            public AnalyzedAuthor author;
            public string publishDate;
        }

        public struct AnalyzedTags
        {

            public List<DomElement> elements;
            public List<DomElement> text;
            public List<DomElement> anchorLinks;
            public List<DomElement> headers;
        }

        public struct AnalyzedText
        {
            public DomElement element;
            public int index;
            public enumTextType type;
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

        public struct AnalyzedParentIndex
        {
            public List<DomElement> elements;
            public int index;
            public int textLength;
        }

        public enum enumTextType
        {
            useless = -1,
            mainArticle = 0,
            authorName = 1,
            publishDate = 2,
            comment = 3,
            advertisement = 4,
            linkTitle = 5,
            menuTitle = 6,
            header = 7,
            copyright = 8,
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

            if (1 == 0) //!!! offline debug only !!!
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

            //!!! offline debug only !!!
            htm = File.ReadAllText(S.Server.MapPath("/content/webpages/man-who-shaped-tomorrow-peter-muller-munk.txt"));

            //save raw html
            analyzed.rawHtml = htm;

            //remove spaces, line breaks, & tabs
            htm = htm.Replace("\n", "").Replace("\r","").Replace("  ", " ").Replace("  ", " ").Replace("	","");

            //separate tags into hierarchy of objects
            var parsed = new Parser(S, htm);

            //sort elements into different lists
            var textElements = new List<DomElement>();
            var anchorElements = new List<DomElement>();
            var headerElements = new List<DomElement>();
            var parentIndexes = new List<AnalyzedParentIndex>();
            DomElement traverseElement;

            foreach (DomElement element in parsed.Elements)
            {
                switch(element.tagName.ToLower())
                {
                    case "#text":
                        //sort text elements
                        textElements.Add(element);
                        traverseElement = element;
                        //add element's parent indexes to list
                        do
                        {
                            var parentIndex = parentIndexes.FindIndex(x => x.index == traverseElement.parent);
                            if (parentIndex >= 0)
                            {
                                //update existing parent index
                                var parent = parentIndexes[parentIndex];
                                parent.elements.Add(element);
                                parent.textLength += element.text.Length;
                                parentIndexes[parentIndex] = parent;
                            }
                            else
                            {
                                //create new parent index
                                var parent = new AnalyzedParentIndex();
                                parent.index = traverseElement.parent;
                                parent.elements = new List<DomElement>();
                                parent.elements.Add(element);
                                parent.textLength = element.text.Length;
                                parentIndexes.Add(parent);
                            }
                            if(traverseElement.parent > -1)
                            {
                                //get next parent element
                                traverseElement = parsed.Elements[traverseElement.parent];
                            }
                            else { break; }
                        } while (traverseElement.parent > -1);

                        break;
                    case "a":
                        //sort anchor links
                        anchorElements.Add(element);
                        break;
                    case "h1": case "h2": case "h3": case "h4": case "h5": case "h6":
                        headerElements.Add(element);
                        break;
                }
            }

            //sort lists
            var textOrdered = textElements.OrderBy(p => p.text.Length * -1).ToList();
            var headersOrdered = headerElements.OrderBy(p => p.tagName).ToList();
            var parentIndexesOrdered = parentIndexes.OrderBy(p => (p.elements.Count * p.textLength) * -1).ToList();

            //get a list of parent element indexes from list of text
            analyzed.tags.elements = parsed.Elements;
            analyzed.tags.text = textOrdered;
            analyzed.tags.headers = headersOrdered;
            analyzed.tags.anchorLinks = anchorElements;
            

            return analyzed;
        }

    }
}
