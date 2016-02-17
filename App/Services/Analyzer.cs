using System;
using System.Collections.Generic;
using System.Linq;

namespace Collector.Services
{
    public class Analyzer : Service
    {
        public Analyzer(Core CollectorCore, string[] paths) : base(CollectorCore, paths)
        {
        }
    }
}
