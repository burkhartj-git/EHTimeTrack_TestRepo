using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EH.TimeTrackNet.Web.Models;

namespace EH.TimeTrackNet.Web.Repositories
{
    public class WorkerGet
    {
        /// <summary>
        /// to get the worker number by user name
        /// </summary>
        public Int32 GetWorkerNumber(string UserName)
        {
            using (Entities dbWorker = new Entities())
            {
                Int32 workerNumber = -1;
                workerNumber = Convert.ToInt32(dbWorker.REF_WORKER_TB
                                .Where(h => h.SZ_USER_NAME == UserName)
                                .FirstOrDefault());
                return workerNumber;
            }
        }

        /// <summary>
        /// to get the next available worker number
        /// </summary>
        public int GetNextWorkerNumber()
        {
            using (Entities db = new Entities())
            {
                int workerNumber = Convert.ToInt32((from h in db.REF_WORKER_TB.ToList()
                                                    orderby h.N_WORKER_NUMBER
                                                    select h.N_WORKER_NUMBER).LastOrDefault());

                return workerNumber + 1;
            }
        }

        /// <summary>
        /// to get the next available worker id
        /// </summary>
        public int GetNextWorkerID()
        {
            using (Entities db = new Entities())
            {
                int workerID = Convert.ToInt32((from h in db.REF_WORKER_TB.ToList()
                                                orderby h.N_WORKER_SYSID
                                                select h.N_WORKER_SYSID).LastOrDefault());

                return workerID + 1;
            }
        }

        /// <summary>
        /// to check if the worker number exists
        /// </summary>
        public bool IsWorkerNumberDuplicate(int WorkerNumber)
        {
            using (Entities dbWorker = new Entities())
            {
                return dbWorker.REF_WORKER_TB.Any(h => h.N_WORKER_NUMBER == WorkerNumber);
            }
        }
    }
}