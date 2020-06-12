using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EH.TimeTrackNet.Web.ViewModels
{
    public class EditProgramsViewModel
    {
        public int Id { get; set; }

        [Range(0, Int32.MaxValue, ErrorMessage = "Program Number must be an integer")]
        [Required(ErrorMessage = "Program Number is required")]
        public int Code { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Program Description is required")]
        public string Description { get; set; }

        public List<SelectListItem> Sections { get; set; }
        public int SelectedSection { get; set; }

        public EditProgramsViewModel()
        {
            Sections = new List<SelectListItem>();
        }

        public bool IsNew { get; set; }
        public bool ShowMessageCode { get; set; }
        public bool ShowMessageDescription { get; set; }
        public string ValidationMessage { get; set; }
    }
}