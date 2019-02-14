using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectWebAspTest
{
    public class HandleAllRequestsHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var task = ProjectWeb.WebHelper.ProcessAsync(context);
            task.Wait();
        }

        public bool IsReusable => true;
    }
}