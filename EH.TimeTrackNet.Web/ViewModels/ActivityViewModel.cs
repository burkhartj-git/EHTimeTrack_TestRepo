using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAnnotationsExtensions;

namespace EH.TimeTrackNet.Web.ViewModels
{
    public class ActivityViewModel
    {
        public string WorkerName { get ;set; }

        public int ServiceLogID { get; set; }

        public int ActivityLogID { get; set; }

        public string ServiceDate { get; set; }

        public Nullable<decimal> MileageTime { get; set; }

        public Nullable<decimal> TotalTime { get; set; }

        [DataType(DataType.Text)]
        [StringLength(1000, ErrorMessage = "Address cannot exceed 1000 characters in length")]
        public string Address { get; set; }

        public List<SelectListItem> CivilDivisions { get; set; }
        public string SelectedCivilDivision { get; set; }

        public List<SelectListItem> Activities { get; set; }
        public int SelectedActivity { get; set; }

        public List<SelectListItem> Programs { get; set; }
        public int SelectedProgram { get; set; }

        public List<SelectListItem> Subprograms { get; set; }
        public int SelectedSubprogram { get; set; }

        [Required(ErrorMessage = "Activity Time is required")]
        public decimal ActivityTime { get; set; }
        public string ActivityTimeValidationMessage { get; set; }
        public bool ShowActivityTimeMessage { get; set; }

        public string Identical { get; set; }
        public string IdenticalValidationMessage { get; set; }
        public bool ShowIdenticalMessage { get; set; }

        public List<SelectListItem> Options { get; set; }
        public int SelectedOption { get; set; }

        public ActivityViewModel()
        {
            CivilDivisions = new List<SelectListItem>();
            Activities = new List<SelectListItem>();
            Programs = new List<SelectListItem>();
            Subprograms = new List<SelectListItem>();
            Options = new List<SelectListItem>();
        }
    }
}