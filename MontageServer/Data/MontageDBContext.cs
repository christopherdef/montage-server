using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using MontageServer.Models;

namespace MontageServer.Data
{
    public class MontageDbContext : DbContext
    {
        public MontageDbContext(DbContextOptions<MontageDbContext> options)
            : base(options)
        {
        }
        public DbSet<AdobeClip> AdobeClips { get; set; }
        public DbSet<AdobeProject> AdobeProjects { get; set; }
        public DbSet<ClipAssignment> ClipAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AdobeClip>().ToTable("AdobeClip");
            modelBuilder.Entity<AdobeProject>().ToTable("AdobeProject");
            modelBuilder.Entity<ClipAssignment>().ToTable("ClipAssignment");
            modelBuilder.Entity<AnalysisResult>().ToTable("AnalysisResult");


            modelBuilder.Entity<AdobeProject>()
                .HasKey(p => new { p.ProjectId, p.UserId });

            modelBuilder.Entity<ClipAssignment>()
                .HasKey(a => new { a.ProjectId, a.ClipId });
        }
    }
}
