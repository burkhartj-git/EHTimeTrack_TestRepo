using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EH.TimeTrackNet.Web.Models;

namespace EH.TimeTrackNet.Web.Repositories
{
    public class ProgramGet
    {
        /// <summary>
        /// to get all programs
        /// </summary>
        public IEnumerable<SelectListItem> GetPrograms()
        {
            using (Entities dbProgram = new Entities())
            {
                IEnumerable<REF_PROGRAM_TB> programsOrdered = (IEnumerable<REF_PROGRAM_TB>)dbProgram.REF_PROGRAM_TB
                                                                .Distinct()
                                                                .OrderBy(u => u.SZ_DESCRIPTION)
                                                                .ToList();
                IEnumerable<SelectListItem> programs = programsOrdered
                            .Select(u => new SelectListItem { Value = u.N_PROGRAM_SYSID.ToString(), Text = u.SZ_DESCRIPTION })
                            .ToList();
                return programs;
            }                   
        }

        /// <summary>
        /// to get all sections
        /// </summary>
        public IEnumerable<SelectListItem> GetSections()
        {
            using (Entities dbSection = new Entities())
            {
                IEnumerable<REF_SECTION_TB> sectionsOrdered = (IEnumerable<REF_SECTION_TB>)dbSection.REF_SECTION_TB
                                                                .Distinct()
                                                                .OrderBy(u => u.SZ_DESCRIPTION)
                                                                .ToList();
                IEnumerable<SelectListItem> sections = sectionsOrdered
                            .Select(u => new SelectListItem { Value = u.N_SECTION_SYSID.ToString(), Text = u.SZ_DESCRIPTION })
                            .ToList();
                return sections;
            }
        }
        

        /// <summary>
        /// to get first programID
        /// </summary>
        public int GetProgramID()
        {
            using (Entities dbProgram = new Entities())
            {
                int programID = Convert.ToInt32(dbProgram.REF_PROGRAM_TB
                            .OrderBy(u => u.N_PROGRAM_SYSID)
                            .Select(u => u.N_PROGRAM_SYSID)
                            .FirstOrDefault());
                return programID;
            }
        }

        /// <summary>
        /// lookup - to get program name by program id
        /// </summary>
        public string GetProgramNameByID(int programID)
        {
            using (Entities dbProgram = new Entities())
            {
                string name = dbProgram.REF_PROGRAM_TB
                                .Where(u => u.N_PROGRAM_SYSID == programID)
                                .Select(u => u.SZ_DESCRIPTION)
                                .FirstOrDefault();
                return name;
            }
        }

        /// <summary>
        /// to check if the program code exists
        /// </summary>
        public bool IsProgramNumberDuplicate(int ProgramCode)
        {
            using (Entities dbProgram = new Entities())
            {
                string programCode = ProgramCode.ToString();
                return dbProgram.REF_PROGRAM_TB.Any(u => u.SZ_CODE == programCode);
            }
        }

        /// <summary>
        /// to check if the program description exists
        /// </summary>
        public bool IsProgramDescriptionDuplicate(string ProgramDescription)
        {
            using (Entities dbProgram = new Entities())
            {
                return dbProgram.REF_PROGRAM_TB.Any(u => u.SZ_DESCRIPTION == ProgramDescription);
            }
        }

        /// <summary>
        /// to get the next available program code
        /// </summary>
        public int GetNextProgramCode()
        {
            using (Entities db = new Entities())
            {
                int number = Convert.ToInt32((from r in db.REF_PROGRAM_TB.ToList()
                                              orderby int.Parse(r.SZ_CODE)
                                              select r.SZ_CODE).LastOrDefault());

                return number + 1;
            }
        }
    }
}