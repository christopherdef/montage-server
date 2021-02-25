using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using MontageServer.Models;

namespace MontageServer.Data
{
    public class MontageDBContext : DbContext
    {
        public MontageDBContext(DbContextOptions<MontageDBContext> options)
            : base(options)
        {
        }
        public DbSet<MontageServer.Models.ProjectCaching> ProjectCaching { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectCaching>().ToTable("ProjectCache");
        }
    }
}
