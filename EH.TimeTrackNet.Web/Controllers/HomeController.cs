using DotNetOpenAuth.OAuth2;
using log4net;
using EH.TimeTrackNet.Web.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using EH.TimeTrackNet.Web.Filters;

namespace EH.TimeTrackNet.Web.Controllers
{
    public class HomeController : BaseController
    {
        protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult About()
        {
            if (string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.About.Defined"]) || System.Configuration.ConfigurationManager.AppSettings["Application.About.Defined"].ToLower() == "false")
                RedirectToAction("Home");

            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.About.BodyHtml"]))
                ViewBag.BodyHtml = HttpUtility.HtmlDecode(System.Configuration.ConfigurationManager.AppSettings["Application.About.BodyHtml"]);

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Help()
        {
            if (string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.Help.Defined"]) || System.Configuration.ConfigurationManager.AppSettings["Application.Help.Defined"].ToLower() == "false")
                RedirectToAction("Home");

            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.Help.BodyHtml"]))
                ViewBag.BodyHtml = HttpUtility.HtmlDecode(System.Configuration.ConfigurationManager.AppSettings["Application.Help.BodyHtml"]);

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ContactUs()
        {
            if (string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.Defined"]) || System.Configuration.ConfigurationManager.AppSettings["Application.Contact.Defined"].ToLower() == "false")
                RedirectToAction("Home");

            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.BodyHtml"]))
                ViewBag.BodyHtml = HttpUtility.HtmlDecode(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.BodyHtml"]);

            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.AddressHtml"]))
                ViewBag.AddressHtml = HttpUtility.HtmlDecode(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.AddressHtml"]);
            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.EmailHtml"]))
                ViewBag.EmailHtml = HttpUtility.HtmlDecode(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.EmailHtml"]);
            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.HeaderText"]))
                ViewBag.HeaderText = System.Configuration.ConfigurationManager.AppSettings["Application.Contact.HeaderText"];
            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.PhoneHtml"]))
                ViewBag.PhoneHtml = HttpUtility.HtmlDecode(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.PhoneHtml"]);

            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.FacebookUrl"]))
                ViewBag.FacebookUrl = System.Configuration.ConfigurationManager.AppSettings["Application.Contact.FacebookUrl"];
            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.GitHubUrl"]))
                ViewBag.GitHubUrl = System.Configuration.ConfigurationManager.AppSettings["Application.Contact.GitHubUrl"];
            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.GooglePlusUrl"]))
                ViewBag.GooglePlusUrl = System.Configuration.ConfigurationManager.AppSettings["Application.Contact.GooglePlusUrl"];
            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.InstagramUrl"]))
                ViewBag.InstagramUrl = System.Configuration.ConfigurationManager.AppSettings["Application.Contact.InstagramUrl"];
            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.LinkedInUrl"]))
                ViewBag.LinkedInUrl = System.Configuration.ConfigurationManager.AppSettings["Application.Contact.LinkedInUrl"];
            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.RssUrl"]))
                ViewBag.RssUrl = System.Configuration.ConfigurationManager.AppSettings["Application.Contact.RssUrl"];
            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.SkypeUrl"]))
                ViewBag.SkypeUrl = System.Configuration.ConfigurationManager.AppSettings["Application.Contact.SkypeUrl"];
            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.TwitterUrl"]))
                ViewBag.TwitterUrl = System.Configuration.ConfigurationManager.AppSettings["Application.Contact.TwitterUrl"];
            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Application.Contact.YouTubeUrl"]))
                ViewBag.YouTubeUrl = System.Configuration.ConfigurationManager.AppSettings["Application.Contact.YouTubeUrl"];

            return View();
        }

        //
        // GET: /Home/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            try
            {
                if (!Request.IsAuthenticated && !string.IsNullOrEmpty(SessionHelper.UserName))
                {
                    var identity = new ClaimsIdentity(
                        new[] { new Claim(ClaimTypes.Name, SessionHelper.UserName) }
                        , DefaultAuthenticationTypes.ApplicationCookie
                        , ClaimTypes.Name
                        , ClaimTypes.Role
                    );

                    identity.AddClaim(new Claim(ClaimTypes.Role, "Authenticated"));

                    Authentication.SignIn(new AuthenticationProperties {IsPersistent = false}, identity);

                    SessionHelper.UserSignedIn = DateTime.Now.Ticks.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Home/LogOff
        public ActionResult LogOff()
        {
            Authentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
        
        IAuthenticationManager Authentication
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }
    }
}