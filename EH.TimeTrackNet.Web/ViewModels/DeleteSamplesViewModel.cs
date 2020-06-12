using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EH.TimeTrackNet.Web.ViewModels
{
    public class DeleteSamplesViewModel
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string Description { get; set; }
        public bool ShowMessage { get; set; }
        public string Message { get; set; }
    }
}