using System;
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

        #region "Article Structure"
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

        public struct AnalyzedElementCount
        {
            public int index;
            public int count;
        }

        public struct PossibleTextType
        {
            public enumTextType type;
            public int count;
        }

        public struct ArticleSubject
        {
            public int id;
            public int parentId;
            public string title;
            public enumWordType type;
            public string[] breadcrumb;
            public int[] hierarchy;
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
            verb = 1,
            adverb = 2,
            noun = 3,
            pronoun = 4,
            adjective = 5,
            article = 6,
            preposition = 7,
            conjunction = 8,
            interjection = 9,
            punctuation = 10
        }

        public enum enumAuthorSex
        {
            female = 0,
            male = 1
        }

        private int textTypesCount = 13;

        #endregion

        #region "Dictionaries"

        private string[] wordSeparators = new string[] { "(", ")", ".", ",", "?", "/", "\\", "|", "!", "\"", "'", "&nbsp;", ";", ":", "[", "]", "{", "}", "”", "“", "—", "_" };

        private string[] scriptSeparators = new string[] { "{", "}", ";", "$", "=", "(", ")" };

        private string[] dateTriggers = new string[] {
            "published","written","posted",
            "january","february","march","april","may","june",
            "july","august","september","october","november","december" };

        private string[] badTags = new string[]  {
            "applet", "area", "audio", "canvas", "dialog", "small",
            "embed", "footer", "iframe", "input", "label", "nav",
            "object", "option", "s", "script", "style", "textarea",
            "video" };

        private string[] badArticleTags = new string[]  {
            "applet", "area", "audio", "canvas", "dialog", "small",
            "embed", "footer", "iframe", "input", "label", "nav",
            "object", "option", "s", "script", "style", "textarea",
            "video", "form", "figure", "figcaption" };

        private string[] badClasses = new string[] { "aside", "sidebar", "advert", "menu", "comment", "tag", "keyword" };

        private string[] badPhotoCredits = new string[] { "photo", "courtesy", "by", "copyright" };

        private string[] badMenu = new string[] { "previous", "next", "post", "posts", "entry", "entries", "article", "articles", "more", "back", "go", "view", "about", "home", "blog" };

        private string[] badChars = new string[] { "|", ":", "{", "}", "[", "]" };

        #endregion

        public Articles(Core CollectorCore, string[] paths):base(CollectorCore, paths)
        {
        }

        #region "Analyze Article"
        public AnalyzedArticle Analyze(string url)
        {
            AnalyzedArticle analyzed = new AnalyzedArticle();
            analyzed.tags = new AnalyzedTags();
            analyzed.tags.text = new List<AnalyzedText>();
            analyzed.author = new AnalyzedAuthor();
            analyzed.body = new List<int>();
            analyzed.url = url;

            //first, download url
            string htm = "";

            if (url.IndexOf("local") == 0) //!!! offline debug only !!!
            {
                htm = File.ReadAllText(S.Server.MapPath("/wwwroot/tests/" + url.Replace("local ","") + ".html"));
                analyzed.url = "/tests/" + url.Replace("local ", "") + ".html";
            }
            else
            { 
                try
                {
                    using (var http = new HttpClient())
                    {
                        Task<string> response = http.GetStringAsync(url);
                        htm = response.Result;
                        File.WriteAllText(S.Server.MapPath("/wwwroot/tests/new.html"), htm);
                    }
                }
                catch (System.Exception ex)
                {
                    //open local file instead
                    htm = File.ReadAllText(S.Server.MapPath("/wwwroot/tests/1.html"));
                    analyzed.url = "/tests/1.html";
                }
            }

            //save raw html
            analyzed.rawHtml = htm;

            //remove spaces, line breaks, & tabs
            htm = htm.Replace("\n", "").Replace("\r","").Replace("  ", " ").Replace("  ", " ").Replace("	","").Replace(" "," ");

            //separate tags into hierarchy of objects
            var parsed = new Parser(S, htm);
            var tagNames = new List<AnalyzedTag>();

            //sort elements into different lists
            var textElements = new List<DomElement>();
            var anchorElements = new List<DomElement>();
            var headerElements = new List<DomElement>();
            var imgElements = new List<DomElement>();
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
                    case "img":
                        imgElements.Add(element);
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

            //setup analyzed headers
            var headers = new List<int>();
            foreach (DomElement header in headersOrdered)
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

            //analyze sorted text /////////////////////////////////////////////////////////////////////////////////////////////////////////
            var textblock = "";
            string[] texts;
            AnalyzedWord word;
            AnalyzedWordInText wordIn;
            var index = 0;
            var i = -1;
            var allWords = new List<AnalyzedWord>();
            var words = new List<AnalyzedWord>();
            foreach (DomElement element in textOrdered)
            {
                
                var wordsInText = new List<AnalyzedWordInText>();
                var newText = new AnalyzedText();
                newText.index = element.index;
                //separate all words & symbols with a space
                textblock = S.Util.Str.replaceAll(element.text, " {1} ", wordSeparators).Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
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
                    }
                    else
                    {
                        //add new word to list
                        word = new AnalyzedWord();
                        word.word = texts[x];
                        word.count = 1;
                        allWords.Add(word);
                    }

                    //set reference to analyzed word
                    wordsInText.Add(wordIn);
                }
                newText.words = wordsInText;

                //check all words for patterns to determine
                //what type of text is in this element
                i = -1;
                var sortedAllWords = allWords.OrderBy(a => a.count * -1).ToList();
                var len = wordsInText.Count;
                var possibleTypes = new int[textTypesCount+1];

                foreach (AnalyzedWordInText aword in wordsInText)
                {
                    i++;
                    var w = aword.word.ToLower();
                    if (scriptSeparators.Contains(w))
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
                    else if (dateTriggers.Contains(w))
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
                        if(t.type != enumTextType.script)
                        {
                            //script type takes presidence over other types
                            if(e < sortedPossTypes.Count - 1)
                            {
                                if(sortedPossTypes[e+1].type == enumTextType.script)
                                {
                                    if(sortedPossTypes[e + 1].count >= 5)
                                    {
                                        found = false;
                                    }
                                }
                            }
                        }

                    }
                    if(found == true)
                    {
                        newText.type = t.type;
                        break;
                    }
                }
                
                //add text to analyzed results
                analyzed.tags.text.Add(newText);
            }
            //end: analyze sorted text

            //find article body from sorted text /////////////////////////////////////////////////////////////////////////////////////////////////////////
            var pIndexes = new List<AnalyzedElementCount>();
            i = 0;
            foreach (var a in analyzed.tags.text)
            {
                
                if(a.type == enumTextType.mainArticle)
                {
                    //find most relevant parent indexes shared by all article text elements
                    i++;

                    //limit to large article text
                    if (a.words.Count < 20 && i > 2) { i--;  break; }

                    //limit to top 3 article text
                    if (i > 3) { i--;  break; } 

                    foreach (int indx in parsed.Elements[a.index].hierarchyIndexes)
                    {
                        var parindex = pIndexes.FindIndex(p => p.index == indx);
                        if (parindex >= 0)
                        {
                            //update count for a parent index
                            var p = pIndexes[parindex];
                            p.count++;
                            pIndexes[parindex] = p;
                        }
                        else
                        {
                            //add new parent index
                            var p = new AnalyzedElementCount();
                            p.index = indx;
                            p.count = 1;
                            pIndexes.Add(p);
                        }
                    }
                }
            }
            var sortedArticleParents = pIndexes.OrderBy(p => p.index).OrderBy(p => p.count * -1).ToList();

            //determine best parent element that contains the entire article
            var bodyText = new List<int>();
            var uselessText = new[] {
                enumTextType.advertisement,
                enumTextType.comment,
                enumTextType.copyright,
                enumTextType.script,
                enumTextType.style,
                enumTextType.useless
            };

            int parentId;
            var isFound = false;
            var isBad = false;
            var badIndexes = new List<int>();
            string[] articleText;
            DomElement hElem;

            for (var x = sortedArticleParents.Count - 1; x >= 0; x--)
            {
                if(sortedArticleParents[x].count >= i)
                {
                    //all elements are a part of this parent element
                    //get a list of text elements that are a part of the 
                    //parent element
                    parentId = sortedArticleParents[x].index;
                    isFound = false;
                    isBad = false;
                    for (var y = parentId + 1; y < parsed.Elements.Count; y++)
                    {
                        var elem = parsed.Elements[y];
                        
                        if (elem.hierarchyIndexes.Contains(parentId))
                        {
                            if(elem.hierarchyIndexes.Where(ind => badIndexes.Contains(ind)).Count() > 0) { continue; }
                            if (elem.tagName == "#text")
                            {
                                //element is text & is part of parent index
                                //check if text type is article
                                var textTag = analyzed.tags.text.Find(p => p.index == y);
                                if(!uselessText.Contains(textTag.type))
                                {
                                    articleText = S.Util.Str.replaceAll(elem.text.ToLower(), " {1} ", wordSeparators).Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Split(' ').Select(t => t.Trim()).Where(t => t.Length > 0 && t != "&").ToArray();
                                    
                                    
                                    //check for any text from the article that does not belong,
                                    //such as advertisements, side-bars, photo credits, widgets
                                    isBad = false;

                                    for(var z = elem.hierarchyIndexes.Length - 1; z >= 0; z--)
                                    {
                                        //search down the hierarchy DOM tree
                                        hElem = analyzed.elements[elem.hierarchyIndexes[z]];
                                        if(hElem.index == parentId) { break; }
                                        
                                        //check for bad tag names
                                        if(badTags.Contains(hElem.tagName)) { isBad = true; }

                                        //check classes for bad element indicators within class names
                                        if (hElem.className.Count > 0)
                                        {
                                            if (hElem.className.Where(c => badClasses.Contains(c)).Count() > 0) { isBad = true; }
                                        }
                                    }

                                    if(articleText.Length <= 20)
                                    {
                                        if (articleText.Where(t => badPhotoCredits.Contains(t)).Count() >= 2)
                                        {
                                            //photo credits
                                            isBad = true;
                                        }
                                    }
                                    if(articleText.Length <= 7)
                                    {
                                        if(articleText.Where(t => badMenu.Contains(t)).Count() >= 1)
                                        {
                                            //menu
                                            isBad = true;
                                        }
                                    }

                                    if(articleText.Length <= 3)
                                    {
                                        if(articleText.Where(t => badChars.Contains(t)).Count() >= 1)
                                        {
                                            //bad characters
                                            isBad = true;
                                        }
                                    }

                                    if(isBad == false)
                                    {
                                        //finally, add text to article
                                        bodyText.Add(elem.index);

                                        //clean up text in element
                                        elem.text = S.Util.Str.HtmlDecode(elem.text);
                                    }
                                }
                            }
                            else
                            {
                                //element is not text, 
                                //determine if element contains bad content
                                if (elem.className.Where(c => badClasses.Where(bc => c.IndexOf(bc) >= 0).Count() > 0).Count() > 0)
                                {
                                    badIndexes.Add(elem.index);
                                }
                                else if (elem.attribute.ContainsKey("id"))
                                {
                                    if (badClasses.Where(bc => elem.attribute["id"].IndexOf(bc) >= 0).Count() > 0)
                                    {
                                        badIndexes.Add(elem.index);
                                    }
                                }
                                else if (badArticleTags.Contains(elem.tagName))
                                {
                                    badIndexes.Add(elem.index);
                                }

                            }
                        }
                        else
                        {
                            //no longer part of parent id
                            isFound = true;
                            break;
                        }
                    }
                    if(isFound == true) { break; }
                }
            }
           
            analyzed.body = bodyText;

            //analyze all words from article body //////////////////////////////////////////////////////////////////
            var text = "";
            var commonWords = GetCommonWords();
            allWords = new List<AnalyzedWord>();
            for (var x = 0; x < analyzed.body.Count; x++)
            {
                text += analyzed.elements[analyzed.body[x]].text + " ";
            }
            textblock = S.Util.Str.replaceAll(S.Util.Str.HtmlDecode(text.ToLower()), " {1} ", wordSeparators).Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            texts = textblock.Split(' ');
            for (var x = 0; x < texts.Length; x++)
            {
                if (texts[x].Trim() == "") { continue; }

                //add word to all words list
                index = allWords.FindIndex(w => w.word == texts[x]);
                if (index >= 0)
                {
                    //incriment word count
                    var w = allWords[index];
                    w.count++;
                    allWords[index] = w;
                }
                else
                {
                    //add new word to list
                    word = new AnalyzedWord();
                    word.word = texts[x];
                    word.count = 1;
                    word.importance = 1;
                    if (commonWords.Contains(texts[x].Trim().ToLower())){
                        word.importance = 0;
                    }
                    allWords.Add(word);
                }
            }


            analyzed.words = allWords.OrderByDescending(w => w.word.Length).OrderByDescending(w => w.importance).ToList();
            
            return analyzed;
        }
        #endregion

        #region "Subjects"
        public void AddSubject(string subject, string grammertype, string hierarchy)
        {
            int parentId = 0;
            if(hierarchy != "")
            {
                var hier = hierarchy.Replace(" > ", ">").Replace("> ", ">").Replace(" >", ">").Split('>');
                var parentTitle = "";
                if (hier.Length > 0) { parentTitle = hier[hier.Length - 1]; }
                var reader = S.Sql.ExecuteReader("EXEC GetSubject @title='" + "" + "', @breadcrumb='" + "" + "'");
                //if(reader.R)
            }
        }

        public List<ArticleSubject> GetSubjects(string[] subject)
        {
            var subjects = new List<ArticleSubject>();

            return subjects;
        }

        public void AddCommonWord(string wordlist)
        {
            var commonWords = GetCommonWords();
            if(wordlist == ",") { if (!commonWords.Contains(",")) { commonWords.Add(","); } }
            var words = wordlist.Split(',');
            foreach (string word in words)
            {
                var w = word.Trim().ToLower();
                if (w == "") { continue; }
                if (!commonWords.Contains(w)) { commonWords.Add(w); }
            }

            S.Server.Cache["commonwords"] = commonWords;
            S.Util.Serializer.SaveToFile(commonWords, S.Server.MapPath("/content/commonwords.json"));
        }

        public List<string> GetCommonWords()
        {
            var commonWords = new List<string>();
            if (S.Server.Cache.ContainsKey("commonwords"))
            {
                commonWords = (List<string>)S.Server.Cache["commonwords"];
            }
            else if (File.Exists(S.Server.MapPath("/content/commonwords.json")))
            {
                commonWords = (List<string>)S.Util.Serializer.OpenFromFile(typeof(List<string>), S.Server.MapPath("/content/commonwords.json"));
            }
            return commonWords;
        }
        #endregion
    }
}
