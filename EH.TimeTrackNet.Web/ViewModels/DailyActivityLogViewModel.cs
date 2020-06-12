using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EH.TimeTrackNet.Web.ViewModels
{
    public class DailyActivityLogViewModel
    {
        public string WorkerName { get ;set; }

        public int ServiceLogID { get; set; }

        public string ServiceDate { get; set; }

        public Nullable<decimal> MileageTime { get; set; }

        public Nullable<decimal> TotalActivityTime { get; set; }

        public Nullable<decimal> TotalTime { get; set; }

        public string SelectedActivity { get; set; }

        public string SelectedSample { get; set; }

        public bool IsValid { get; set; }
    }
}