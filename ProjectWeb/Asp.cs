using ProjectWeb.Global;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ProjectWeb
{
    public class Asp : IProjectWeb
    {
        public void ProcessAsync(object o)
        {
            WebHelper.ProcessAsync(o as HttpContext);
        }
        
        public void Start()
        {
            throw new NotImplementedException();
        }

    }
}
