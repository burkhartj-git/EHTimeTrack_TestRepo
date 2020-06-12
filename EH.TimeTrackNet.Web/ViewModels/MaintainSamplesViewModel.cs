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
    public class MaintainSamplesViewModel
    {
        public bool ShowEditMessage { get; set; }
        public string EditMessage { get; set; }
        public List<MaintainSamplesEditorViewModel> Sample { get; set; }

        public MaintainSamplesViewModel()
        {
            this.Sample = new List<MaintainSamplesEditorViewModel>();
        }

        public IEnumerable<int> getSelectedIds()
        {
            // Return an Enumerable containing the Id's of the selected samples:
            return (from p in this.Sample where p.Selected select p.Id).ToList();
        }
    }
}