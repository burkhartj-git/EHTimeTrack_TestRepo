using log4net;
using EH.TimeTrackNet.Web.Utilities;
using Microsoft.AspNet.Identity;
using log4net;
using EH.TimeTrackNet.Web.Utilities;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;

namespace EH.TimeTrackNet.Web.Controllers
{
    public class LayoutController : ApiController
    {
        protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [HttpGet]
        [Route("api/layout/security/accesstoken")]
        public HttpResponseMessage GetAccessToken()
        {
            return Request.CreateResponse(HttpStatusCode.OK, SessionHelper.AccessToken);
        }

        [HttpGet]
        [Route("api/layout/security/applicationname")]
        public HttpResponseMessage GetApplicationName()
        {
            return Request.CreateResponse(HttpStatusCode.OK, ConfigurationManager.AppSettings["Application.Name"]);
        }

        [HttpGet]
        [Route("api/layout/security/authorizationclientbaseaddress")]
        public HttpResponseMessage GetAuthorizationClientBaseAddress()
        {
            return Request.CreateResponse(HttpStatusCode.OK, ConfigurationManager.AppSettings["OAuth.MacombCounty.Paths.Remote.AuthorizationClient"]);
        }

        [HttpPost]
        [Route("api/layout/security/authorizationclientresponse")]
        public void GetAuthorizationClientResponse(string accessToken, string displayName, string userName)
        {
            SessionHelper.AccessToken = accessToken;
            SessionHelper.DisplayName = displayName;
            SessionHelper.UserName = userName;
        }

        [HttpGet]
        [Route("api/layout/security/baseaddress")]
        public HttpResponseMessage GetApiBaseAddress()
        {
            return Request.CreateResponse(HttpStatusCode.OK, Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/");
        }

        [HttpGet]
        [Route("api/layout/security/refreshtoken")]
        public HttpResponseMessage GetRefreshToken()
        {
            return Request.CreateResponse(HttpStatusCode.OK, SessionHelper.RefreshToken);
        }

        [HttpGet]
        [Route("api/layout/security/resourceserverbaseaddress")]
        public HttpResponseMessage GetResourceServerBaseAddress()
        {
            return Request.CreateResponse(HttpStatusCode.OK, ConfigurationManager.AppSettings["OAuth.MacombCounty.Paths.Remote.ResourceServerBaseAddress"]);
        }

        [HttpGet]
        [Route("api/layout/security/ssrsreporturl")]
        public HttpResponseMessage GetSSRSUrl()
        {
            return Request.CreateResponse(HttpStatusCode.OK, ConfigurationManager.AppSettings["Ssrs.WebService.Url"]);
        }

        [HttpGet]
        [Route("api/layout/security/ssrsreportpath")]
        public HttpResponseMessage GetSSRSReportFolder()
        {
            return Request.CreateResponse(HttpStatusCode.OK, ConfigurationManager.AppSettings["Ssrs.Report.Path"]);
        }

        [HttpGet]
        [Route("api/layout/security/state")]
        public HttpResponseMessage GetState()
        {
            var state = SessionHelper.State;
            if (string.IsNullOrEmpty(state))
            {
                state = DateTime.Now.Ticks.ToString();
                SessionHelper.State = state;
            }

            return Request.CreateResponse(HttpStatusCode.OK, state);
        }

        [HttpGet]
        [Route("api/layout/security/username")]
        public HttpResponseMessage GetUserName()
        {
            return Request.CreateResponse(HttpStatusCode.OK, User.Identity.GetUserName());
        }

        [HttpGet]
        [Route("api/layout/security/userdisplayname")]
        public HttpResponseMessage GetUserDisplayName()
        {
            return Request.CreateResponse(HttpStatusCode.OK, SessionHelper.DisplayName);
        }

        [HttpGet]
        [Route("api/layout/sidebarclosed")]
        public HttpResponseMessage GetSideBarClosed()
        {
            return Request.CreateResponse(HttpStatusCode.OK, SessionHelper.SideBarClosed);
        }

        [HttpPost]
        [Route("api/layout/sidebarclosed/{isclosed}")]
        public HttpResponseMessage GetSideBarClosed(string isClosed)
        {
            try
            {
                SessionHelper.SideBarClosed = isClosed;
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                log.Error(e.StackTrace);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
            return Request.CreateResponse(HttpStatusCode.OK, SessionHelper.SideBarClosed);
        }
    }
}