using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Utility.Serialization;
using Utility.DOM;
using Collector.Models.Article;
using Collector.Models.Nodes;
using Utility.Strings;
using Utility;

namespace Collector.Common.Analyze
{
    public static class Html
    {
        #region "Step #1: Get HTML Document"
        public static AnalyzedArticle DeserializeArticle(string dom)
        {
            var article = new AnalyzedArticle();
            try
            {
                //deserialize object from string
                var node = (Document)Serializer.ReadObject(dom, typeof(Document));
                var html = new StringBuilder();
                var hierarchy = new List<int>();                
                var parser = new Parser("");

                //build DOM tree
                var elems = new List<DomElement>();
                var index = 0;
                Traverse(node.dom, ref index, elems, hierarchy, node.a, parser);
                parser.Elements = elems;
                article.elements = elems;
                article.rawHtml = FormatHtml(elems).ToString();
            }
            catch (Exception)
            {
                article.rawHtml = dom;
                throw new Exception();
            }
            

            return article;
        }

        private static DomElement Traverse(Node parent, ref int index, List<DomElement> elems, List<int> hierarchy, string[] attributes, Parser parser)
        {
            //create local copy of hierarchy
            var parentId = -1;
            int[] hier = new int[hierarchy.Count];
            if(hierarchy.Count > 0)
            {
                hierarchy.CopyTo(hier);
                parentId = hierarchy[hierarchy.Count() - 1];
            }
            
            //create DOM element
            var elem = new DomElement(parser);
            elem.index = index;
            elem.parent = parentId;
            elem.tagName = parent.t;
            elem.className = new List<string>();
            elem.hierarchyIndexes = hier.ToArray();
            elem.childIndexes = new List<int>();
            elem.style = new Dictionary<string, string>();
            elem.attribute = new Dictionary<string, string>();

            switch (elem.tagName)
            {
                case "#text": case "br": case "meta": case "link": case "hr": case "img":
                    elem.isSelfClosing = true;
                    break;
            }

            if(elem.tagName == "#text")
            {
                elem.text = parent.v;
            }

            //build style list for element
            if (parent.s != null)
            {
                switch (parent.s[0])
                {
                    case 0: elem.style.Add("display", "none"); break;
                    case 2: elem.style.Add("display", "inline"); break;
                    case 3: elem.style.Add("display", "inline-block"); break;
                }
                elem.style.Add("font-size", parent.s[1].ToString() + "px");
                if (parent.s[2] == 2)
                {
                    elem.style.Add("font-weight", "bold");
                }
                if (parent.s[3] == 1)
                {
                    elem.style.Add("font-style", "italic");
                }
            }

            //build attributes list
            if (parent.a != null)
            {
                foreach (var x in parent.a)
                {
                    if(attributes[x.Key] == "class")
                    {
                        elem.className = x.Value.Replace("  ", " ").Replace("  ", " ").Split(" ").ToList();
                    }
                    else
                    {
                        elem.attribute.Add(attributes[x.Key], x.Value);
                    }
                }
            }

            //append index to hierarchy list
            hier = hier.Append(index).ToArray();

            //add element to DOM elements list
            elems.Add(elem);

            if(parent.c != null)
            {
                //traverse all children
                foreach (var child in parent.c)
                {
                    index++;
                    var childElem = Traverse(child, ref index, elems, hier.ToList(), attributes, parser);
                    elem.childIndexes.Add(childElem.index);
                }
            }

            if(elem.isSelfClosing == false)
            {
                //add closing tag to DOM elements list
                index++;
                var closing = new DomElement(parser);
                closing.index = index;
                closing.tagName = "/" + parent.t;
                closing.hierarchyIndexes = hierarchy.ToArray();
                closing.childIndexes = new List<int>();
                closing.style = new Dictionary<string, string>();
                closing.attribute = new Dictionary<string, string>();
                closing.isClosing = true;
                elems.Add(closing);
            }
            return elem;
        }

        public static StringBuilder FormatHtml(List<DomElement> elements)
        {
            var htms = new StringBuilder();
            var htmelem = "";
            var tabs = "";
            foreach (var el in elements)
            {
                tabs = "";
                if (el.isClosing == true && el.isSelfClosing == false)
                {
                    //closing tag
                    htmelem = "<" + el.tagName + ">";
                }
                else
                {
                    if (el.tagName == "#text")
                    {
                        htmelem = el.text;
                    }
                    else
                    {
                        htmelem = "<" + el.tagName;
                        if(el.className.Count() > 0)
                        {
                            htmelem += " class=\"" + string.Join(" ", el.className) + "\"";
                        }
                        if (el.attribute != null)
                        {
                            if (el.attribute.Count > 0)
                            {
                                foreach (var attr in el.attribute)
                                {
                                    htmelem += " " + attr.Key + "=\"" + attr.Value + "\"";
                                }
                            }
                        }

                        if (el.isSelfClosing == true)
                        {
                            if (el.tagName == "!--")
                            {
                                htmelem += el.text + "-->";
                            }
                            else
                            {
                                htmelem += "/>";
                            }

                        }
                        else
                        {
                            htmelem += ">" + el.text;
                        }
                    }


                }

                for (var x = 0; x < el.hierarchyIndexes.Length - 1; x++)
                {
                    tabs += "    ";
                }
                htms.Append(tabs + htmelem + "\n");
            }
            return htms;
        }
        #endregion

        #region "Step #2: Get Article Info"
        public static void GetArticleInfoFromDOM(AnalyzedArticle article)
        {
            //get title
            article.title = "";
            var title = article.elements.Where(a => a.tagName == "title").FirstOrDefault();
            if(title != null)
            {
                var child = title.FirstChild;
                if(child != null)
                {
                    article.title = child.text;
                }
            }
            if(article.title == "")
            {
                //get title from header tags
                var headers = article.elements.Where(a =>
                {
                    switch (a.tagName)
                    {
                        case "h1": case "h2": case "h3": case "h4": case "h5": case "h6":
                            return true;
                    }
                    return false;
                }).OrderBy(a => a.tagName).ToList();
                if(headers.Count > 0)
                {
                    for(var x = 0; x < headers.Count; x++)
                    {
                        var child = headers[x].FirstChild;
                        if(child != null)
                        {
                            if(child.text != "")
                            {
                                article.title = child.text;
                                break;
                            }
                        }
                    }
                }
            }

            //get page description
            var description = article.elements.Where(a => a.tagName == "meta" && a.attribute.ContainsKey("name") && a.attribute["name"].IndexOf("description") >= 0).FirstOrDefault();
            if(description != null)
            {
                article.summary = description.attribute["content"];
            }
        }
        #endregion

        #region "Step #3: Get Article Contents"
        public static int TraverseIndexes(AnalyzedArticle article, DomElement root, List<int> children, int limitDepth = -1)
        {
            var childNodes = root.Children();
            var maxDepth = 1;
            for (var x = 0; x < childNodes.Count; x++)
            {
                children.Add(childNodes[x].index);
                if(limitDepth != 0)
                {
                    var depth = TraverseIndexes(article, childNodes[x], children, limitDepth-=1);
                    if (depth > maxDepth) { maxDepth = depth; }
                }
            }
            return maxDepth;
        }

        public static void GetBestElementIndexes(AnalyzedArticle article, List<AnalyzedElement> indexes)
        {
            //First, analyze each DOM element /////////////////////////////////////////////////////////////////////
            var futureElements = new List<AnalyzedElement>();

            for (var x = 0; x < article.elements.Count; x++)
            {
                var element = article.elements[x];

                //check if element should be skipped
                if (Rules.skipTags.Contains(element.tagName)) { continue; }

                //initialize analyzed element object
                var index = new AnalyzedElement()
                {
                    Element = element,
                    index = element.index,
                    depth = element.hierarchyIndexes.Length
                };

                //check if any future element exist to replace the new index object with
                if(futureElements.Count > 0 && futureElements.Any(a => a.index == element.index))
                {
                    //clone element and remove from future elements list
                    futureElements.First(a => a.index == element.index).CopyTo(index);
                    futureElements.RemoveAt(futureElements.FindIndex(a => a.index == element.index));
                }

                //build hierarchy of analyzed elements
                foreach(var i in element.hierarchyIndexes)
                {
                    var parent = indexes.FirstOrDefault(a => a.index == i);
                    if(parent != null) {
                        index.hierarchy.Add(parent);
                        if(parent.isContaminated == true) { break; }
                    }
                }
                //check if element is a child of a contaminated parent element
                if(index.hierarchy.Any(a => a.isContaminated == true)) { continue; }

                //check element tag for contamination
                if (element.tagName == "#text")
                {
                    //text element
                    var txt = element.text.ToLower();
                    var words = SeparateWordsFromText(txt);
                    index.UpdateCounter(ElementFlagCounters.words, words.Length);
                    foreach(var elem in index.hierarchy)
                    {
                        //add current element words to all analyzed elements in hierarchy
                        elem.wordsInHierarchy += words.Length;
                    }

                    if (words.Length < 7)
                    {
                        //check if element is potential menu item
                        if (index.hierarchy.Where(a => Rules.textTags.Contains(a.Element.tagName)).Count() == 0 &&
                            index.hierarchy.Where(a => Rules.menuTags.Contains(a.Element.tagName)).Count() > 0 &&
                            CountWordsInText(txt, Rules.badMenu) > 0
                        )
                        {
                            index.AddFlag(ElementFlags.MenuItem);
                        }

                        //check if element has an anchor link in the hierarchy
                        if(index.hierarchy.Any(a => a.Element.tagName == "a"))
                        {
                            if (CountWordsInText(txt, Rules.badLinkWords) > 0)
                            {
                                index.AddFlag(ElementFlags.BadLinkWord);
                                index.isBad = true;
                            }
                        }

                        //check if element has a header tag (h1, h2, h3, h4, h5, h6) in the hierarchy
                        if (Rules.headerTags.Any(a => index.hierarchy.Any(b => b.Element.tagName == a)))
                        {
                            if (index.isBad == true || Rules.badHeaderWords.Any(a => txt.IndexOf(a) >= 0))
                            {
                                index.AddFlag(ElementFlags.BadHeaderWord);
                                index.isBad = true;

                                //remove ul if exists right below bad header tag
                                var h = index.hierarchy.FindLast(a => Rules.headerTags.Any(b => a.Element.tagName == b));
                                if(h != null)
                                {
                                    var next = h.Element.NextSibling;
                                    if(next != null && next.tagName == "ul")
                                    {
                                        var newIndex = new AnalyzedElement()
                                        {
                                            Element = next,
                                            index = next.index,
                                            depth = next.hierarchyIndexes.Length
                                        };
                                        newIndex.AddFlag(ElementFlags.BadHeaderMenu);
                                        newIndex.isBad = true;
                                        newIndex.isContaminated = true;
                                        futureElements.Add(newIndex);
                                    }
                                }
                            }
                        }
                    }
                    if (words.Length <= 30)
                    {
                        //check bad keywords
                        index.UpdateCounter(ElementFlagCounters.badKeywords,
                            CountWordsInText(txt, Rules.badKeywords) +
                            (CountWordsInText(txt, Rules.badTrailing) > 2 ? 1 : 0));

                        //check bad keywords for flagging parent element
                        if(CountWordsInText(txt, Rules.badKeywordsForParentElement) > 0)
                        {
                            index.hierarchy.Last().isContaminated = true;
                            RemoveAllChildIndexes(article, indexes, index);
                            continue;
                        }
                    }

                    //check for legal words
                    var legalWords = CountWordsInText(txt, Rules.badLegal);
                    if(legalWords > 0)
                    {
                        index.UpdateCounter(ElementFlagCounters.badLegalWords, legalWords);
                    }
                }
                else
                {
                    //non-text element
                    if(element.tagName == "img")
                    {
                        index.AddFlag(ElementFlags.IsImage);
                    }
                    
                    //get count of text elements as immediate children of current element
                    var childTextElements = 0;
                    foreach (var child in element.Children())
                    {
                        if (child.tagName == "#text")
                        {
                            childTextElements++;
                        }
                    }
                    if(childTextElements > 0)
                    {
                        index.UpdateCounter(ElementFlagCounters.childTextElements, childTextElements);
                    }

                    //check for header
                    if (element.tagName == "header" && element.Parent.tagName == "body")
                    {
                        index.isBad = true;
                        index.isContaminated = true;
                    }

                    //check element for bad class names
                    if (element.className != null && element.className.Count > 0)
                    {
                        var bad = GetWordsInText(string.Join(' ', element.className), Rules.badClasses).ToList();
                        if (bad.Count > 0)
                        {
                            bad = bad.Where(badclass =>
                                element.className.Where(classes =>
                                     classes.IndexOf(badclass) >= 0 && !Rules.ignoreClasses.Any(a => classes.IndexOf(a) >= 0)
                                ).Count() > 0).ToList();
                        }
                        index.badClasses = bad.Count;
                        if(index.badClasses > 0)
                        {
                            index.badClassNames = bad;
                            index.isContaminated = true;
                        }
                    }

                    //check element for bad tag names
                    if (Rules.badTags.Contains(element.tagName))
                    {
                        index.AddFlag(ElementFlags.BadTag);
                        index.isBad = true;
                        index.isContaminated = true;
                    }

                    //check element for bad URL
                    if (element.tagName == "a")
                    {
                        if (element.attribute.ContainsKey("href") &&
                            Rules.badUrls.Where(a => element.attribute["href"].IndexOf(a) >= 0).Count() > 0)
                        {
                            index.AddFlag(ElementFlags.BadUrl);
                            index.isBad = true;
                        }
                    }
                }
                indexes.Add(index);

            } //check next element

            //Next, analyze groups of elements ////////////////////////////////////////////////////////////////////

            //filter indexes that contain too many legal words
            for (var x = 0; x < indexes.Count; x++)
            {
                var index = indexes[x];
                var children = new List<int>();
                TraverseIndexes(article, index.Element, children, 2);
                var legalwords = indexes.Select(a => children.Contains(a.index) ? 
                    (a.Counter(ElementFlagCounters.badLegalWords) > 1 ? a.Counter(ElementFlagCounters.badLegalWords) : 0) 
                    : 0).Sum();

                if (legalwords > 5)
                {
                    //too many legal words
                    index.isBad = true;
                    var remove = indexes.Where(a => children.Contains(a.index)).ToList();
                    for (var y =0; y < remove.Count; y++)
                    {
                        remove[y].isBad = true;
                    }
                }
            }

            //filter best indexes that contain menus
            var menus = indexes.Where(a => a.HasFlag(ElementFlags.MenuItem));
            foreach(var item in menus)
            {
                var parents = item.Element.Hierarchy();
                if(parents.Any(a => a.tagName == "ul" || a.tagName == "ol"))
                {
                    //found parent by ul or ol tag
                    var elem = parents.Last(a => a.tagName == "ul" || a.tagName == "ol");
                    var index = indexes.Where(a => a.index == elem.index).FirstOrDefault();
                    if (index != null)
                    {
                        //make menu container contaminated
                        indexes.Where(a => a.index == elem.index).First().isBad = true;
                        //make all child indexes contaminated
                        var children = new List<int>();
                        TraverseIndexes(article, elem, children);
                        var remove = indexes.Where(a => children.Contains(a.index)).ToList();
                        for (var y = 0; y < remove.Count; y++)
                        {
                            remove[y].isBad = true;
                        }
                    }
                }
                else
                {
                    //try to find menu by traversing backwards through parent elements
                    var i = 4;
                    var elem = article.elements[item.index];
                    while (i-- > 0)
                    {
                        elem = elem.Parent;
                        if (elem != null && elem.tagName == "div")
                        {
                            var index = indexes.Where(a => a.index == elem.index).FirstOrDefault();
                            if (index != null)
                            {
                                if (menus.Where(a => article.elements[a.index].hierarchyIndexes.Where(b => b == index.index).Count() > 0).Count() > 1)
                                {
                                    //found menu container
                                    index.isBad = true;
                                }
                            }
                        }
                    }
                }
                item.isBad = true;
            }

            //filter indexes that contain roles
            for (var x = 0; x < indexes.Count; x++)
            {
                var index = indexes[x];
                var elem = article.elements[index.index];
                if (elem.attribute.ContainsKey("role"))
                {
                    var role = elem.attribute["role"];
                    if (Rules.badRoles.Where(a => a == role).Count() > 0)
                    {
                        //make all child indexes contaminated
                        var children = new List<int>();
                        TraverseIndexes(article, elem, children);
                        var remove = indexes.Where(a => children.Contains(a.index)).ToList();
                        for (var y = 0; y < remove.Count; y++)
                        {
                            remove[y].isBad = true;
                        }
                    }
                }
            }
        }

        public static void GetArticleElements(AnalyzedArticle article, List<AnalyzedElement> indexes)
        {
            //build list of DOM elements that contains the article
            var bodyText = new List<int>();
            int parentId;
            var isFound = false;
            var isEnd = false;
            DomElement elem;
            var checkedIndexes = new List<int>();

            for (var x = indexes.Count - 1; x >= 0; x--)
            {
                //all elements are a part of this parent element
                //get a list of text elements that are a part of the 
                //parent element
                parentId = indexes[x].index;
                isFound = false;
                isEnd = false;
                for (var y = parentId + 1; y < article.elements.Count; y++)
                {
                    elem = article.elements[y];

                    if (elem.hierarchyIndexes.Contains(parentId))
                    {
                        //check if index was already handled
                        if (checkedIndexes.Contains(elem.index)) { continue; }
                        checkedIndexes.Add(elem.index);

                        //check for bad indexes
                        if (indexes.Where(a => (a.isBad == true || a.isContaminated == true) && 
                            (a.index == elem.index || elem.hierarchyIndexes.Contains(a.index))
                            ).Count() > 0 
                        ) { break; }


                        //determine which elements to include in the results
                        if (elem.tagName == "#text")
                        {
                            //add text
                            elem.text = WebUtility.HtmlDecode(elem.text);
                            bodyText.Add(elem.index);
                        }
                        else if(elem.tagName == "br")
                        {
                            //add line break
                            bodyText.Add(elem.index);
                        }
                        else if(elem.tagName == "img")
                        {
                            //add image
                            bodyText.Add(elem.index);
                        }
                        else if(elem.tagName == "a")
                        {
                            
                        }
                    }
                    else
                    {
                        //no longer part of parent id
                        isFound = true;
                        break;
                    }
                    if (isFound == true || isEnd == true) { break; }
                }
            }
            bodyText.Sort();
            article.body = bodyText;
        }

        public static string GetArticleText(AnalyzedArticle article)
        {
            //build article text
            DomElement domText;
            var text = new StringBuilder();
            for (var x = 0; x < article.body.Count; x++)
            {
                domText = article.elements[article.body[x]];
                if (domText.HierarchyTags().Where(h => Rules.headerTags.Contains(h)).Count() > 0)
                {
                    continue;
                }
                text.Append(domText.text.Trim() + " ");
            }
            return text.ToString();
        }

        public static List<string> GetSentences(string text)
        {
            var txt = "";
            var charA = "A".Asc();
            var charZ = "Z".Asc();
            var txt1 = "";
            var txt2 = "";
            char txt3;
            int sentenceStart = 0;
            bool foundSentence = false;
            var sentence = "";
            var sentences = new List<string>();
            var modtext = WebUtility.HtmlDecode(text.Replace("&nbsp;", ""))
                .Replace("... ", "{{p3}}").Replace(".. ", ". ").Replace(".. ", ". ").Replace("{{p3}}", "... ")
                .ReplaceAll("=+={1}=+=", Rules.sentenceSeparators);
            var texts = modtext.Replace("=+==+=", "=+=").Replace("=+==+=", "=+=")
                .Split(new string[] { "=+=" }, StringSplitOptions.RemoveEmptyEntries).Where(p => p.Trim() != "").ToArray();
            for (var x = 0; x < texts.Length; x++)
            {
                foundSentence = false;
                sentence = "";
                txt = texts[x].ToLower().Trim();
                if (txt == "") { continue; }

                //find end of sentence
                if (txt == ".")
                {
                    foundSentence = true;
                    //is period actually used for the end of a sentence 
                    //or is it used for an acronym or abbreviation instead?
                    if (texts.Length > x + 3)
                    {
                        //look ahead for a period
                        txt1 = texts[x + 1].Trim();
                        if (txt1 == "") { txt1 = " "; }
                        if (texts[x + 2].Trim() == ".")
                        {
                            if (txt1.IndexOf(" ") < 0)
                            {
                                //part of an abbreviation or acronym
                                foundSentence = false;
                            }
                            txt2 = texts[x + 3];
                            if (txt2.Length > 3)
                            {
                                //look ahead for website URLs
                                if (Rules.domainSuffixes.Contains(txt2.Substring(0, 3)) == true)
                                {
                                    txt3 = char.Parse(txt2.Substring(3, 1));
                                    if (txt3.CheckChar() == false)
                                    {
                                        //found a website URL, it is not an acronym like I had thought
                                        foundSentence = true;
                                    }
                                }
                            }

                        }
                        else if (Char.Parse(txt1.Substring(0, 1)).CheckChar(true, false, null, true) == true)
                        {
                            //look ahead for a capital letter
                        }
                        //text is a potential sentence part
                        if (x >= 2)
                        {
                            if (texts[x - 1].Trim() == ".")
                            {
                                foundSentence = false;
                            }
                        }
                        if (x < texts.Length - 2)
                        {
                            txt2 = texts[x + 1].Trim();
                            if (txt2.IndexOf("com") == 0)
                            {
                                if (1 == 1)
                                {

                                }
                            }
                            if (txt2.Length > 3)
                            {
                                //look for website URLs
                                if (Rules.domainSuffixes.Contains(txt2.Substring(0, 3)) == true)
                                {
                                    txt3 = Char.Parse(txt2.Substring(3, 1));
                                    if (txt3.CheckChar() == false)
                                    {
                                        //found a website URL
                                        foundSentence = false;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (txt == "!" || txt == "?" || txt == ":")
                {
                    foundSentence = true;
                }


                if (foundSentence == true)
                {
                    //look for quotes
                    if (texts.Length - 1 > x)
                    {
                        txt1 = texts[x + 1].Trim();
                        if (txt1 != "")
                        {
                            txt2 = txt1.Substring(0, 1);
                            if (txt2 == "\"" || txt2 == ",")
                            {
                                foundSentence = false;
                            }
                        }

                    }
                }


                if (foundSentence == true || x == texts.Length - 1)
                {
                    for (int z = sentenceStart; z <= x; z++)
                    {
                        sentence += texts[z];
                    }
                    //check sentence length
                    if (sentence.Length > 1)
                    {
                        txt = sentence.Trim();
                        if (txt.Length > 0)
                        {
                            txt1 = txt.Substring(0, 1);
                            txt2 = txt.Substring(txt.Length - 1, 1);
                            if (Rules.badChars.Contains(txt1) == true)
                            {
                                if (txt.Length > 1)
                                {
                                    txt = txt.Substring(1);
                                }
                                else
                                {
                                    txt = "";
                                }
                            }
                            if (Rules.badChars.Contains(txt2) == true)
                            {
                                if (txt2 != ":")
                                {
                                    txt = txt.Substring(0, txt.Length - 1);
                                }
                            }
                            if (txt != "")
                            {
                                sentences.Add(txt);
                                sentenceStart = x + 1;
                            }
                        }
                    }
                }
            }
            return sentences;
        }

        public static bool IsSentenceSeparator(string character, string[] exceptions = null)
        {
            if (exceptions != null)
            {
                if (exceptions.Contains(character) == true) { return false; }
            }
            return Rules.sentenceSeparators.Contains(character);
        }

        public static void GetImages(AnalyzedArticle article)
        {
            article.images = new List<AnalyzedImage>();
            for (var x = 0; x < article.body.Count; x++)
            {
                var elem = article.elements[article.body[x]];
                if (elem.tagName == "img" && elem.attribute.ContainsKey("src"))
                {
                    var img = new AnalyzedImage()
                    {
                        index = elem.index,
                        url = elem.attribute["src"]
                    };

                    img.filename = img.url.GetFilename();
                    img.extension = img.url.GetFileExtension().ToLower();

                    switch (img.extension)
                    {
                        case "jpg": case "jpeg": case "png": case "gif": case "tiff":
                        case "webp": case "bpg": case "flif": 
                            //only add supported image types
                            article.images.Add(img);
                            break;
                    }
                    
                }
            }
        }

        public static string[] SeparateWordsFromText(string text, string[] exceptions = null)
        {
            if (exceptions != null)
            {

                var ws = Rules.wordSeparators.Where(w => !exceptions.Contains(w)).ToArray();
                return text.ReplaceAll(" {1} ", ws).Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Split(' ').Where(w => w != "").ToArray();
            }
            return text.ReplaceAll(" {1} ", Rules.wordSeparators).Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Split(' ').Where(w => w != "").ToArray();
        }

        public static int CountWordsInText(string text, string[] words)
        {
            return GetWordsInText(text, words).Length;
        }

        public static string[] GetWordsInText(string text, string[] words)
        {
            return words.Where(word => text.IndexOf(word) == 0 || ( //first character in text,
                text.IndexOf(word) > 0 &&                           //or beginning of word
                text.Substring(text.IndexOf(word) - 1, 1).ToCharArray()[0].CheckChar(true, true) == false
            )).ToArray();
        }


        public static void RemoveAllChildIndexes(AnalyzedArticle article, List<AnalyzedElement> indexes, AnalyzedElement element)
        {
            var children = new List<int>();
            TraverseIndexes(article, element.hierarchy.Last().Element, children);
            var remove = indexes.Where(a => children.Contains(a.index)).ToList();
            if (remove.Count > 0)
            {
                var xs = new List<int>();
                foreach (var r in remove.Select(a => a.index))
                {
                    indexes.RemoveAt(indexes.FindIndex(a => a.index == r));
                }
            }
        }
        
        #endregion

    }
}
