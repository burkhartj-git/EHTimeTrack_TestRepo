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
    public class MaintainSamplesEditorViewModel
    {
        public bool Selected { get; set; }
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }
}