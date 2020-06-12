using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EH.TimeTrackNet.Web.Models;

namespace EH.TimeTrackNet.Web.Repositories
{
    public class CivilDivisionGet
    {
        /// <summary>
        /// to get all civil divisions
        /// </summary>
        public List<SelectListItem> GetCivilDivisions()
        {
            using (ApplicationServicesCoreEntities dbCivilDivision = new ApplicationServicesCoreEntities())
            {
                List<SelectListItem> civilDivisions = (List<SelectListItem>)(from cd in dbCivilDivision.Regions
                                                                                           join r in dbCivilDivision.RegionTypes
                                                                                           on cd.sz_region_type_id
                                                                                           equals r.sz_region_type_id
                                                                                           where r.sz_name == "CVT" 
                                                                                           select new SelectListItem { Value = cd.sz_fips_code.ToString(), Text = cd.sz_description })
                                                                                            .Distinct()
                                                                                            .ToList();

                foreach (var item in civilDivisions)
                {
                    string text;

                    if (item.Text.Substring(0, 11) == "Township of")
                    {
                        text = item.Text.Replace("Township of ", "");
                        item.Text = text + " Township";
                    }

                    text = item.Text.Replace("City of ", "").Replace("Village of ", "").Replace("Villiage of ", "");
                    item.Text = text;
                }

                var sortedCivilDivisions = civilDivisions.OrderBy(u => u.Text).ToList();

                sortedCivilDivisions.Add(new SelectListItem { Value = "", Text = "Out of County" });
                sortedCivilDivisions.Insert(0, new SelectListItem { Value = "US26099", Text = "County Wide" });

                return (List<SelectListItem>)sortedCivilDivisions;
            }
        }

        /// <summary>
        /// lookup - to get civil division name by civil division id
        /// </summary>
        public string GetCivilDivisionNameByID(string civilDivisionFIPS)
        {
            using (ApplicationServicesCoreEntities dbCivilDivision = new ApplicationServicesCoreEntities())
            {
                if (civilDivisionFIPS == "0")
                {
                    return "";
                }
                else if (civilDivisionFIPS == "US26099")
                {
                    return "County Wide";
                }
                else if (civilDivisionFIPS == "")
                {
                    return "Out of County";
                }
                else
                {
                    string name = dbCivilDivision.Regions
                                    .Where(u => u.sz_fips_code == civilDivisionFIPS)
                                    .Select(u => u.sz_name)
                                    .FirstOrDefault();

                    return name;
                }
            }
        }
    }
}