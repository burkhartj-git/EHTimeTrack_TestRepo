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
    public class ActivitySamplesViewModel
    {
        public string WorkerName { get ;set; }

        public int ServiceLogID { get; set; }

        public int ActivityLogID { get; set; }

        public int SampleLogID { get; set; }

        public string ServiceDate { get; set; }

        public Nullable<decimal> MileageTime { get; set; }

        public string Address { get; set; }

        public string CivilDivision { get; set; }

        public string Activity { get; set; }

        public string Program { get; set; }

        public string Subprogram { get; set; }

        public Nullable<decimal> ActivityTime { get; set; }

        public string Identical { get; set; }

        public string Option { get; set; }

        public List<SelectListItem> SampleTypes { get; set; }
        public int SelectedSampleType { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a numeric value greater than zero.")]
        public int Quantity { get; set; }

        public ActivitySamplesViewModel()
        {
            SampleTypes = new List<SelectListItem>();
        }
    }
}