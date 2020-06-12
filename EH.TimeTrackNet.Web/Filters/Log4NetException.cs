using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace EH.TimeTrackNet.Web.Filters
{
    public class Log4NetException : IExceptionFilter
    {
        protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void OnException(ExceptionContext filterContext)
        {
            Exception exception = filterContext.Exception;
            var request = filterContext.HttpContext.Request;
            log.Error("Unhandled Exception Occured at " + request.RawUrl, exception);
        }
    }
}