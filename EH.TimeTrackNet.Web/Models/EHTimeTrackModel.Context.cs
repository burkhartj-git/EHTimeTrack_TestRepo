﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class Entities : DbContext
    {
        public Entities()
            : base("name=Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<REF_ACTIVITY_TYPE_TB> REF_ACTIVITY_TYPE_TB { get; set; }
        public virtual DbSet<REF_OPTION_TB> REF_OPTION_TB { get; set; }
        public virtual DbSet<REF_PROGRAM_TB> REF_PROGRAM_TB { get; set; }
        public virtual DbSet<REF_SAMPLE_TYPE_TB> REF_SAMPLE_TYPE_TB { get; set; }
        public virtual DbSet<REF_SECTION_TB> REF_SECTION_TB { get; set; }
        public virtual DbSet<REF_SUBPROGRAM_TB> REF_SUBPROGRAM_TB { get; set; }
        public virtual DbSet<TRN_SAMPLE_TB> TRN_SAMPLE_TB { get; set; }
        public virtual DbSet<TRN_SERVICE_TB> TRN_SERVICE_TB { get; set; }
        public virtual DbSet<TRN_SERVICE_X_ACTIVITY_TYPE_TB> TRN_SERVICE_X_ACTIVITY_TYPE_TB { get; set; }
        public virtual DbSet<REF_WORKER_TB> REF_WORKER_TB { get; set; }
    
        public virtual int usp_PurgeActivityData(Nullable<System.DateTime> purgeDate)
        {
            var purgeDateParameter = purgeDate.HasValue ?
                new ObjectParameter("purgeDate", purgeDate) :
                new ObjectParameter("purgeDate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("usp_PurgeActivityData", purgeDateParameter);
        }
    
        public virtual int usp_PurgeActivityDataCas(Nullable<System.DateTime> purgeDate)
        {
            var purgeDateParameter = purgeDate.HasValue ?
                new ObjectParameter("purgeDate", purgeDate) :
                new ObjectParameter("purgeDate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("usp_PurgeActivityDataCas", purgeDateParameter);
        }
    }
}
