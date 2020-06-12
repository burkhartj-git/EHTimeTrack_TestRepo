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
    public class MaintainOptionsViewModel
    {
        public bool ShowEditMessage { get; set; }
        public string EditMessage { get; set; }
        public List<MaintainOptionsEditorViewModel> Option { get; set; }

        public MaintainOptionsViewModel()
        {
            this.Option = new List<MaintainOptionsEditorViewModel>();
        }

        public IEnumerable<int> getSelectedIds()
        {
            // Return an Enumerable containing the Id's of the selected options:
            return (from p in this.Option where p.Selected select p.Id).ToList();
        }
    }
}