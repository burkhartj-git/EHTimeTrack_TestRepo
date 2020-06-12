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
    public class MaintainProgramsViewModel
    {
        public bool ShowEditMessage { get; set; }
        public string EditMessage { get; set; }
        public List<MaintainProgramsEditorViewModel> Program { get; set; }

        public MaintainProgramsViewModel()
        {
            this.Program = new List<MaintainProgramsEditorViewModel>();
        }

        public IEnumerable<int> getSelectedIds()
        {
            // Return an Enumerable containing the Id's of the selected programs:
            return (from p in this.Program where p.Selected select p.Id).ToList();
        }
    }
}