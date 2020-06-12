using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EH.TimeTrackNet.Web.Utilities
{
    public class User
    {
        public string CommonName { get; set; }
        public string Department { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string Domain { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastLogon { get; set; }
        public string LastName { get; set; }
        public string Manager { get; set; }
        public List<string> ManagerList { get; set; }
        public List<string> OrganizationUnitList { get; set; }
        public string Phone { get; set; }
        public string PrincipalName { get; set; }
        public string Title { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
} 