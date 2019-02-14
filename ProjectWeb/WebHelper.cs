using ProjectWeb.Global;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ProjectWeb
{
    public static class WebHelper
    {
        public static void Echo(ref string data, HttpListenerContext context)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            context.Response.ContentLength64 = buffer.Length;
            using (Stream st = context.Response.OutputStream)
            {
                st.Write(buffer, 0, buffer.Length);
            }
        }
        public static void Echo(ref string data, HttpContext context)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);            
            using (Stream st = context.Response.OutputStream)
            {
                st.Write(buffer, 0, buffer.Length);
            }
        }

        public class RequestContextInfo
        {
            public string FileRequest;
            public string ContentType;
            public string Page;
            public StateManager StateManager;
            public bool IsLocal;            
        }

        public static RequestContextInfo GetRequestPath<T>(T context)
        {
            RequestContextInfo requestContextInfo = new RequestContextInfo();
            string fileRequest = "";
            if(context is HttpContext httpContext)
            {
                fileRequest = httpContext.Request.Url.LocalPath;
                requestContextInfo.ContentType = httpContext.Request.ContentType;
                requestContextInfo.StateManager = new StateManager()
                {
                    Builder = new StringBuilder()
                };
                requestContextInfo.StateManager.Globals = new AspParams()
                {
                    builder = requestContextInfo.StateManager.Builder,
                    Context = httpContext
                };
                requestContextInfo.IsLocal = httpContext.Request.IsLocal;

            }
            else if (context is HttpListenerContext httpContextListener)
            {
                fileRequest = httpContextListener.Request.Url.LocalPath;
                requestContextInfo.ContentType = httpContextListener.Request.ContentType;
                requestContextInfo.StateManager = new StateManager()
                {
                    Builder = new StringBuilder()
                };
                requestContextInfo.StateManager.Globals = new StandaloneParams()
                {
                    builder = requestContextInfo.StateManager.Builder,
                    Context = httpContextListener
                };
                requestContextInfo.IsLocal = httpContextListener.Request.IsLocal;
            }
            if (string.IsNullOrWhiteSpace(fileRequest) || fileRequest == "/")
            {
                fileRequest = "index.html";
            }            
            if (fileRequest.EndsWith(".html"))
            {
                requestContextInfo.Page = $"{Directory.GetCurrentDirectory()}/wwwroot/{fileRequest}";
                requestContextInfo.FileRequest = fileRequest;                
                return requestContextInfo;
            }
            return null;
        }

        public static async void ProcessAsync<T>(T context)
        {
            // process request and make response
            HttpContext httpContext = context is HttpContext ? context as HttpContext  : null;
            HttpListenerContext httpListenerContext = context is HttpListenerContext ? context as HttpListenerContext : null;
            if(httpContext == null && httpListenerContext == null)
            {
                throw new NotImplementedException();
            }
            bool UseDefault = httpContext != null;

            var requestInfo = GetRequestPath(context);
            if (requestInfo != null)
            {
                string result;
                try
                {
                    var st = Stopwatch.StartNew();

                    string ct = requestInfo.ContentType;
                    bool isHTML = ct == null || ct.ToLower().Trim() == "text/html";
                    var stateManager = requestInfo.StateManager;
                    if (isHTML)
                    {
                        stateManager.Builder.AppendLine("<html>");
                    }
                    var fileRequestToLower = requestInfo.FileRequest.ToLower();
                    var absPath = Path.GetFullPath(fileRequestToLower);

                    if (StateCache.Cache.ContainsKey(absPath))
                    {
                        await StateCache.Cache[absPath].Run(stateManager);
                    }
                    else
                    {
                        var file = new Document.IncludedFiles();
                        file.Files.Add(fileRequestToLower);
                        TextReader tr = new StreamReader(requestInfo.Page);

                        await Document.ParseAsync(tr.ReadToEnd(), stateManager, file);

                        if (!StateCache.Cache.ContainsKey(absPath))
                        {
                            StateCache.Cache[absPath] = stateManager.StateCache;
                        }
                    }

                    if (isHTML && stateManager.Builder.Length > 0)
                    {
                        ct = requestInfo.StateManager.Globals.GetResponseContentType();
                        isHTML = ct == null || ct.ToLower().Trim() == "text/html";
                        if (isHTML)
                        {
                            stateManager.Builder.AppendLine("</html>");
                        }
                    }

                    st.Stop();

                    result = stateManager.Builder.ToString();
                    // if the user has written out aka download file down pass html
                    if (result.Length > 0)
                    {
                        if(UseDefault)
                        {
                            WebHelper.Echo(ref result, httpContext);
                        }
                        else
                        {
                            WebHelper.Echo(ref result, httpListenerContext);
                        }                       
                    }
                }
                catch (Exception ex)
                {                    
                    if (requestInfo.IsLocal)
                    {
                        result = ex.ToString();
                        if (UseDefault)
                        {
                            WebHelper.Echo(ref result, httpContext);
                        }
                        else
                        {
                            WebHelper.Echo(ref result, httpListenerContext);
                        }
                    }
                    if (UseDefault)
                    {
                        httpContext.Response.StatusCode = 500;
                    }
                    else 
                    {
                        httpListenerContext.Response.StatusCode = 500;
                    }
                    
                }
                if (UseDefault)
                {
                    httpContext.Response.Close();
                }
                else
                {
                    httpListenerContext.Response.Close();
                }

            }
        }
    }
}
