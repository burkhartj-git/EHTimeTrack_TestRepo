using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EH.TimeTrackNet.Web.Utilities;

namespace EH.TimeTrackNet.Web.ViewModels
{
    public class PurgeActivityDataViewModel
    {
        [Required(ErrorMessage = "Purge Date is required")]
        [DataType(DataType.Date, ErrorMessage = "Please enter the Purge Date in the valid format")]
        public DateTime PurgeDate { get; set; }
    }
}