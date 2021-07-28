
namespace Collector.Components
{
    public static class Accordion
    {
        public static string Render(string title, string classNames, string contents, bool expanded = true, bool whiteBg = false)
        {
            var view = new View("/Views/Components/Accordion/accordion.html");
            view["title"] = title;
            view["classNames"] = classNames;
            view["contents"] = contents;
            view["expanded"] = expanded == true ? "expanded" : "";
            view["whitebg"] = whiteBg == true ? "white" : "";

            return view.Render();
        }
    }
}
