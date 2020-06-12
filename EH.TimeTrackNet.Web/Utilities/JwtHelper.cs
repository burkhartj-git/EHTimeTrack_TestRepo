using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace EH.TimeTrackNet.Web.Utilities
{
    public class JwtHelper
    {
        public static string GetTokenValueByKey(string token, string key)
        {
            var value = string.Empty;

            var parsedToken = string.Empty;
            if (!string.IsNullOrEmpty(token) && token.Contains('.'))
            {
                parsedToken = token.Split('.')[1].Replace(" ", "+");
                if (parsedToken.Length % 4 > 0)
                    parsedToken += new string('=', 4 - (parsedToken.Length % 4));
                byte[] encryptedBytes = System.Convert.FromBase64String(parsedToken);
                parsedToken = System.Text.ASCIIEncoding.ASCII.GetString(encryptedBytes);
            }

            if (!string.IsNullOrEmpty(parsedToken) && !string.IsNullOrEmpty(key))
            {
                dynamic json = System.Web.Helpers.Json.Decode(parsedToken);
                value = json[key];
            }

            return value;
        }

        public static void SetSessionFromTokenCookie(HttpRequestBase request, string tokenCookie)
        {
            var token = string.Empty;

            if (request.Cookies != null && request.Cookies[tokenCookie] != null && request.Cookies[tokenCookie].Value != null)
                token = request.Cookies[tokenCookie].Value;
            if (string.IsNullOrEmpty(token) && request.Cookies != null && request.Cookies[tokenCookie + "-L"] != null && request.Cookies[tokenCookie + "-L"].Value != null)
                token = request.Cookies[tokenCookie + "-L"].Value;

            SessionHelper.AccessToken = token;
            SessionHelper.DisplayName = GetTokenValueByKey(token, "DisplayName");
            SessionHelper.UserName = GetTokenValueByKey(token, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn");
            SessionHelper.UserSignedIn = DateTime.Now.Ticks.ToString();
        }
    }
}