using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EH.TimeTrackNet.Web.Models;

namespace EH.TimeTrackNet.Web.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet]
        public ActionResult Generic()
        {
            Response.StatusCode = 500;
            return View();
        }

        [HttpGet]
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View();
        }

        [HttpGet]
        public ActionResult InternalServerError(string ErrorMessage = "")
        {
            //Response.StatusCode = 500;
            EHTT_Error error = new EHTT_Error();
            if (Session["ErrorMessage"] == null && ErrorMessage == "")
            {
                error.EHTT_ErrorMessage = "";
            }
            else
            {
                if (ErrorMessage == "")
                {
                    error.EHTT_ErrorMessage = Session["ErrorMessage"].ToString();
                }
                else
                {
                    error.EHTT_ErrorMessage = ErrorMessage;
                };

            };
            return View(error);
        }

        [HttpGet]
        public ActionResult Unauthorized()
        {
            Response.StatusCode = 401;
            return View();
        }

        [HttpGet]
        public ActionResult Forbidden()
        {
            Response.StatusCode = 403;
            return View();
        }
    }
}