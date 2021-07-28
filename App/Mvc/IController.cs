namespace Collector
{
    public interface IController: Datasilk.Core.Web.IController
    {
        string Title { get; set; }
        string Description { get; set; }
        string Theme { get; set; }
        string Favicon { get; set; }
        bool ContainsResource(string url);
    }
}