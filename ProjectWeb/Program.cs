using Microsoft.CodeAnalysis.CSharp.Scripting;
using ProjectWeb.Global;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ProjectWeb.Document;

namespace ProjectWeb
{
    class Program
    {
        static bool Running = true;
        static void Main(string[] args)
        {
            //var state = await CSharpScript.RunAsync("int x = 1;");
            //state = await state.ContinueWithAsync("int y = 2;");
            //state = await state.ContinueWithAsync("x+y");
            //Console.WriteLine(state.ReturnValue);            
            // Create a listener.
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:8180/");
            listener.Prefixes.Add("http://localhost:8180/");

            listener.Start();
            
            Console.WriteLine("Started");
            while (Running)
            {
                ThreadPool.QueueUserWorkItem(ProcessAsync, listener.GetContext());
            }
            Console.WriteLine("Stopped");
            listener.Stop();            
        }

        static void Echo(ref string data, HttpListenerContext context)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);            
            context.Response.ContentLength64 = buffer.Length;
            using (Stream st = context.Response.OutputStream)
            {
                st.Write(buffer, 0, buffer.Length);
            }
        }

        static async void ProcessAsync(object o)
        {
            var context = o as HttpListenerContext;
            // process request and make response
            string fileRequest = context.Request.Url.LocalPath;
            if(string.IsNullOrWhiteSpace(fileRequest) || fileRequest == "/")
            {
                fileRequest = "index.html";
            }

            string page = $"{Directory.GetCurrentDirectory()}/wwwroot/{fileRequest}";

            if(context.Request.Url.LocalPath.EndsWith(".html"))
            {
                string result;
                try
                {
                    var st = Stopwatch.StartNew();

                    string ct = context.Request.ContentType;
                    bool isHTML = ct == null || ct.ToLower().Trim() == "text/html";

                    var stateManager = new StateManager()
                    {
                        Globals = new Params
                        {
                            builder = new StringBuilder()   ,
                            Context = context
                        }
                    };

                    if (isHTML)
                    {
                        stateManager.Globals.builder.AppendLine("<html>");
                    }

                    var absPath = Path.GetFullPath(fileRequest.ToLower());
                    
                    if(StateCache.Cache.ContainsKey(absPath))
                    {
                        await StateCache.Cache[absPath].Run(stateManager);
                    }
                    else
                    {
                        var file = new Document.IncludedFiles();
                        file.Files.Add(fileRequest.ToLower());
                        TextReader tr = new StreamReader(page);

                        await Document.ParseAsync(tr.ReadToEnd(), stateManager, file);

                        if (!StateCache.Cache.ContainsKey(absPath))
                        {
                            StateCache.Cache[absPath] = stateManager.StateCache;
                        }
                    }

                    if(isHTML)
                    {
                        stateManager.Globals.builder.AppendLine("</html>");
                    }                    

                    st.Stop();

                    if (context.Request.IsLocal)
                    {
                        //builder.AppendLine($"<-- Total Execute Time: {st.ElapsedMilliseconds}ms -->");
                    }

                    result = stateManager.Globals.builder.ToString();

                    Echo(ref result, context);
                }
                catch (Exception ex)
                {
                    if (context.Request.IsLocal)
                    {
                        result = ex.ToString();
                        Echo(ref result, context);
                    }
                    context.Response.StatusCode = 500;
                }
                
                context.Response.Close();  // here we close the connection
            }
            
        }
    }
}
