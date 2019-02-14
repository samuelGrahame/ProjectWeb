using System;
using System.Net;
using System.Text;
using System.Web;

namespace ProjectWeb.Global
{
    public class Params<T> : IParams
    {
        public StringBuilder builder;
        public T Context;

        public virtual string GetResponseContentType()
        {
            return string.Empty;
        }
    }

    public interface IParams
    {
        string GetResponseContentType();
    }

    public class StandaloneParams : Params<HttpListenerContext>
    {
        public override string GetResponseContentType()
        {
            return this.Context.Response.ContentType;
        }
    }

    public class AspParams : Params<HttpContext>
    {
        public override string GetResponseContentType()
        {
            return this.Context.Response.ContentType;
        }
    }
}
