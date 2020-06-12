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
    public class MaintainWorkersViewModel
    {
        public bool ShowEditMessage { get; set; }
        public string EditMessage { get; set; }
        public List<MaintainWorkersEditorViewModel> Worker { get; set; }

        public MaintainWorkersViewModel()
        {
            this.Worker = new List<MaintainWorkersEditorViewModel>();
        }  
  
        public IEnumerable<int> getSelectedIds()
        {
            // Return an Enumerable containing the Id's of the selected workers:
            return (from p in this.Worker where p.Selected select p.Id).ToList();
        }
    }
}