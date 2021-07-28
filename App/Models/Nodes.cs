using System.Collections.Generic;

namespace Collector.Models.Nodes
{
    //used for deserializing computed DOM object from WebBrowser console app
    public class Document
    {
        public string[] a { get; set; }
        public Node dom { get; set; }
    }

    public class Node
    {
        public string t { get; set; } = ""; //tag name
        public int[] s { get; set; } = null; //array of style values [display (0 = none, 1 = block, 2 = inline), font-size, bold, italic]
        public Dictionary<int, string> a { get; set; } //dictionary of element attributes
        public List<Node> c { get; set; } //list of child elements
        public string v { get; set; } //optional #text element value
    }
}
