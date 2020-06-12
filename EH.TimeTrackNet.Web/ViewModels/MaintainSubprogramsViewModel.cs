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
    public class MaintainSubprogramsViewModel
    {
        public bool ShowEditMessage { get; set; }
        public string EditMessage { get; set; }
        public int SelectedProgram { get; set; }

        public string ProgramDescription { get; set; }
        public string ProgramCode { get; set; }

        public List<MaintainSubprogramsEditorViewModel> Subprogram { get; set; }

        public MaintainSubprogramsViewModel()
        {
            this.Subprogram = new List<MaintainSubprogramsEditorViewModel>();
        }

        public IEnumerable<int> getSelectedIds()
        {
            // Return an Enumerable containing the Id's of the selected subprograms:
            return (from p in this.Subprogram where p.Selected select p.Id).ToList();
        }
    }
}