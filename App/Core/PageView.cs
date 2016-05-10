
namespace Collector
{
    public class PageView
    {

        protected Core S;
        protected Scaffold parentScaffold;

        public string scriptFiles = "";

        public PageView(Core CollectorCore, Scaffold ParentScaffold)
        {
            S = CollectorCore;
            parentScaffold = ParentScaffold;
        }

        public virtual string Render()
        {
            return "";
        }
    }
}
