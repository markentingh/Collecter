using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Utility.Serialization;
using Utility.DOM;
using Collector.Models.Article;
using Utility.Strings;
using Utility;

namespace Collector.Common.Analyze
{
    public static class Html
    {
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

        #region "Get HTML Document"
        public static AnalyzedArticle DeserializeArticle(string dom)
        {
            //deserialize object from string
            var node = (DomNode)Serializer.ReadObject(dom, typeof(DomNode));
            var html = new StringBuilder();
            var hierarchy = new List<int>();
            var article = new AnalyzedArticle();
            var parser = new Parser("");

            //build DOM tree
            var elems = new List<DomElement>();
            var index = 0;
            Traverse(node, ref index, elems, hierarchy, parser);
            article.elements = elems;
            article.rawHtml = FormatHtml(elems).ToString();

            return article;
        }

        private static DomElement Traverse(DomNode parent, ref int index, List<DomElement> elems, List<int> hierarchy, Parser parser)
        {
            //create local copy of hierarchy
            int[] hier = new int[hierarchy.Count];
            if(hierarchy.Count > 0)
            {
                hierarchy.CopyTo(hier);
            }
            
            //create DOM element
            var elem = new DomElement(parser);
            elem.index = index;
            elem.tagName = parent.tag;
            elem.hierarchyIndexes = hier;
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
                elem.text = parent.value;
            }
            else
            {
                //build style list for element
                if (parent.style != null)
                {
                    switch (parent.style.display)
                    {
                        case 0: elem.style.Add("display", "none"); break;
                        case 2: elem.style.Add("display", "inline"); break;
                        case 3: elem.style.Add("display", "inline-block"); break;
                    }
                    elem.style.Add("font-size", parent.style.fontsize.ToString() + "px");
                    if (parent.style.fontweight == 2)
                    {
                        elem.style.Add("font-weight", "bold");
                    }
                    if (parent.style.italic == true)
                    {
                        elem.style.Add("font-style", "italic");
                    }
                }

                //build attributes list
                if (parent.attrs != null)
                {
                    elem.attribute = parent.attrs;
                }
            }

            //append index to hierarchy list
            hier = hier.Append(index).ToArray();

            //add element to DOM elements list
            elems.Add(elem);

            if(parent.children != null)
            {
                //traverse all children
                foreach (var child in parent.children)
                {
                    index++;
                    var childElem = Traverse(child, ref index, elems, hier.ToList(), parser);
                    elem.childIndexes.Add(childElem.index);
                }
            }

            if(elem.isSelfClosing == false)
            {
                //add closing tag to DOM elements list
                index++;
                var closing = new DomElement(parser);
                closing.index = index;
                closing.tagName = "/" + parent.tag;
                closing.hierarchyIndexes = hierarchy.ToArray();
                closing.childIndexes = new List<int>();
                closing.style = new Dictionary<string, string>();
                closing.attribute = new Dictionary<string, string>();
                closing.isClosing = true;
                elems.Add(closing);
            }
            return elem;
        }
        #endregion

        #region "Get HTML Content & Words"
        public static void GetContent(
            AnalyzedArticle article,
            List<AnalyzedTag> tagNames, 
            List<DomElement> textElements,
            List<DomElement> anchorElements,
            List<DomElement> headerElements,
            List<DomElement> imgElements,
            List<AnalyzedParentIndex> parentIndexes)
        {
            DomElement traverseElement;

            foreach (DomElement element in article.elements)
            {
                switch (element.tagName.ToLower())
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
                            if (traverseElement.parent > -1)
                            {
                                //get next parent element
                                traverseElement = article.elements[traverseElement.parent];
                            }
                            else { break; }
                        } while (traverseElement.parent > -1);
                        break;
                    case "a":
                        //sort anchor links
                        anchorElements.Add(element);
                        break;
                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6":
                        headerElements.Add(element);
                        break;
                    case "img":
                        imgElements.Add(element);
                        break;
                    case "title":
                        if (article.title == "")
                        {
                            article.pageTitle = article.title = article.elements[element.index + 1].text.Trim();

                            //check for 404 error
                            if (article.title.IndexOf("404") >= 0)
                            {
                                return;
                            }
                        }
                        break;
                }

                //add new tag to list or incriment count of existing tag in list
                var tagIndex = tagNames.FindIndex(x => x.name == element.tagName);
                if (tagIndex >= 0)
                {
                    //incriment tag name count
                    var t = tagNames[tagIndex];
                    t.count += 1;
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
        }
        
        public static void GetWords(AnalyzedArticle article, List<DomElement> text)
        {
            string[] texts;
            AnalyzedWord word;
            AnalyzedWordInText wordIn;
            var index = 0;
            var i = -1;
            var allWords = new List<AnalyzedWord>(); //every word in text
            var words = new List<AnalyzedWord>(); //every unique word in text


            foreach (DomElement element in text)
            {
                var textTypesCount = 13;
                var wordsInText = new List<AnalyzedWordInText>();
                var newText = new AnalyzedText();
                newText.index = element.index;
                //separate all words & symbols
                texts = GetWordsFromText(element.text);

                for (var x = 0; x < texts.Length; x++)
                {
                    if (texts[x].Trim() == "") { continue; }
                    wordIn = new AnalyzedWordInText();
                    wordIn.word = texts[x];
                    wordIn.index = x;

                    //add word to all words list
                    index = allWords.FindIndex(w => w.word == texts[x]);
                    if (index >= 0)
                    {
                        //incriment word count
                        var w = allWords[index];
                        w.count += 1;
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

                    //set reference to article word
                    wordsInText.Add(wordIn);
                }
                newText.words = wordsInText;
                //var sortedAllWords = allWords.OrderBy(a => a.count * -1).ToList();

                //check all words for patterns to determine
                //what type of text is in this element
                i = -1;
                var len = wordsInText.Count;
                var possibleTypes = new int[textTypesCount + 1];

                foreach (AnalyzedWordInText aword in wordsInText)
                {
                    i++;
                    if (CheckWordForPossibleTypes(article, element, aword.word.ToLower(), ref possibleTypes, len) == false)
                    {
                        break;
                    }
                }

                //check element hierarchy to guess what kind of content it consists of
                if (element.text.Length >= 4)
                {
                    //useless type
                    if (element.text.Trim().IndexOf("<!--") == 0)
                    {
                        possibleTypes[(int)TextType.useless] = 10000;
                    }
                }
                if (element.HasTagInHierarchy("ul") && element.HasTagInHierarchy("li"))
                {
                    //menu item
                    possibleTypes[(int)TextType.menuItem] += 100;
                }
                else if (element.HasTagInHierarchy("a"))
                {
                    //anchor link
                    possibleTypes[(int)TextType.anchorLink] += 100;
                }

                //sort possible types by count
                var possTypes = new List<PossibleTextType>();
                var e = 0;
                for (e = 0; e <= textTypesCount; e++)
                {
                    var newPoss = new PossibleTextType();
                    newPoss.type = (TextType)e;
                    newPoss.count = possibleTypes[e];
                    possTypes.Add(newPoss);
                }
                var sortedPossTypes = possTypes.OrderBy(p => p.count * -1).ToList();
                //newText.possibleTypes = sortedPossTypes;

                //figure out dominant type from possible types
                for (e = 0; e < sortedPossTypes.Count; e++)
                {
                    var t = sortedPossTypes[e];
                    var found = false;
                    if (t.count > 1)
                    {
                        if (t.type == TextType.script)
                        {
                            if (t.count >= 5) { found = true; }
                        }
                        else if (t.type == TextType.style)
                        {
                            if (t.count >= 5) { found = true; }
                        }
                        else if (t.type == TextType.useless)
                        {
                            if (t.count >= 5) { found = true; }
                        }
                        else if (t.type == TextType.copyright)
                        {
                            if (t.count >= 2) { found = true; }
                        }
                        else if (t.type == TextType.publishDate)
                        {
                            if (t.count >= 5)
                            {
                                found = true;
                            }
                        }
                        else if (t.type == TextType.anchorLink)
                        {
                            found = true;
                        }
                        else if (t.type == TextType.menuItem)
                        {
                            found = true;
                        }
                    }
                    if (found == true)
                    {
                        newText.type = t.type;
                        break;
                    }
                }

                //add text to article results
                article.tags.text.Add(newText);
            }
        }

        public static string[] GetWordsFromText(string text, string[] exceptions = null)
        {
            if (exceptions != null)
            {

                var ws = Rules.wordSeparators.Where(w => !exceptions.Contains(w)).ToArray();
                return text.ReplaceAll(" {1} ", ws).Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Split(' ').Where(w => w != "").ToArray();
            }
            return text.ReplaceAll(" {1} ", Rules.wordSeparators).Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Split(' ').Where(w => w != "").ToArray();
        }

        private static bool CheckWordForPossibleTypes(AnalyzedArticle article, DomElement element, string w, ref int[] possibleTypes, int totalWords)
        {
            if (Rules.scriptSeparators.Contains(w))
            {
                if (element.index > 0)
                {
                    if (article.elements[element.index - 1].tagName == "script")
                    {
                        possibleTypes[(int)TextType.script] += 5000;
                        return false;
                    }
                    if (article.elements[element.index - 1].tagName == "style")
                    {
                        possibleTypes[(int)TextType.style] += 5000;
                        return false;
                    }
                }



            }
            else if (w == "copyright")
            {
                possibleTypes[(int)TextType.copyright] += 1;
            }
            else if (w.IndexOf("&copy") >= 0 || w == "©")
            {
                possibleTypes[(int)TextType.copyright] += 2;
            }
            else if (w == "rights" || w == "reserved")
            {
                possibleTypes[(int)TextType.copyright] += 1;
            }
            else if (Rules.dateTriggers.Contains(w))
            {
                if (totalWords < 20)
                {
                    //small group of text has better chance 
                    //of being a publish date
                    possibleTypes[(int)TextType.publishDate] += 5;
                }
                else
                {
                    possibleTypes[(int)TextType.publishDate] += 1;
                }
            }
            else if (w.IndexOf("advertis") >= 0 || w.IndexOf("sponsor") >= 0)
            {
                if (totalWords < 10)
                {
                    //small group of text has better chance 
                    //of being an advertisement
                    possibleTypes[(int)TextType.advertisement] += 5;
                }
                else
                {
                    possibleTypes[(int)TextType.advertisement] += 1;
                }
            }
            return true;
        }
        #endregion

        #region "Get Article Text"
        public static void GetArticleElements(AnalyzedArticle article)
        {
            var pIndexes = new List<AnalyzedElementCount>();
            var i = 0;
            foreach (var a in article.tags.text)
            {

                if (a.type == TextType.mainArticle)
                {
                    //find most relevant parent indexes shared by all article text elements
                    i++;

                    //limit to large article text
                    if (a.words.Count < 10 && i > 2) { i--; break; }

                    //limit to top 5 article text
                    if (i > 10) { i--; break; }

                    foreach (int indx in article.elements[a.index].hierarchyIndexes)
                    {
                        var parindex = pIndexes.FindIndex(p => p.index == indx);
                        if (parindex >= 0)
                        {
                            //update count for a parent index
                            var p = pIndexes[parindex];
                            p.count += 1;
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
            var sortedArticleParents = pIndexes.OrderBy(p => p.index * -1).OrderBy(p => p.count * -1).ToList();

            //determine best parent element that contains the entire article
            var bodyText = new List<int>();
            var uselessText = new[] {
                TextType.advertisement,
                TextType.comment,
                TextType.copyright,
                TextType.script,
                TextType.style,
                TextType.useless
            };

            int parentId;
            var isFound = false;
            var isBad = false;
            var isEnd = false;
            var badIndexes = new List<int>();
            string[] articleText;
            string articletxt = "";
            DomElement hElem;
            DomElement elem;

            for (var x = sortedArticleParents.Count - 1; x >= 0; x--)
            {
                //all elements are a part of this parent element
                //get a list of text elements that are a part of the 
                //parent element
                parentId = sortedArticleParents[x].index;
                isFound = false;
                isBad = false;
                isEnd = false;
                for (var y = parentId + 1; y < article.elements.Count; y++)
                {
                    elem = article.elements[y];
                    if (Objects.IsEmpty(elem)) { continue; }
                    if (elem.hierarchyIndexes.Contains(parentId))
                    {
                        if (elem.hierarchyIndexes.Where(ind => badIndexes.Contains(ind)).Count() > 0) { continue; }
                        if (elem.tagName == "#text")
                        {
                            //element is text & is part of parent index
                            //check if text type is article
                            var textTag = article.tags.text.Find(p => p.index == y);
                            if (!uselessText.Contains(textTag.type))
                            {
                                articletxt = elem.text.ToLower();
                                articleText = GetWordsFromText(articletxt);
                                if (elem.className != null)
                                {
                                    elem.className = elem.className.Select(c => c.ToLower()).ToList();
                                }
                                else { elem.className = new List<string>(); }

                                //check for any text from the article that does not belong,
                                //such as advertisements, side-bars, photo credits, widgets
                                isBad = false;

                                for (var z = elem.hierarchyIndexes.Length - 1; z >= 0; z--)
                                {
                                    //search down the hierarchy DOM tree
                                    hElem = article.elements[elem.hierarchyIndexes[z]];
                                    if (hElem.index == parentId) { break; }

                                    //check for bad tag names
                                    if (Rules.badTags.Contains(hElem.tagName)) { isBad = true; break; }

                                    //check classes for bad element indicators within class names
                                    if (hElem.className.Count > 0)
                                    {
                                        if (hElem.className.Where(c => Rules.badClasses.Contains(c)).Count() > 0)
                                        {
                                            isBad = true; break;
                                        }
                                    }
                                }

                                if (articletxt.IndexOf("disqus") >= 0)
                                {
                                    //article comments
                                    isBad = true;
                                    isEnd = true;
                                }

                                if (articleText.Length <= 20)
                                {
                                    if (articleText.Where(t => Rules.badPhotoCredits.Contains(t)).Count() >= 2)
                                    {
                                        //photo credits
                                        isBad = true;
                                    }
                                }
                                if (articleText.Length <= 7 && isBad != true)
                                {
                                    if (articleText.Where(t => Rules.badMenu.Contains(t)).Count() >= 1)
                                    {
                                        //menu
                                        isBad = true;
                                    }
                                    if (articletxt.IndexOf("additional resources") >= 0)
                                    {
                                        //end of article
                                        isBad = true;
                                        isEnd = true;
                                    }
                                }

                                if (articleText.Length <= 3 && isBad != true)
                                {
                                    if (articleText.Where(t => Rules.badChars.Contains(t)).Count() >= 1)
                                    {
                                        //bad characters
                                        isBad = true;
                                    }
                                    if (articleText.Where(t => Rules.badTrailing.Contains(t)).Count() >= 1)
                                    {
                                        //bad characters
                                        isBad = true;
                                        isEnd = true;
                                    }
                                }

                                if (isBad == false)
                                {
                                    //finally, add text to article
                                    if (!bodyText.Contains(elem.index))
                                    {
                                        bodyText.Add(elem.index);
                                    }
                                    //clean up text in element
                                    elem.text = WebUtility.HtmlDecode(elem.text);
                                }
                            }
                        }
                        else
                        {
                            //element is not text, 
                            //determine if element contains bad content
                            if (elem.className != null)
                            {
                                if (elem.className.Where(c => Rules.badClasses.Where(bc => c.IndexOf(bc) >= 0).Count() > 0).Count() > 0)
                                {
                                    badIndexes.Add(elem.index);
                                }
                            }
                            if (elem.attribute != null)
                            {
                                if (elem.attribute.ContainsKey("id"))
                                {
                                    if (Rules.badClasses.Where(bc => elem.attribute["id"].ToLower().IndexOf(bc) >= 0).Count() > 0)
                                    {
                                        badIndexes.Add(elem.index);
                                    }
                                }
                            }

                            if (Rules.badArticleTags.Contains(elem.tagName))
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

        public static bool isSentenceSeparator(string character, string[] exceptions = null)
        {
            if (exceptions != null)
            {
                if (exceptions.Contains(character) == true) { return false; }
            }
            return Rules.sentenceSeparators.Contains(character);
        }
        #endregion

    }
}
