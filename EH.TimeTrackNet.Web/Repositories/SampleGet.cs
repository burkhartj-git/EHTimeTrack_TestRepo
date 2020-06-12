using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EH.TimeTrackNet.Web.Models;

namespace EH.TimeTrackNet.Web.Repositories
{
    public class SampleGet
    {
        /// <summary>
        /// to get samples by activity id
        /// </summary>
        public IEnumerable<TRN_SAMPLE_TB> GetSamplesByActivityID(int activityID)
        {
            using (Entities dbSample = new Entities())
            {
                IEnumerable<TRN_SAMPLE_TB> samples = (IEnumerable<TRN_SAMPLE_TB>)dbSample.TRN_SAMPLE_TB
                            .Where(s => s.N_SERVICE_X_ACTIVITY_TYPE_SYSID == activityID)
                            .Distinct()
                            .OrderBy(u => u.N_SERVICE_X_ACTIVITY_TYPE_SYSID)
                            .ToList();
                return samples;
            }
        }

        /// <summary>
        /// lookup - to get sample name by sample id
        /// </summary>
        public string GetSampleNameByID(int sampleID)
        {
            using (Entities dbSample = new Entities())
            {
                string name = dbSample.REF_SAMPLE_TYPE_TB
                                .Where(u => u.N_SAMPLE_TYPE_SYSID == sampleID)
                                .Select(u => u.SZ_DESCRIPTION)
                                .FirstOrDefault();
                return name;
            }
        }

        /// <summary>
        /// to get all samples
        /// </summary>
        public IEnumerable<SelectListItem> GetSamples(int ActivityLogID)
        {
            using (Entities dbSample = new Entities())
            {
                IEnumerable<TRN_SAMPLE_TB> sampleList = (IEnumerable<TRN_SAMPLE_TB>)dbSample.TRN_SAMPLE_TB
                                                                    .Where(u => u.N_SERVICE_X_ACTIVITY_TYPE_SYSID == ActivityLogID)
                                                                    .ToList();

                IEnumerable<REF_SAMPLE_TYPE_TB> samplesOrdered = (IEnumerable<REF_SAMPLE_TYPE_TB>)dbSample.REF_SAMPLE_TYPE_TB
                                                                    .Distinct()
                                                                    .OrderBy(u => u.SZ_DESCRIPTION)
                                                                    .ToList();

                samplesOrdered = samplesOrdered.Where(u => !sampleList.Any(u2 => u2.N_SAMPLE_TYPE_SYSID == u.N_SAMPLE_TYPE_SYSID));

                IEnumerable<SelectListItem> samples = samplesOrdered
                            .Select(u => new SelectListItem { Value = u.N_SAMPLE_TYPE_SYSID.ToString(), Text = u.SZ_DESCRIPTION })
                            .ToList();

                return samples;
            }
        }

        /// <summary>
        /// to get sample by sample id
        /// </summary>
        public TRN_SAMPLE_TB GetSampleBySampleID(int sampleID)
        {
            using (Entities dbSample = new Entities())
            {
                TRN_SAMPLE_TB sample = dbSample.TRN_SAMPLE_TB
                                        .Where(u => u.N_SAMPLE_SYSID == sampleID)
                                        .FirstOrDefault();
                return sample;
            }
        }

        /// <summary>
        /// to check if the sample code exists
        /// </summary>
        public bool IsSampleNumberDuplicate(int SampleCode)
        {
            using (Entities dbSample = new Entities())
            {
                string sampleCode = SampleCode.ToString();
                return dbSample.REF_SAMPLE_TYPE_TB.Any(u => u.SZ_CODE == sampleCode);
            }
        }

        /// <summary>
        /// to check if the sample description exists
        /// </summary>
        public bool IsSampleDescriptionDuplicate(string SampleDescription)
        {
            using (Entities dbSample = new Entities())
            {
                return dbSample.REF_SAMPLE_TYPE_TB.Any(u => u.SZ_DESCRIPTION == SampleDescription);
            }
        }

        /// <summary> 
        /// to get the next available sample code
        /// </summary>
        public int GetNextSampleCode()
        {
            using (Entities db = new Entities())
            {
                int number = Convert.ToInt32((from r in db.REF_SAMPLE_TYPE_TB.ToList()
                                              orderby int.Parse(r.SZ_CODE)
                                              select r.SZ_CODE).LastOrDefault());

                return number + 1;
            }
        }
    }
}