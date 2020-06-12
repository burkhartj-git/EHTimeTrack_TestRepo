using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EH.TimeTrackNet.Web.ViewModels
{
    public class EditWorkersViewModel
    {
        public int Id { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Worker User Name is required")]
        public string UserName { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Worker Number is required")]
        public string WorkerNumber { get; set; }

        public bool IsNew { get; set; }
        public bool ShowMessageCode { get; set; }
        public bool ShowMessageDescription { get; set; }
        public string ValidationMessage { get; set; }
    }
}