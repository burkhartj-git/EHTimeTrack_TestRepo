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
    public class MaintainActivitiesViewModel
    {
        public bool ShowEditMessage { get; set; }
        public string EditMessage { get; set; }
        public List<MaintainActivitiesEditorViewModel> Activity { get; set; }

        public MaintainActivitiesViewModel()
        {
            this.Activity = new List<MaintainActivitiesEditorViewModel>();
        }  
  
        public IEnumerable<int> getSelectedIds()
        {
            // Return an Enumerable containing the Id's of the selected people:
            return (from p in this.Activity where p.Selected select p.Id).ToList();
        }
    }
}