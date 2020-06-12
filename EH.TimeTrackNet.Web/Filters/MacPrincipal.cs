using EH.TimeTrackNet.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace EH.TimeTrackNet.Web.Filters
{
    public interface IMacPrincipal : IPrincipal
    {
        IEnumerable<string> Actions { get; }
        IEnumerable<string> Permissions { get; }
        IEnumerable<string> Roles { get; }
        bool HasAction(string action);
        bool HasPermission(string permission);
    }

    public class MacPrincipal : IMacPrincipal
    {
        private readonly IPrincipal _principal;
        private bool _isAuthenticated;
        private bool _isInitialized;
        private IEnumerable<AspNetRole> _aspNetRoles;
        private IEnumerable<AspNetPermission> _aspNetPermissions;
        private IEnumerable<string> _actions;
        private IEnumerable<string> _permissions;
        private IEnumerable<string> _roles;

        public MacPrincipal(IPrincipal principal) { _principal = principal; }

        public IEnumerable<string> Actions { get { return getActionsForUser(); } }
        public IIdentity Identity { get { return _principal.Identity; } }
        public IEnumerable<string> Permissions { get { return getPermissionsForUser(); } }
        public IEnumerable<string> Roles { get { return getRolesForUser(); } }

        public bool HasAction(string action) { return hasAction(action); }
        public bool HasPermission(string permission) { return hasPermission(permission); }
        public bool IsInRole(string role) { return isInRole(role); }

        private IEnumerable<string> getActionsForUser()
        {
            _isAuthenticated = HttpContext.Current.Session["MacPrincipal.IsAuthenticated"] == null ? false : (bool)HttpContext.Current.Session["MacPrincipal.IsAuthenticated"];
            _isInitialized = HttpContext.Current.Session["MacPrincipal.IsInitialized"] == null ? false : (bool)HttpContext.Current.Session["MacPrincipal.IsInitialized"];
            if (!_isInitialized || _principal.Identity.IsAuthenticated ^ _isAuthenticated)
            {
                preparePrincipalCollections();
            }
            _aspNetPermissions = HttpContext.Current.Session["MacPrincipal.AspNetPermissions"] == null ? null : (IEnumerable<AspNetPermission>)HttpContext.Current.Session["MacPrincipal.AspNetPermissions"];
            _actions = HttpContext.Current.Session["MacPrincipal.Actions"] == null ? null : (IEnumerable<string>)HttpContext.Current.Session["MacPrincipal.Actions"];
            if ((_actions == null || _actions.Count() == 0) && _aspNetPermissions != null && _aspNetPermissions.Count() > 0)
            {
                var actions = new List<string>();
                foreach (var permission in _aspNetPermissions)
                {
                    if (permission.AddNew && !actions.Contains("AddNew"))
                        actions.Add("AddNew");
                    if (permission.AdministerAll && !actions.Contains("AdministerAll"))
                        actions.Add("AdministerAll");
                    if (permission.AdministerOwn && !actions.Contains("AdministerOwn"))
                        actions.Add("AdministerOwn");
                    if (permission.DeleteAll && !actions.Contains("DeleteAll"))
                        actions.Add("DeleteAll");
                    if (permission.DeleteOwn && !actions.Contains("DeleteOwn"))
                        actions.Add("DeleteOwn");
                    if (permission.EditAll && !actions.Contains("EditAll"))
                        actions.Add("EditAll");
                    if (permission.EditOwn && !actions.Contains("EditOwn"))
                        actions.Add("EditOwn");
                    if (permission.ExpungeAll && !actions.Contains("ExpungeAll"))
                        actions.Add("ExpungeAll");
                    if (permission.ExpungeOwn && !actions.Contains("ExpungeOwn"))
                        actions.Add("ExpungeOwn");
                    if (permission.ViewAll && !actions.Contains("ViewAll"))
                        actions.Add("ViewAll");
                    if (permission.ViewOwn && !actions.Contains("ViewOwn"))
                        actions.Add("ViewOwn");
                    if (permission.ViewPublished && !actions.Contains("ViewPublished"))
                        actions.Add("ViewPublished");
                }
                if (actions.Count() > 0)
                {
                    _actions = actions.ToArray();
                    HttpContext.Current.Session["MacPrincipal.Actions"] = _actions;
                }
            }
            if (_actions != null && _actions.Count() > 0)
            {
                return _actions;
            }

            return new string[0];
        }

        private IEnumerable<string> getPermissionsForUser()
        {
            _isAuthenticated = HttpContext.Current.Session["MacPrincipal.IsAuthenticated"] == null ? false : (bool)HttpContext.Current.Session["MacPrincipal.IsAuthenticated"];
            _isInitialized = HttpContext.Current.Session["MacPrincipal.IsInitialized"] == null ? false : (bool)HttpContext.Current.Session["MacPrincipal.IsInitialized"]; 
            if (!_isInitialized || _principal.Identity.IsAuthenticated ^ _isAuthenticated)
            {
                preparePrincipalCollections();
            }
            _aspNetPermissions = HttpContext.Current.Session["MacPrincipal.AspNetPermissions"] == null ? null : (IEnumerable<AspNetPermission>)HttpContext.Current.Session["MacPrincipal.AspNetPermissions"];
            _permissions = HttpContext.Current.Session["MacPrincipal.Permissions"] == null ? null : (IEnumerable<string>)HttpContext.Current.Session["MacPrincipal.Permissions"];
            if ((_permissions == null || _permissions.Count() == 0) && _aspNetPermissions != null && _aspNetPermissions.Count() > 0)
            {
                _permissions = _aspNetPermissions.Select(p => p.Name);
                HttpContext.Current.Session["MacPrincipal.Permissions"] = _permissions;
            }
            if (_permissions != null && _permissions.Count() > 0)
            {
                return _permissions;
            }

            return new string[0];
        }

        private IEnumerable<string> getRolesForUser()
        {
            _isAuthenticated = HttpContext.Current.Session["MacPrincipal.IsAuthenticated"] == null ? false : (bool)HttpContext.Current.Session["MacPrincipal.IsAuthenticated"];
            _isInitialized = HttpContext.Current.Session["MacPrincipal.IsInitialized"] == null ? false : (bool)HttpContext.Current.Session["MacPrincipal.IsInitialized"];
            if (!_isInitialized || _principal.Identity.IsAuthenticated ^ _isAuthenticated)
            {
                preparePrincipalCollections();
            }
            _aspNetRoles = HttpContext.Current.Session["MacPrincipal.AspNetRoles"] == null ? null : (IEnumerable<AspNetRole>)HttpContext.Current.Session["MacPrincipal.AspNetRoles"];
            _roles = HttpContext.Current.Session["MacPrincipal.Roles"] == null ? null : (IEnumerable<string>)HttpContext.Current.Session["MacPrincipal.Roles"];
            if ((_roles == null || _roles.Count() == 0) && _aspNetRoles != null && _aspNetRoles.Count() > 0)
            {
                _roles = _aspNetRoles.Select(p => p.Name);
                HttpContext.Current.Session["MacPrincipal.Roles"] = _roles;
            }
            if (_roles != null && _roles.Count() > 0)
            {
                return _roles;
            }

            return new string[0];
        }

        private bool hasAction(string action)
        {
            foreach (var userAction in getActionsForUser())
            {
                if (action.Contains(','))
                {
                    foreach (var item in action.Split(','))
                    {
                        if (userAction.ToUpper() == item.ToUpper())
                            return true;
                    }
                }
                else if (userAction.ToUpper() == action.ToUpper())
                {
                        return true;
                }
            }

            return false;
        }

        private bool hasPermission(string permission)
        {
            foreach (var userPermission in getPermissionsForUser())
            {
                if (permission.Contains(','))
                {
                    foreach (var item in permission.Split(','))
                    {
                        if (userPermission.ToUpper() == item.ToUpper())
                            return true;
                    }
                }
                else if (userPermission.ToUpper() == permission.ToUpper())
                {
                    return true;
                }
            }

            return false;
        }

        private bool isInRole(string role)
        {
            foreach (var userRole in getRolesForUser())
            {
                if (role.Contains(','))
                {
                    foreach (var item in role.Split(','))
                    {
                        if (userRole.ToUpper() == item.ToUpper())
                            return true;
                    }
                }
                else if (userRole.ToUpper() == role.ToUpper())
                {
                    return true;
                }
            }

            return false; 
        }

        private void preparePrincipalCollections()
        {
            _aspNetRoles = null;
            _aspNetPermissions = null;
            _actions = null;
            _permissions = null;
            _roles = null;
            HttpContext.Current.Session["MacPrincipal.AspNetRoles"] = _aspNetRoles;
            HttpContext.Current.Session["MacPrincipal.AspNetPermissions"] = _aspNetPermissions;
            HttpContext.Current.Session["MacPrincipal.Actions"] = _actions;
            HttpContext.Current.Session["MacPrincipal.Permissions"] = _permissions;
            HttpContext.Current.Session["MacPrincipal.Roles"] = _roles;

            using (var db = new RoleProviderSqlEntities())
            {
                var roles = new List<string>();
                var user = db.AspNetUsers.Where(u => u.Inactive == false && u.UserName.ToUpper() == _principal.Identity.Name.ToUpper()).FirstOrDefault();

                // Anonymous
                var roleAnonymous = db.AspNetRoles.Where(r => r.Inactive == false && r.Name.ToUpper() == "ANONYMOUS").FirstOrDefault();
                if (roleAnonymous != null)
                    roles.Add(roleAnonymous.Id);
                
                var application = ConfigurationManager.AppSettings["Application.Name"].ToUpper();
                var aspNetApplication = db.AspNetApplications.Where(a => a.Inactive == false && a.Name.ToUpper() == application.ToUpper()).FirstOrDefault();
                if (aspNetApplication != null && _principal.Identity.IsAuthenticated)
                {
                    // Authenticated
                    var roleAuthenticated = db.AspNetRoles.Where(r => r.Inactive == false && r.Name.ToUpper() == "AUTHENTICATED").FirstOrDefault();
                    if (roleAuthenticated != null)
                        roles.Add(roleAuthenticated.Id);

                    // Member
                    if (user != null)
                    {
                        // Application User Role
                        var aspNetApplicationUserRoles = db.AspNetApplicationUserRoles.Where(aur => aur.Inactive == false && aur.AspNetApplication.Id == aspNetApplication.Id && aur.AspNetUser.Id == user.Id);
                        if (aspNetApplicationUserRoles != null)
                            roles.AddRange(aspNetApplicationUserRoles.Select(aur => aur.RoleId));
                        // User Role
                        var aspNetUserRoles = user.AspNetRoles.Where(r => r.Inactive == false);
                        if (aspNetUserRoles != null)
                            roles.AddRange(aspNetUserRoles.Select(ur => ur.Id));
                    }
                }
                _aspNetRoles = db.AspNetRoles.Where(r => r.Inactive == false && roles.Contains(r.Id)).ToList();
                HttpContext.Current.Session["MacPrincipal.AspNetRoles"] = _aspNetRoles;

                // System Administrator
                var roleSystemAdministrator = db.AspNetRoles.Where(r => r.Inactive == false && r.Name.ToUpper() == "SYSTEM ADMINISTRATOR").FirstOrDefault();
                //if (roleSystemAdministrator != null && roles.Contains(roleSystemAdministrator.Id))
                if (roleSystemAdministrator != null && user != null && user.AspNetRoles.Contains(roleSystemAdministrator))
                {

                    _aspNetPermissions = db.AspNetPermissions.Where(p => p.Inactive == false).ToList();
                    HttpContext.Current.Session["MacPrincipal.AspNetPermissions"] = _aspNetPermissions;
                }
                else if (aspNetApplication != null)
                {
                    var aspNetApplicationRolePermissions = db.AspNetApplicationRolePermissions.Where(arp => arp.Inactive == false && arp.AspNetApplication.Id == aspNetApplication.Id && roles.Contains(arp.AspNetRole.Id));
                    if (aspNetApplicationRolePermissions != null)
                    {
                        _aspNetPermissions = aspNetApplicationRolePermissions.Select(arp => arp.AspNetPermission).ToList();
                        HttpContext.Current.Session["MacPrincipal.AspNetPermissions"] = _aspNetPermissions;
                    }
                    if (_aspNetPermissions.Count() == 0)
                    {
                        _aspNetPermissions = null;
                        HttpContext.Current.Session["MacPrincipal.AspNetPermissions"] = _aspNetPermissions;
                    }
                }
            }

            _isAuthenticated = _principal.Identity.IsAuthenticated;
            HttpContext.Current.Session["MacPrincipal.IsAuthenticated"] = _isAuthenticated;
            _isInitialized = true;
            HttpContext.Current.Session["MacPrincipal.IsInitialized"] = _isInitialized;
        }
    }

    public class BaseController : Controller
    {
        protected virtual new IMacPrincipal User
        {
            get { return (IMacPrincipal)base.User; }
        }

        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonNetResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
        }
    }

    public class JsonNetResult : JsonResult
    {
        public JsonNetResult()
        {
            Settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
            };
        }

        public Newtonsoft.Json.JsonSerializerSettings Settings { get; private set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (this.JsonRequestBehavior == JsonRequestBehavior.DenyGet && string.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("JSON GET is not allowed");

            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType = string.IsNullOrEmpty(this.ContentType) ? "application/json" : this.ContentType;

            if (this.ContentEncoding != null)
                response.ContentEncoding = this.ContentEncoding;
            if (this.Data == null)
                return;

            var scriptSerializer = Newtonsoft.Json.JsonSerializer.Create(this.Settings);

            using (var stringWriter = new System.IO.StringWriter())
            {
                scriptSerializer.Serialize(stringWriter, this.Data);
                response.Write(stringWriter.ToString());
            }
        }
    }
    public abstract class BaseViewPage : WebViewPage
    {
        public virtual new IMacPrincipal User
        {
            get { return (IMacPrincipal)base.User; }
        }
    }

    public abstract class BaseViewPage<TModel> : WebViewPage<TModel>
    {
        public virtual new IMacPrincipal User
        {
            get { return (IMacPrincipal)base.User; }
        }
    }
}