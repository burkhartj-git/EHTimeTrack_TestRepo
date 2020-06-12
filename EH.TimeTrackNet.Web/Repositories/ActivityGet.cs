using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EH.TimeTrackNet.Web.Models;

namespace EH.TimeTrackNet.Web.Repositories
{
    public class ActivityGet
    {
        /// <summary>
        /// to get all activities
        /// </summary>
        public IEnumerable<SelectListItem> GetActivities()
        {
            //
            using (Entities dbActivity = new Entities())
            {
                IEnumerable<REF_ACTIVITY_TYPE_TB> activitiesOrdered = (IEnumerable<REF_ACTIVITY_TYPE_TB>)dbActivity.REF_ACTIVITY_TYPE_TB
                            .OrderBy(u => u.SZ_DESCRIPTION)
                            .ToList();

                IEnumerable<SelectListItem> activities = activitiesOrdered
                            .Select(u => new SelectListItem { Value = u.N_ACTIVITY_TYPE_SYSID.ToString(), Text = u.SZ_DESCRIPTION })
                            .Distinct()
                            .ToList();
                return activities;
            }
        }

        /// <summary>
        /// to get all activities by service log id
        /// </summary>
        public IEnumerable<TRN_SERVICE_X_ACTIVITY_TYPE_TB> GetActivitiesByServiceLogID(int serviceLogID)
        {
            //
            using (Entities dbActivity = new Entities())
            {
                IEnumerable<TRN_SERVICE_X_ACTIVITY_TYPE_TB> activities = (IEnumerable<TRN_SERVICE_X_ACTIVITY_TYPE_TB>)dbActivity.TRN_SERVICE_X_ACTIVITY_TYPE_TB
                            .Where(u => u.N_SERVICE_SYSID == serviceLogID)
                            .Distinct()
                            .OrderBy(u => u.N_ACTIVITY_TYPE_SYSID)
                            .ToList();
                return activities;
            }
        }

        /// <summary>
        /// to get all activities by program id
        /// </summary>
        public IEnumerable<TRN_SERVICE_X_ACTIVITY_TYPE_TB> GetActivitiesByProgramID(int programID)
        {
            using (Entities dbActivity = new Entities())
            {
                IEnumerable<TRN_SERVICE_X_ACTIVITY_TYPE_TB> activities = (IEnumerable<TRN_SERVICE_X_ACTIVITY_TYPE_TB>)dbActivity.TRN_SERVICE_X_ACTIVITY_TYPE_TB
                            .Where(u => u.N_PROGRAM_SYSID == programID)
                            .Distinct()
                            .OrderBy(u => u.N_SERVICE_X_ACTIVITY_TYPE_SYSID)
                            .ToList();
                return activities;
            }
        }

        /// <summary>
        /// to get all activities by subprogram id
        /// </summary>
        public IEnumerable<TRN_SERVICE_X_ACTIVITY_TYPE_TB> GetActivitiesBySubprogramID(int subprogramID)
        {
            using (Entities dbActivity = new Entities())
            {
                IEnumerable<TRN_SERVICE_X_ACTIVITY_TYPE_TB> activities = (IEnumerable<TRN_SERVICE_X_ACTIVITY_TYPE_TB>)dbActivity.TRN_SERVICE_X_ACTIVITY_TYPE_TB
                            .Where(u => u.N_SUBPROGRAM_SYSID == subprogramID)
                            .Distinct()
                            .OrderBy(u => u.N_SERVICE_X_ACTIVITY_TYPE_SYSID)
                            .ToList();
                return activities;
            }
        }

        /// <summary>
        /// lookup - to get activity name by activity id
        /// </summary>
        public string GetActivityNameByID(int activityID)
        {
            using (Entities dbActivity = new Entities())
            {
                string name = dbActivity.REF_ACTIVITY_TYPE_TB
                                .Where(u => u.N_ACTIVITY_TYPE_SYSID == activityID)
                                .Select(u => u.SZ_DESCRIPTION)
                                .FirstOrDefault();
                return name;
            }
        }

        /// <summary>
        /// to get activity by activity id
        /// </summary>
        public TRN_SERVICE_X_ACTIVITY_TYPE_TB GetActivityByID(int activityID)
        {
            using (Entities dbActivity = new Entities())
            {
                TRN_SERVICE_X_ACTIVITY_TYPE_TB activity = dbActivity.TRN_SERVICE_X_ACTIVITY_TYPE_TB
                                                            .Where(u => u.N_SERVICE_X_ACTIVITY_TYPE_SYSID == activityID)
                                                            .FirstOrDefault();

                return activity;
            }
        }

        /// <summary>
        /// to get sum of activty time for a daily service log
        /// </summary>
        public decimal GetTotalActivityTime(int serviceLogID)
        {
            using (Entities dbActivity = new Entities())
            {
                var query = (from u in dbActivity.TRN_SERVICE_X_ACTIVITY_TYPE_TB
                             where u.N_SERVICE_SYSID == serviceLogID
                             select new
                             {
                                 Number = u.N_TIME
                             })
                            .Sum(s => (decimal?)s.Number) ?? 0;
                return Convert.ToDecimal(query);
            }
        }

        /// <summary>
        /// to check if the activity code exists
        /// </summary>
        public bool IsActivityNumberDuplicate(int ActivityCode)
        {
            using (Entities dbActivity = new Entities())
            {
                string activityCode = ActivityCode.ToString();
                return dbActivity.REF_ACTIVITY_TYPE_TB.Any(u => u.SZ_CODE == activityCode);
            }
        }

        /// <summary>
        /// to check if the activity description exists
        /// </summary>
        public bool IsActivityDescriptionDuplicate(string ActivityDescription)
        {
            using (Entities dbActivity = new Entities())
            {
                return dbActivity.REF_ACTIVITY_TYPE_TB.Any(u => u.SZ_DESCRIPTION == ActivityDescription);
            }
        }

        /// <summary>
        /// to get the next available activity code
        /// </summary>
        public int GetNextActivityCode()
        {
            using (Entities db = new Entities())
            {
                int number = Convert.ToInt32((from r in db.REF_ACTIVITY_TYPE_TB.ToList()
                                              orderby int.Parse(r.SZ_CODE)
                                              select r.SZ_CODE).LastOrDefault());

                return number + 1;
            }
        }
    }
}