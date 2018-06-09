
namespace Collector.Components
{
    public static class Accordion
    {
        public static string Render(string title, string classNames, string contents, bool expanded = true, bool whiteBg = false)
        {
            var scaffold = new Scaffold("/Views/Components/Accordion/accordion.html", Server.Scaffold);
            scaffold.Data["title"] = title;
            scaffold.Data["classNames"] = classNames;
            scaffold.Data["contents"] = contents;
            scaffold.Data["expanded"] = expanded == true ? "expanded" : "";
            scaffold.Data["whitebg"] = whiteBg == true ? "white" : "";

            return scaffold.Render();
        }
    }
}
