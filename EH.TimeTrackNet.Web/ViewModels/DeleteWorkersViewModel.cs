using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EH.TimeTrackNet.Web.ViewModels
{
    public class DeleteWorkersViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string WorkerNumber { get; set; }
        public bool ShowMessage { get; set; }
        public string Message { get; set; }
    }
}