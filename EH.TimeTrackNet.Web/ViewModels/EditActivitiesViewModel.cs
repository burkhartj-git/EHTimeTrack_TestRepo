using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EH.TimeTrackNet.Web.ViewModels
{
    public class EditActivitiesViewModel
    {
        public int Id { get; set; }

        [Range(0, Int32.MaxValue, ErrorMessage = "Activity Number must be an integer")]
        [Required(ErrorMessage = "Activity Number is required")]
        public int Code { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Activity Description is required")]
        public string Description { get; set; }

        public bool IsNew { get; set; }
        public bool ShowMessageCode { get; set; }
        public bool ShowMessageDescription { get; set; }
        public string ValidationMessage { get; set; }
    }
}