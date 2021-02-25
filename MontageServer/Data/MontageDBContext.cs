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
        public DbSet<AdobeProject> AdobeProject { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdobeProject>().ToTable("AdobeProject");
        }
    }
}
