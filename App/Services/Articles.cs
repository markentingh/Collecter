using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Collector.Services
{
    public class Articles : Service
    {
        public Articles(HttpContext context) : base(context) {}


    }
}
