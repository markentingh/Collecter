﻿
namespace Collector
{
    public class Include
    {

        protected Core S;
        protected Scaffold parentScaffold;

        public Include(Core CollectorCore, Scaffold ParentScaffold)
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
