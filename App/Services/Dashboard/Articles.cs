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
            public List<DomElement> elements;
            public AnalyzedTags tags;
            public List<AnalyzedTag> tagNames;
            public List<AnalyzedParentIndex> parentIndexes;
            public List<AnalyzedWord> words;
            public string title;
            public List<int> body;
            public string summary;
            public AnalyzedAuthor author;
            public string publishDate;
        }

        public struct AnalyzedTags
        {
            public List<AnalyzedText> text;
            public List<int> anchorLinks;
            public List<int> headers;
        }

        public struct AnalyzedTag
        {
            public string name;
            public int count;
            public int[] index;
        }

        public struct AnalyzedText
        {
            public int index;
            public List<AnalyzedWordInText> words;
            public enumTextType type;
            public List<PossibleTextType> possibleTypes;
        }

        public struct AnalyzedWord
        {
            public string word;
            public int count;
            public int importance;
            public enumWordType type;
        }

        public struct AnalyzedWordInText
        {
            public string word;
            public AnalyzedWord[] relations;
            public int index;
            public int wordIndex;
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

        public struct AnalyzedFile
        {
            public string filename;
            public string fileType;
        }

        public struct AnalyzedParentIndex
        {
            public List<int> elements;
            public int index;
            public int textLength;
        }

        public struct PossibleTextType
        {
            public enumTextType type;
            public int count;
        }

        public enum enumTextType
        {
            mainArticle = 0,
            authorName = 1,
            publishDate = 2,
            comment = 3,
            advertisement = 4,
            linkTitle = 5,
            menuTitle = 6,
            header = 7,
            copyright = 8,
            script = 9,
            useless = 10,
            style = 11,
            anchorLink = 12,
            menuItem = 13
        }

        public enum enumWordType
        {
            none = 0,
            person = 1,
            place = 2,
            thing = 3,
            verb = 4,
            adjective = 5,
            preposition = 6,
            conjunction = 7,
            interjection = 8,
            punctuation = 9
        }

        public enum enumAuthorSex
        {
            female = 0,
            male = 1
        }

        private int textTypesCount = 13;

        public Articles(Core CollectorCore, string[] paths):base(CollectorCore, paths)
        {
        }

        public AnalyzedArticle Analyze(string url)
        {
            AnalyzedArticle analyzed = new AnalyzedArticle();
            analyzed.tags = new AnalyzedTags();
            analyzed.tags.text = new List<AnalyzedText>();
            analyzed.author = new AnalyzedAuthor();
            analyzed.body = new List<int>();

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
            var tagNames = new List<AnalyzedTag>();

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
                                parent.elements.Add(element.index);
                                parent.textLength += element.text.Length;
                                parentIndexes[parentIndex] = parent;
                            }
                            else
                            {
                                //create new parent index
                                var parent = new AnalyzedParentIndex();
                                parent.index = traverseElement.parent;
                                parent.elements = new List<int>();
                                parent.elements.Add(element.index);
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
                    case "title":
                        if(element.index < parsed.Elements.Count - 2)
                        {
                            analyzed.pageTitle = parsed.Elements[element.index + 1].text.Trim();
                        }
                        break;
                }

                //get a count of each tag name
                var tagIndex = tagNames.FindIndex(x => x.name == element.tagName);
                if(tagIndex >= 0)
                {
                    //incriment tag name count
                    var t = tagNames[tagIndex];
                    t.count++;
                    tagNames[tagIndex] = t;
                }
                else
                {
                    //add new tag to list
                    var t = new AnalyzedTag();
                    t.name = element.tagName;
                    t.count = 1;
                    tagNames.Add(t);
                }
            }

            //sort lists
            var textOrdered = textElements.OrderBy(p => p.text.Length * -1).ToList();
            var headersOrdered = headerElements.OrderBy(p => p.tagName).ToList();
            var parentIndexesOrdered = parentIndexes.OrderBy(p => (p.elements.Count * p.textLength) * -1).ToList();
            var tagNamesOrdered = tagNames.OrderBy(t => t.count * -1).ToList();

            //set up elements
            analyzed.elements = parsed.Elements;
            analyzed.tagNames = tagNamesOrdered;

            //analyze sorted text /////////////////////////////////////////////////////////////////////////////////////////////////////////
            var textblock = "";
            string[] texts;
            AnalyzedWord word;
            AnalyzedWordInText wordIn;
            var index = 0;
            var wordIndex = 0;
            var words = new List<AnalyzedWord>();
            foreach (DomElement element in textOrdered)
            {
                var allWords = new List<AnalyzedWord>();
                var wordsInText = new List<AnalyzedWordInText>();
                var newText = new AnalyzedText();
                newText.index = element.index;
                //separate all words & symbols with a space
                textblock = S.Util.Str.replaceAll(element.text, " {1} ",
                    new string[] { "(", ")", ".", ",", "?", "/", "\\", "|", "!", "\"", "'", ";", ":", "[", "]", "{", "}", "”", "“" }
                    ).Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                texts = textblock.Split(' ');
                for (var x = 0; x < texts.Length; x++)
                {
                    if(texts[x].Trim() == "") { continue; }
                    wordIn = new AnalyzedWordInText();
                    wordIn.word = texts[x];
                    wordIn.index = x;

                    //add word to all words list
                    index = allWords.FindIndex(w => w.word == texts[x]);
                    if (index >= 0)
                    {
                        //incriment word count
                        var w = allWords[index];
                        w.count++;
                        allWords[index] = w;
                        wordIndex = index;
                    }
                    else
                    {
                        //add new word to list
                        word = new AnalyzedWord();
                        word.word = texts[x];
                        word.count = 1;
                        wordIndex = allWords.Count;
                        allWords.Add(word);
                    }

                    //set reference to analyzed word
                    wordIn.wordIndex = wordIndex;
                    wordsInText.Add(wordIn);
                }
                newText.words = wordsInText;

                //check all words for patterns to determine
                //what type of text is in this element
                var sortedAllWords = allWords.OrderBy(a => a.count * -1).ToList();
                var i = -1;
                var len = wordsInText.Count;
                var possibleTypes = new int[textTypesCount+1];
                int w_nouns = 0, w_verbs = 0, w_adj = 0, w_pronouns = 0;

                foreach (AnalyzedWordInText aword in wordsInText)
                {
                    i++;
                    var w = aword.word.ToLower();
                    if (w == "{" || w == "}" || w == ";" || w == "$" || w == "=" || w == "(" || w == ")")
                    {
                        if (parsed.Elements[element.index - 1].tagName == "script")
                        {
                            possibleTypes[(int)enumTextType.script] += 5;
                            break;
                        }
                        else if (parsed.Elements[element.index - 1].tagName == "style")
                        {
                            possibleTypes[(int)enumTextType.style] += 5;
                            break;
                        }
                    }
                    else if (w == "copyright")
                    {
                        possibleTypes[(int)enumTextType.copyright] += 1;
                    }
                    else if (w.IndexOf("&copy") >= 0 || w == "©")
                    {
                        possibleTypes[(int)enumTextType.copyright] += 2;
                    }
                    else if (w == "rights" || w == "reserved")
                    {
                        possibleTypes[(int)enumTextType.copyright] += 1;
                    }
                    else if (w == "published" || w == "written" || w == "posted" ||
                        w == "january" || w == "february" || w == "march" || w == "april" || w == "may" || w == "june" ||
                        w == "july" || w == "august" || w == "september" || w == "october" || w == "november" || w == "december")
                    {
                        if (len < 20)
                        {
                            //small group of text has better chance 
                            //of being a publish date
                            possibleTypes[(int)enumTextType.publishDate] += 5;
                        }
                        else
                        {
                            possibleTypes[(int)enumTextType.publishDate] += 1;
                        }
                    }
                    else if (w.IndexOf("advertis") >= 0 || w.IndexOf("sponsor") >= 0)
                    {
                        if (len < 10)
                        {
                            //small group of text has better chance 
                            //of being an advertisement
                            possibleTypes[(int)enumTextType.advertisement] += 5;
                        }
                        else
                        {
                            possibleTypes[(int)enumTextType.advertisement] += 1;
                        }
                    } 
                }
                if(element.text.Length >= 4)
                {
                    if (element.text.Trim().IndexOf("<!--") == 0)
                    {
                        possibleTypes[(int)enumTextType.useless] = 10000;
                    }
                }
                if (element.hierarchyTags.Contains("ul") && element.hierarchyTags.Contains("li"))
                {
                    //menu item
                    possibleTypes[(int)enumTextType.menuItem] += 100;
                }
                else if (element.hierarchyTags.Contains("a"))
                {
                    //anchor link
                    possibleTypes[(int)enumTextType.anchorLink] += 100;
                }

                //sort possible types by count
                var possTypes = new List<PossibleTextType>();
                var e = 0;
                for (e = 0; e <= textTypesCount; e++)
                {
                    var newPoss = new PossibleTextType();
                    newPoss.type = (enumTextType)e;
                    newPoss.count = possibleTypes[e];
                    possTypes.Add(newPoss);
                }
                var sortedPossTypes = possTypes.OrderBy(p => p.count * -1).ToList();
                newText.possibleTypes = sortedPossTypes;

                //figure out dominant type from possible types
                for (e = 0; e < sortedPossTypes.Count; e++)
                {
                    var t = sortedPossTypes[e];
                    var found = false;
                    if (t.count > 1)
                    {
                        if(t.type == enumTextType.script)
                        { 
                            if(t.count >= 5){ found = true; }
                        }
                        else if (t.type == enumTextType.style)
                        {
                            if (t.count >= 5) { found = true; }
                        }
                        else if (t.type == enumTextType.useless)
                        {
                            if (t.count >= 5) { found = true; }
                        }
                        else if (t.type == enumTextType.copyright)
                        {
                            if (t.count >= 2) { found = true; }
                        }
                        else if (t.type == enumTextType.publishDate)
                        {
                            if (t.count >= 5) { found = true; }
                        }
                        else if(t.type == enumTextType.anchorLink)
                        {
                            found = true;
                        }
                        else if (t.type == enumTextType.menuItem)
                        {
                            found = true;
                        }

                    }
                    if(found == true)
                    {
                        newText.type = t.type;
                        break;
                    }
                }

                //add main article text to body list
                if(newText.type == enumTextType.mainArticle)
                {
                    analyzed.body.Add(newText.index);
                }
                

                //add words to global words
                if(newText.type != enumTextType.script && newText.type != enumTextType.useless)
                {
                    foreach(AnalyzedWord aword in sortedAllWords)
                    {
                        index = words.FindIndex(w => w.word == aword.word);
                        if (index >= 0)
                        {
                            //incriment word count
                            var w = words[index];
                            w.count++;
                            words[index] = w;
                        }
                        else
                        {
                            //add new word to list
                            word = aword;
                            word.count = 1;
                            words.Add(word);
                        }
                    }
                }
                
                //add text to analyzed results
                analyzed.tags.text.Add(newText);
            }
            analyzed.words = words;
            //end: analyze sorted text /////////////////////////////////////////////////////////////////////////////////////////////////////////

            //find relavant article body
            foreach(var a in analyzed.tags.text)
            {
                if(a.type == enumTextType.mainArticle)
                {

                }
            }


            //setup analyzed headers
            var headers = new List<int>();
            foreach(DomElement header in headersOrdered)
            {
                headers.Add(header.index);
            }
            analyzed.tags.headers = headers;

            //setup analyzed links
            var anchors = new List<int>();
            foreach (DomElement anchor in anchorElements)
            {
                anchors.Add(anchor.index);
            }
            analyzed.tags.anchorLinks = anchors;

            return analyzed;
        }

    }
}
