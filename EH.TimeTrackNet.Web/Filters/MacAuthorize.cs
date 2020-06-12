using log4net;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EH.TimeTrackNet.Web.Filters
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class MacAuthorize : AuthorizeAttribute
    {
        protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string Action { get; set; }
        public string Permission { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            bool isPermitted = false;
            IMacPrincipal user = null;
            try
            {
                ControllerBase currentController = (ControllerBase)filterContext.Controller;
                if (Action == null)
                    Action = string.Empty;
                if (Permission == null)
                    Permission = string.Empty;

                //if (HttpContext.Current.Request.IsAuthenticated && (!string.IsNullOrEmpty(Permission) || !string.IsNullOrEmpty(Action)))
                if (!string.IsNullOrEmpty(Permission) || !string.IsNullOrEmpty(Action))
                {
                    user = new MacPrincipal(HttpContext.Current.User);
                    if (!string.IsNullOrEmpty(Permission))
                        isPermitted = user.HasPermission(Permission);
                    if (!isPermitted && !string.IsNullOrEmpty(Action))
                        isPermitted = user.HasAction(Action);
                }
                else
                {
                    isPermitted = false;
                }
            }
            catch
            {
                isPermitted = false;
            }

            if (!isPermitted)
            {
                if (!filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                     {
                         {"controller", "Error"},
                         {"action", "Forbidden"}
                     });
                }
                else
                {
                    var errors = new List<string> { "You are not authorized to perform this action. Please contact the application administrator for further information." };
                    filterContext.Result = new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { errors }
                    };
                    filterContext.RequestContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
            else if (HttpContext.Current.Request.IsAuthenticated)
            {
                base.OnAuthorization(filterContext);
            }
        }
    }
}