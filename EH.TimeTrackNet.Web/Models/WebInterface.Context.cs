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
    
    public partial class RoleProviderSqlEntities : DbContext
    {
        public RoleProviderSqlEntities()
            : base("name=RoleProviderSqlEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AspNetApplicationRolePermission> AspNetApplicationRolePermissions { get; set; }
        public virtual DbSet<AspNetApplication> AspNetApplications { get; set; }
        public virtual DbSet<AspNetApplicationUserRole> AspNetApplicationUserRoles { get; set; }
        public virtual DbSet<AspNetPermission> AspNetPermissions { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetSitemapNode> AspNetSitemapNodes { get; set; }
        public virtual DbSet<AspNetSitemap> AspNetSitemaps { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
    }
}