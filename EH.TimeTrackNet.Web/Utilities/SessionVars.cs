using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EH.TimeTrackNet.Web.Utilities
{
    public partial class SessionHelper
    {
        //Session variable constants
        private const string ACCESS_TOKEN = "OAuth.AccessToken";
        private const string DISPLAY_NAME = "OAuth.DisplayName";
        private const string REFRESH_TOKEN = "OAuth.RefreshToken";
        private const string SIDEBAR_CLOSED = "sidebar_closed";
        private const string STATE = "OAuth.State";
        private const string USER_NAME = "OAuth.UserName";
        private const string USER_SIGNED_IN = "AuthenticationManager.SignedIn";
        private const string USER_ROLE = "None";

        //Session setters and getters
        public static string AccessToken
        {
            get { return Read<string>(ACCESS_TOKEN); }
            set { Write(ACCESS_TOKEN, value); }
        }

        public static string DisplayName
        {
            get { return Read<string>(DISPLAY_NAME); }
            set { Write(DISPLAY_NAME, value); }
        }

        public static string RefreshToken
        {
            get { return Read<string>(REFRESH_TOKEN); }
            set { Write(REFRESH_TOKEN, value); }
        }

        public static string SideBarClosed
        {
            get { return Read<string>(SIDEBAR_CLOSED); }
            set { Write(SIDEBAR_CLOSED, value); }
        }

        public static string State
        {
            get { return Read<string>(STATE); }
            set { Write(STATE, value); }
        }
        
        public static string UserName
        {
            get { return Read<string>(USER_NAME); }
            set { Write(USER_NAME, value); }
        }

        public static string UserSignedIn
        {
            get { return Read<string>(USER_SIGNED_IN); }
            set { Write(USER_SIGNED_IN, value); }
        }

        public static string UserRole
        {
            get { return Read<string>(USER_ROLE); }
            set { Write(USER_ROLE, value); }
        }
    }
}