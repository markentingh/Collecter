using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Collector.Services
{
    public class Search : Service
    {

        public Search(Core CollectorCore, string[] paths) : base(CollectorCore, paths)
        {
        }

        public Inject GetResultsUI()
        {
            var inject = new Inject();

            return inject;
        }
    }
}
