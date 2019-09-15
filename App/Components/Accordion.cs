
namespace Collector.Components
{
    public static class Accordion
    {
        public static string Render(string title, string classNames, string contents, bool expanded = true, bool whiteBg = false)
        {
            var scaffold = new Scaffold("/Views/Components/Accordion/accordion.html");
            scaffold["title"] = title;
            scaffold["classNames"] = classNames;
            scaffold["contents"] = contents;
            scaffold["expanded"] = expanded == true ? "expanded" : "";
            scaffold["whitebg"] = whiteBg == true ? "white" : "";

            return scaffold.Render();
        }
    }
}
