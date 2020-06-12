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
    public class DailySummaryLogViewModel
    {
        public List<SelectListItem> Workers { get; set; }
        public int SelectedWorker { get; set; }

        [Required(ErrorMessage = "Service Start Date is required")]
        [DataType(DataType.Date, ErrorMessage = "Please enter the Service Start Date in the valid format")]
        public DateTime ServiceStartDate { get; set; }

        [Required(ErrorMessage = "Service End Date is required")]
        [DataType(DataType.Date, ErrorMessage = "Please enter the Service Start Date in the valid format")]
        public DateTime ServiceEndDate { get; set; }

        public DailySummaryLogViewModel()
        {
            Workers = new List<SelectListItem>();
        }
    }
}