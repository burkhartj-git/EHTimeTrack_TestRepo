using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EH.TimeTrackNet.Web.Models;

namespace EH.TimeTrackNet.Web.Repositories
{
    public class DailyActivityGet
    {
        /// <summary>
        /// to get daily activities by service log id
        /// </summary>
        public IEnumerable<TRN_SERVICE_X_ACTIVITY_TYPE_TB> GetDailyActivityByServiceLogID(int serviceLogID)
        {
            using (Entities entities = new Entities())
            {
                IEnumerable<TRN_SERVICE_X_ACTIVITY_TYPE_TB> dailyActivities = (IEnumerable<TRN_SERVICE_X_ACTIVITY_TYPE_TB>)entities.TRN_SERVICE_X_ACTIVITY_TYPE_TB
                            .Where(u => u.N_SERVICE_SYSID == serviceLogID)
                            .Distinct()
                            .OrderBy(u => u.N_SERVICE_X_ACTIVITY_TYPE_SYSID)
                            .ToList();
                return dailyActivities;
            }
        }
    }
}