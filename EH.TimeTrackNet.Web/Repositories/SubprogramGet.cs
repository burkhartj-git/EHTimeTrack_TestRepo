using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EH.TimeTrackNet.Web.Models;

namespace EH.TimeTrackNet.Web.Repositories
{
    public class SubprogramGet
    {
        /// <summary>
        /// to get all subprograms by program id
        /// </summary>
        public IEnumerable<SelectListItem> GetSubprograms(int programID)
        {
            using (Entities dbSubprogram = new Entities())
            {
                IEnumerable<REF_SUBPROGRAM_TB> subprogramsOrdered = (IEnumerable<REF_SUBPROGRAM_TB>)dbSubprogram.REF_SUBPROGRAM_TB
                                                                    .ToList();
                IEnumerable<SelectListItem> subprograms = (IEnumerable<SelectListItem>)dbSubprogram.REF_SUBPROGRAM_TB
                            .Where(u => u.N_PROGRAM_SYSID == programID && u.SZ_CODE != "0")
                            .OrderBy(u => u.SZ_DESCRIPTION)
                            .Select(u => new SelectListItem { Value = u.N_SUBPROGRAM_SYSID.ToString(), Text = u.SZ_DESCRIPTION })
                            .ToList();
                return subprograms;
            }
        }

        /// <summary>
        /// lookup - to get subprogram name by subprogram id
        /// </summary>
        public string GetSubprogramNameByID(int subprogramID)
        {
            using (Entities dbSubprogram = new Entities())
            {
                string name = dbSubprogram.REF_SUBPROGRAM_TB
                                .Where(u => u.N_SUBPROGRAM_SYSID == subprogramID)
                                .Select(u => u.SZ_DESCRIPTION)
                                .FirstOrDefault();
                return name;
            }
        }

        /// <summary>
        /// to check if the subprogram code exists
        /// </summary>
        public bool IsSubprogramNumberDuplicate(int SubprogramCode, int ProgramID)
        {
            using (Entities dbSubprogram = new Entities())
            {
                string subprogramCode = SubprogramCode.ToString();
                return dbSubprogram.REF_SUBPROGRAM_TB.Any(u => u.SZ_CODE == subprogramCode && u.N_PROGRAM_SYSID == ProgramID);
            }
        }

        /// <summary>
        /// to check if the subprogram description exists
        /// </summary>
        public bool IsSubprogramDescriptionDuplicate(string SubprogramDescription, int ProgramID)
        {
            using (Entities dbSubprogram = new Entities())
            {
                return dbSubprogram.REF_SUBPROGRAM_TB.Any(u => u.SZ_DESCRIPTION == SubprogramDescription && u.N_PROGRAM_SYSID == ProgramID);
            }
        }

        /// <summary>
        /// to get the next available subprogram code
        /// </summary>
        public int GetNextSubprogramCode()
        {
            using (Entities db = new Entities())
            {
                int number = Convert.ToInt32((from r in db.REF_SUBPROGRAM_TB.ToList()
                                              orderby int.Parse(r.SZ_CODE)
                                              select r.SZ_CODE).LastOrDefault());

                return number + 1;          
            }
        }
    }
}