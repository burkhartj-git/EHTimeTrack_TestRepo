using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EH.TimeTrackNet.Web.Models;

namespace EH.TimeTrackNet.Web.Repositories
{
    public class PurgeActivityDataGet
    {
        /// <summary>
        /// to get all service logs prior to purge date
        /// </summary>
        public IEnumerable<Int32> GetServiceLogsForPurge(DateTime PurgeDate)
        {
            using (Entities dbService = new Entities())
            {
                IEnumerable<Int32> serviceLogIds = (IEnumerable<Int32>)dbService.TRN_SERVICE_TB
                                                            .Where(u => u.DT_SERVICE < PurgeDate)
                                                            .Select(u => u.N_SERVICE_SYSID)
                                                            .ToList();
                return serviceLogIds;
            }
        }

        /// <summary>
        /// to get all activity logs for purge by service log id
        /// </summary>
        public IEnumerable<Int32> GetActivityLogsByServiceLogIDForPurge(int ServiceLogID)
        {
            using (Entities dbActivity = new Entities())
            {
                IEnumerable<Int32> activityLogIds = (IEnumerable<Int32>)dbActivity.TRN_SERVICE_X_ACTIVITY_TYPE_TB
                                                        .Where(u => u.N_SERVICE_SYSID == ServiceLogID)
                                                        .Select(u => u.N_SERVICE_X_ACTIVITY_TYPE_SYSID)
                                                        .ToList();
                return activityLogIds;
            }
        }

        /// <summary>
        /// to get all sample logs for purge by activity log id
        /// </summary>
        public IEnumerable<Int32> GetSampleLogsByActivityLogIDForPurge(int ActivityLogID)
        {
            using (Entities dbSample = new Entities())
            {
                IEnumerable<Int32> sampleLogIds = (IEnumerable<Int32>)dbSample.TRN_SAMPLE_TB
                                                    .Where(u => u.N_SERVICE_X_ACTIVITY_TYPE_SYSID == ActivityLogID)
                                                    .Select(u => u.N_SAMPLE_SYSID)
                                                    .ToList();
                return sampleLogIds;
            }
        }

        /// <summary>
        /// to call stored proc to delete all service log activity prior to purge date
        /// </summary>
        public Boolean IsPurged(DateTime purgeDate)
        {
            Boolean isPurged = false;
            using (Entities entities = new Entities())
            {
                entities.usp_PurgeActivityDataCas(purgeDate);
                isPurged = true;
            }
            return isPurged;
        }
    }
}