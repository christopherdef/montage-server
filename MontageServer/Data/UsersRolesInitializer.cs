using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MontageServer.Data
{
    public class UsersRolesDbInitializer
    {
        internal async static Task Initialize(UsersRolesDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, MontageDbContext lotContext)
        {
            context.Database.Migrate();

            // ROLE SEEDING
            // certain roles are required for the proper functioning of the app
            var reqRoles = new HashSet<IdentityRole>(){
                new IdentityRole()
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "Administrator",
                },
                new IdentityRole()
                {
                    Id = "2",
                    Name = "Chair",
                    NormalizedName = "Department Chair"
                },
                new IdentityRole()
                {
                    Id = "3",
                    Name = "Prof",
                    NormalizedName = "Professor"
                },
            };

            // add all the required roles
            foreach (IdentityRole r in reqRoles)
            {
                if (!context.Roles.Contains(r))
                    roleManager.CreateAsync(r).Wait();
            }
            context.SaveChanges();

            // USER/USER-ROLE SEEDING

            // if no users are in the DB, seed them here
            if (!context.Users.Any())
            {
                var users = new List<IdentityUser>() {
                    new IdentityUser() {
                        Id = "10",
                        NormalizedUserName = "Erin Parker",
                        Email = "admin_erin@cs.utah.edu"
                    },
                    new IdentityUser() {
                        Id = "20",
                        NormalizedUserName = "Ross Whitaker",
                        Email = "chair_whitaker@cs.utah.edu"
                    },
                    new IdentityUser() {
                        Id = "30",
                        NormalizedUserName = "H. James de St. Germain",
                        Email = "professor_jim@cs.utah.edu"
                    },
                    new IdentityUser() {
                        Id = "31",
                        NormalizedUserName = "Mary Hall",
                        Email = "professor_mary@cs.utah.edu"
                    },
                    new IdentityUser() {
                        Id = "32",
                        NormalizedUserName = "Daniel Kopta",
                        Email = "professor_danny@cs.utah.edu"
                    }
                };

                foreach (IdentityUser u in users)
                {
                    // add user
                    u.UserName = u.Email;
                    u.EmailConfirmed = true;
                    u.SecurityStamp = Guid.NewGuid().ToString();
                    await userManager.CreateAsync(u, "123ABC!@#def");

                    // add user to role based on id
                    switch (u.Id)
                    {
                        case string id when id[0].Equals('1'):
                            await userManager.AddToRoleAsync(u, "Admin");
                            break;
                        case string id when id[0].Equals('2'):
                            await userManager.AddToRoleAsync(u, "Chair");
                            break;
                        // TODO: right here, or earlier, get the IdentityUsers marked as Instructor into the Instructors table as well
                        case string id when id[0].Equals('3'):
                            await userManager.AddToRoleAsync(u, "Prof");
                            break;
                    }

                }

                context.SaveChanges();
            }




        }

    }
}
