using System.Collections.Generic;

namespace Collector.Models.Nodes
{
    //used for deserializing computed DOM object from WebBrowser console app
    public class Document
    {
        public string[] a;
        public Node dom;
    }

    public class Node
    {
        public string t = ""; //tag name
        public int[] s = null; //array of style values [display (0 = none, 1 = block, 2 = inline), font-size, bold, italic]
        public Dictionary<int, string> a; //dictionary of element attributes
        public List<Node> c; //list of child elements
        public string v; //optional #text element value
    }
}
