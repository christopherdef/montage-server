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
        public DbSet<MontageServer.Models.AudioResponse> AudioResponse { get; set; }
    }
}
