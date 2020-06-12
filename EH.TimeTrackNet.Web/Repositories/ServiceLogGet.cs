using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EH.TimeTrackNet.Web.Models;

namespace EH.TimeTrackNet.Web.Repositories
{
    public class ServiceLogGet
    {
        /// <summary>
        /// to get service log by id
        /// </summary>
        public TRN_SERVICE_TB GetServiceLog(int serviceLogID)
        {
            using (Entities dbServiceLog = new Entities())
            {
                TRN_SERVICE_TB log = dbServiceLog.TRN_SERVICE_TB
                                .Where(u => u.N_SERVICE_SYSID == serviceLogID)
                                .Distinct()
                                .FirstOrDefault();
                return log;
            }
        }

        /// <summary>
        /// to get service log by service date and worker principal name
        /// </summary>
        public TRN_SERVICE_TB GetServiceLog(DateTime serviceDate, string workerName)
        {
            using (Entities dbServiceLog = new Entities())
            {
                string test = dbServiceLog.Database.Connection.ConnectionString;

                TRN_SERVICE_TB log = dbServiceLog.TRN_SERVICE_TB
                                .Where(u => u.DT_SERVICE.Year == serviceDate.Year &&
                                    u.DT_SERVICE.Month == serviceDate.Month &&
                                    u.DT_SERVICE.Day == serviceDate.Day &&
                                    u.SZ_USER_NAME == workerName)
                                .Distinct()
                                .FirstOrDefault();

                return log;
            }
        }

        /// <summary>
        /// to get date range of service logs by worker principal name
        /// </summary>
        public IEnumerable<TRN_SERVICE_TB> GetServiceLogsDateRangeByWorkerID(DateTime startDate, DateTime endDate, string workerName)
        {
            using (Entities entities = new Entities())
            {
                IEnumerable<TRN_SERVICE_TB> serviceLogs = (IEnumerable<TRN_SERVICE_TB>)entities.TRN_SERVICE_TB
                                            .Where(log => log.SZ_USER_NAME == workerName && log.DT_SERVICE >= startDate && log.DT_SERVICE <= endDate)
                                            .OrderBy(log => log.DT_SERVICE)
                                            .ToList();
                return serviceLogs;
            }
        }
    }
}