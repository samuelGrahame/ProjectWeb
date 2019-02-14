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

namespace ProjectWeb
{
    public class Standalone : IProjectWeb
    {
        public string[] Prefixes;
        public bool Running = false;

        public Standalone(string[] prefixes)
        {
            if (prefixes == null || prefixes.Length == 0)
                prefixes = new[] { "http://127.0.0.1:8180/", "http://localhost:8180/" };
            Prefixes = prefixes;
        }

        public void ProcessAsync(object o)
        {            
            WebHelper.ProcessAsync(o as HttpListenerContext);
        }

        public void Start()
        {
            if (Running)
                return;

            HttpListener listener = new HttpListener();
            foreach (var prefixes in Prefixes)
            {
                listener.Prefixes.Add(prefixes);
            }
            
            listener.Start();

            Console.WriteLine("Started");
            Running = true;

            while (Running)
            {
                ThreadPool.QueueUserWorkItem(ProcessAsync, listener.GetContext());
            }
            Console.WriteLine("Stopped");
            listener.Stop();
        }

        public void Stop()
        {
            Running = false;
        }
    }
}
