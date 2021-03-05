using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using MontageServer.Models;

namespace MontageServer.Data
{
    public class UsersRolesDbContext : IdentityDbContext
    {
        public UsersRolesDbContext(DbContextOptions<UsersRolesDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
    }
}
