using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EH.TimeTrackNet.Web.Models;

namespace EH.TimeTrackNet.Web.Repositories
{
    public class OptionGet
    {
        /// <summary>
        /// to get all options
        /// </summary>
        public IEnumerable<SelectListItem> GetOptions()
        {
            //
            using (Entities dbOption = new Entities())
            {
                List<REF_OPTION_TB> optionsOrdered = dbOption.REF_OPTION_TB
                                                            .Distinct()
                                                            .OrderBy(u => u.SZ_DESCRIPTION)
                                                            .ToList();

                List<SelectListItem> options = optionsOrdered
                            .Select(u => new SelectListItem { Value = u.N_OPTION_SYSID.ToString(), Text = u.SZ_DESCRIPTION })
                            .ToList();

                options.Insert(0, (new SelectListItem { Text = "", Value = "0" }));

                return (IEnumerable<SelectListItem>)options;
            }
        }

        /// <summary>
        /// lookup - to get option name by option id
        /// </summary>
        public string GetOptionNameByID(int optionID)
        {
            using (Entities dbOption = new Entities())
            {
                string name = dbOption.REF_OPTION_TB
                                .Where(u => u.N_OPTION_SYSID == optionID)
                                .Select(u => u.SZ_DESCRIPTION)
                                .FirstOrDefault();
                return name;
            }
        }

        /// <summary>
        /// to check if the option code exists
        /// </summary>
        public bool IsOptionNumberDuplicate(int OptionCode)
        {
            using (Entities dbOption = new Entities())
            {
                string optionCode = OptionCode.ToString();
                return dbOption.REF_OPTION_TB.Any(u => u.SZ_CODE == optionCode);
            }
        }

        /// <summary>
        /// to check if the option description exists
        /// </summary>
        public bool IsOptionDescriptionDuplicate(string OptionDescription)
        {
            using (Entities dbOption = new Entities())
            {
                return dbOption.REF_OPTION_TB.Any(u => u.SZ_DESCRIPTION == OptionDescription);
            }
        }

        /// <summary>
        /// to get the next available option code
        /// </summary>
        public int GetNextOptionCode()
        {
            using (Entities db = new Entities())
            {
                int number = Convert.ToInt32((from r in db.REF_OPTION_TB.ToList()
                                              orderby int.Parse(r.SZ_CODE)
                                              select r.SZ_CODE).LastOrDefault());

                return number + 1;
            }
        }
    }
}