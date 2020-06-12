using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EH.TimeTrackNet.Web.Models;

namespace EH.TimeTrackNet.Web.ViewModels
{
    public class MaintainWorkersEditorViewModel
    {
        public bool Selected { get; set; }
        public int Id { get; set; }
        public string UserName { get; set; }
        public string WorkerNumber { get; set; }
    }
}