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
    public class ServiceLogViewModel
    {
        public int ServiceID { get; set; }

        public List<SelectListItem> Workers { get; set; }

        [Required(ErrorMessage = "Please select a worker.")]
        public string SelectedWorker { get; set; }

        [Required(ErrorMessage = "Service Date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Please enter the Service Date in the correct format.")]
        public string ServiceDate { get; set; }

        public string DateValidationMessage { get; set; }
        public bool ShowDateMessage { get; set; }

        public ServiceLogViewModel()
        {
            Workers = new List<SelectListItem>();
        }
    }
}