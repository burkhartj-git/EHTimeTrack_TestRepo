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
    public class MaintainCivilDivisionsViewModel
    {
        public List<MaintainCivilDivisionsEditorViewModel> CivilDivision { get; set; }

        public MaintainCivilDivisionsViewModel()
        {
            this.CivilDivision = new List<MaintainCivilDivisionsEditorViewModel>();
        }
  
  
        public IEnumerable<int> getSelectedIds()
        {
            // Return an Enumerable containing the Id's of the selected civil division:
            return (from p in this.CivilDivision where p.Selected select p.Id).ToList();
        }
    }
}