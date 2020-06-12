using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EH.TimeTrackNet.Web.Utilities
{
    public partial class SessionHelper
    {
        public static T Read<T>(string key)
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null || HttpContext.Current.Session[key] == null)
                return default(T);
            else
                return (T)HttpContext.Current.Session[key];
        }

        public static void Write(string key, object value)
        {
            HttpContext.Current.Session[key] = value;
        }
    }
}