using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EH.TimeTrackNet.Web.ViewModels
{
    public class EditCivilDivisionsViewModel
    {
        public int Id { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Civil Division Number is required")]
        public string Code { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Civil Division Description is required")]
        public string Description { get; set; }
    }
}