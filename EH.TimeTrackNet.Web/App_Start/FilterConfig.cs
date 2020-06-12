using System.Web;
using System.Web.Mvc;

using EH.TimeTrackNet.Web.Filters;

namespace EH.TimeTrackNet.Web.App_Start
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new Log4NetException());
            filters.Add(new HandleErrorAttribute());
        }
    }
}
