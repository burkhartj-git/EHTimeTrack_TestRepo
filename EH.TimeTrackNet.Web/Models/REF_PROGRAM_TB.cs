//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EH.TimeTrackNet.Web.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class REF_PROGRAM_TB
    {
        public REF_PROGRAM_TB()
        {
            this.REF_SUBPROGRAM_TB = new HashSet<REF_SUBPROGRAM_TB>();
            this.TRN_SERVICE_X_ACTIVITY_TYPE_TB = new HashSet<TRN_SERVICE_X_ACTIVITY_TYPE_TB>();
        }
    
        public int N_PROGRAM_SYSID { get; set; }
        public int N_SECTION_SYSID { get; set; }
        public string SZ_CODE { get; set; }
        public string SZ_DESCRIPTION { get; set; }
        public string SZ_MODIFIED_BY { get; set; }
        public System.DateTime DT_MODIFIED { get; set; }
        public bool B_INACTIVE { get; set; }
        public byte[] SZ_TIMESTAMP { get; set; }
    
        public virtual REF_SECTION_TB REF_SECTION_TB { get; set; }
        public virtual ICollection<REF_SUBPROGRAM_TB> REF_SUBPROGRAM_TB { get; set; }
        public virtual ICollection<TRN_SERVICE_X_ACTIVITY_TYPE_TB> TRN_SERVICE_X_ACTIVITY_TYPE_TB { get; set; }
    }
}
