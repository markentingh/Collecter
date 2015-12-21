using System.Collections.Generic;

namespace Collector.Utility.DOM
{
    public struct DomElement
    {
        public bool isSelfClosing;
        public bool isClosing;
        public int index;
        public int parent;
        public int[] hierarchyIndexes;
        public string tagName;
        public string text;
        public string id;
        public string[] hierarchyTags;
        public List<string> className;
        public List<int> children;
        public Dictionary<string, string> attribute;
        public Dictionary<string, string> style;
    }

    public class Element
    {
        public Dictionary<string, string> Attributes = new Dictionary<string, string>();
        public Dictionary<string, string> Style = new Dictionary<string, string>();
        public List<string> Classes = new List<string>();

        public string tagName = "";
        public string innerHTML = "";
        public bool ClosingTag = true;
        public string id = "";

        public Element(string tagname = "div", string tagId = "")
        {
            tagName = tagname;
            id = tagId;
        }

        public string Render()
        {
            string htm = "<" + tagName;

            //add id
            if(id != "") { htm += " id=\"" + id + "\""; }

            //add class names
            if(Classes.Count > 0)
            {
                htm += " class=\"";
                for (int x = 0; x < Classes.Count; x++)
                {
                   if(x > 0) { htm += " " + Classes[x]; }
                   else { htm += Classes[x]; } 
                }
                htm += "\"";
            }

            //add style
            if (Style.Count > 0)
            {
                htm += " style=\"";
                foreach(KeyValuePair<string, string> style in Style)
                {
                    htm += style.Key + ":" + style.Value + "; ";
                }
                htm += "\"";
            }

            //add attributes
            if (Attributes.Count > 0)
            {
                foreach (KeyValuePair<string, string> attr in Attributes)
                {
                    htm += " " + attr.Key + "=\"" + attr.Value + "\"";
                }
            }
            if(ClosingTag == false)
            {
                htm += "/>";
            }else
            {
                htm += ">" + innerHTML + "</" + tagName + ">";
            }
            return htm;
        }


    }

    public class Parser
    {
        private Core S;
        public string rawHtml;
        public List<DomElement> Elements;

        public Parser(Core CollectorCore, string htm)
        {
            S = CollectorCore;
            rawHtml = htm;
            Elements = new List<DomElement>();
            Parse(htm);
        }

        public void Parse(string htm)
        {
            if(htm.Length <= 3) { return; }
            //redevelop DOM parser, use single character traversal with character search
            bool isClosingTag = false;
            bool isSelfClosing = false;
            bool isInScript = false;
            bool foundTag = false;
            int s1, s2, s3, xs = -1;
            int parentElement = -1;
            string str1, schar, strTag, strText ;
            var hierarchy = new List<string>();
            var hierarchyIndexes = new List<int>();
            var domTag = new DomElement();
            var textTag = new DomElement();

            for (var x = 0; x < htm.Length; x++)
            {
                //find HTML tag

                if(foundTag == false && xs == 0)
                {
                    //no tags found in htm, create text tag and exit
                    textTag = new DomElement();
                    textTag.tagName = "#text";
                    textTag.text = htm;
                    AddTag(textTag, parentElement, true, false, hierarchy, hierarchyIndexes);
                    break;
                }
                else if(xs == -1)
                {
                    xs = x;
                }else if(foundTag == true)
                {
                    xs = x;
                }

                isClosingTag = false;
                isSelfClosing = false;
                foundTag = false;
                if(isInScript == true)
                {
                    //find closing script tag
                    //TODO: make sure </script> tag isn't in a 
                    //      javascript string, but instead is the
                    //      actual closing tag for the script
                    x = htm.IndexOf("</script>", x); 
                    if (x == -1) { break; }
                    schar = htm.Substring(x, 9).ToString();
                }
                else
                {
                    //find next html tag
                    x = htm.IndexOf('<', x);
                    if (x == -1) { break; }
                    schar = htm.Substring(x, 3).ToString();
                }
                if(schar[0] == '<')
                {
                    if (S.Util.Str.OnlyAlphabet(schar[1].ToString(), new string[] { "/", "!" }))
                    {
                        //found HTML tag
                        s1 = htm.IndexOf(">", x + 2);
                        s2 = htm.IndexOf("<", x + 2);
                        if(s1 >= 0)
                        {
                            //check for broken tag
                            if (s2 < s1 && s2 >= 0) { continue; }

                            //found end of tag
                            foundTag = true;
                            strTag = htm.Substring(x + 1, s1 - (x + 1));

                            //check for self-closing tag
                            str1 = strTag.Substring(strTag.Length - 1, 1);
                            if (str1 == "/"){ isSelfClosing = true; }

                            //check for attributes
                            s3 = strTag.IndexOf(" ");
                            if(s3 < 0)
                            {
                                //tag has no attributes
                                if(isSelfClosing)
                                {
                                    domTag.tagName = strTag.Substring(0, strTag.Length-2);
                                }
                                else
                                {
                                    //tag has no attributes & no forward-slash
                                    domTag.tagName = strTag;
                                }
                            }
                            else
                            {
                                //tag has attributes
                                domTag.tagName = strTag.Substring(0, s3);
                                domTag.attribute = GetAttributes(strTag);
                                domTag.style = new Dictionary<string, string>();

                                //set up class name list
                                if (domTag.attribute.ContainsKey("class")){
                                    domTag.className = new List<string>(domTag.attribute["class"].Split(' '));
                                }
                                else { domTag.className = new List<string>(); }

                                //set up style dictionary
                                if (domTag.attribute.ContainsKey("style"))
                                {
                                    var domStyle = new List<string>(domTag.attribute["style"].Split(';'));
                                    foreach (string keyval in domStyle)
                                    {
                                        var styleKeyVal = keyval.Trim().Split(new char[] {':'}, 2);
                                        if(styleKeyVal.Length == 2)
                                        {
                                            domTag.style.Add(styleKeyVal[0].Trim(), styleKeyVal[1].Trim());
                                        }
                                    }
                                    
                                }

                            }

                            //check if tag is script
                            if(isInScript == true)
                            {
                                isInScript = false;
                            }
                            else if(domTag.tagName.ToLower() == "script" && isSelfClosing == false)
                            {
                                isInScript = true;
                            }

                            //check if tag is self-closing even if it
                            //doesn't include a forward-slash at the end
                            switch (domTag.tagName.ToLower())
                            {
                                case "br":
                                case "img":
                                case "input":
                                case "link":
                                case "meta":
                                case "hr":
                                    isSelfClosing = true;
                                    break;
                            }
                            if (domTag.tagName.Substring(0, 1) == "!")
                            {
                                //comments & doctype are self-closing tags
                                isSelfClosing = true;
                            }

                            if (strTag.Substring(0, 1) == "/")
                            {
                                //found closing tag
                                isClosingTag = true;
                            }

                            //extract text before beginning of tag
                            strText = htm.Substring(xs, x - xs).Trim();
                            if(strText != "")
                            {
                                textTag = new DomElement();
                                textTag.tagName = "#text";
                                textTag.text = strText;
                                AddTag(textTag, parentElement, true, false, hierarchy, hierarchyIndexes);
                            }

                            //check if domTag is unusable
                            if (domTag.tagName == "" || domTag.tagName == null)
                            {
                                foundTag = false;
                                continue;
                            }

                            //add tag to array
                            parentElement = AddTag(domTag, parentElement, isSelfClosing, isClosingTag, hierarchy, hierarchyIndexes);

                            if (isClosingTag == true)
                            {
                                //go back one parent if this tag is a closing tag
                                if (parentElement >= 0)
                                {
                                    if(Elements[parentElement].tagName.ToLower() != domTag.tagName.Replace("/",""))
                                    {
                                        //not the same tag as the current parent tag, add missing closing tag
                                        if(Elements[parentElement].parent >= 0)
                                        {
                                            if (Elements[Elements[parentElement].parent].tagName.ToLower() == domTag.tagName.Replace("/", ""))
                                            {
                                                //replace unknown closing tag with missing closing tag
                                                domTag.tagName = "/" + Elements[Elements[parentElement].parent].tagName;
                                            }
                                            else
                                            {
                                                //skip this closing tag because it doesn't have an opening tag
                                                Elements.RemoveAt(Elements.Count - 1);
                                                x = xs = s1;
                                                continue;
                                            }
                                        }
                                    }
                                    parentElement = Elements[parentElement].parent;
                                    if (hierarchy.Count > 0)
                                    {
                                        hierarchy.RemoveAt(hierarchy.Count - 1);
                                        hierarchyIndexes.RemoveAt(hierarchyIndexes.Count - 1);
                                    }
                                }
                            }
                            x = xs = s1;
                            //Console.WriteLine(string.Join(" > ", hierarchy.ToArray()) + (isClosingTag || isSelfClosing ? " : " + domTag.tagName : "") + "\n");
                        }
                    }
                }
            }
            //finally, add last text tag (if possible)
            if(xs < htm.Length - 1)
            {
                textTag = new DomElement();
                textTag.tagName = "#text";
                textTag.text = htm.Substring(xs);
                AddTag(textTag, parentElement, true, false, hierarchy, hierarchyIndexes);
            }
        }

        public Dictionary<string, string> GetAttributes(string tag)
        {
            var attrs = new Dictionary<string, string>();
            int s1, s2, s3, s4, s5;
            string attrName, str2;
            string[] arr;
            s1 = tag.IndexOf(" ");
            if(s1 >= 1)
            {
                for (var x = s1; x < tag.Length; x++)
                {
                    //look for attribute name
                    s2 = tag.IndexOf("=", x);
                    s3 = tag.IndexOf(" ", x);
                    s4 = tag.IndexOf("\"", x);
                    s5 = tag.IndexOf("'", x);
                    if(s4 < 0) { s4 = tag.Length + 1; }
                    if (s5 < 0) { s5 = tag.Length + 2; }
                    if (s3 < s2 && s3 < s4 && s3 < s5)
                    {
                        //found a space first, then equal sign (=), then quotes
                        attrName = tag.Substring(s3 + 1, s2 - (s3 + 1));
                        //find type of quotes to use
                        if (s4 < s5)
                        {
                            //use double quotes
                            arr = tag.Substring(s4 + 1).Replace("\\\"","{{q}}").Split('"');
                            str2 = arr[0].Replace("{{q}}", "\\\"");
                            if (!attrs.ContainsKey(attrName))
                            {
                                attrs.Add(attrName, str2);
                            }
                            x = s4 + str2.Length + 1;
                        }else
                        {
                            //use single quotes
                            arr = tag.Substring(s5 + 1).Replace("\\'", "{{q}}").Split('\'');
                            str2 = arr[0].Replace("{{q}}", "\\'");
                            if (!attrs.ContainsKey(attrName))
                            {
                                attrs.Add(attrName, str2);
                            }
                            x = s5 + str2.Length + 1;
                        }
                    }
                }
            }
            return attrs;
        }

        private int AddTag(DomElement domTag, int parentElement, bool isSelfClosing, bool isClosingTag, List<string> hierarchy, List<int> hierarchyIndexes)
        {
            domTag.parent = parentElement;
            if (parentElement > -1)
            {
                DomElement parent = Elements[parentElement];
                if (parent.children == null)
                {
                    parent.children = new List<int>();
                }
                parent.children.Add(Elements.Count);
                Elements[parentElement] = parent;
            }
            //make current tag the parent
            if (isSelfClosing == false && isClosingTag == false)
            {
                parentElement = Elements.Count;
                hierarchy.Add(domTag.tagName);
                hierarchyIndexes.Add(parentElement);
            }

            if(domTag.tagName == "#text")
            {
                //Console.WriteLine(domTag.text + "\n\n");
            }

            domTag.index = Elements.Count;
            domTag.isSelfClosing = isSelfClosing;
            domTag.isClosing = isClosingTag;
            domTag.hierarchyTags = hierarchy.ToArray();
            domTag.hierarchyIndexes = hierarchyIndexes.ToArray();
            Elements.Add(domTag);
            return parentElement;
        }
    }
}
