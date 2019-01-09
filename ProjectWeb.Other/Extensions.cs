using System;
using System.Runtime.ExceptionServices;

namespace ProjectWeb.Other
{
    public static class Extensions
    {
        public static void Die(string message)
        {
            throw new Exception(message);
        }
        public static void Die(Exception ex)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();            
        }
    }
}
