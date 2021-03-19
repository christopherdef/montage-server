using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MontageServer.Data
{
    public class UsersRolesDbInitializer
    {
        internal async static Task Initialize(UsersRolesDbContext context, 
                                              UserManager<IdentityUser> userManager, 
                                              RoleManager<IdentityRole> roleManager,
                                              ILogger<UsersRolesDbInitializer> logger)
        {
            context.Database.Migrate();

            context.SaveChanges();

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
                    Name = "Client",
                    NormalizedName = "Client"
                }
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
                        NormalizedUserName = "Bobbert Alice",
                        Email = "balice@gmail.com"
                    },
                    new IdentityUser() {
                        Id = "20",
                        NormalizedUserName = "Steve Stevenson",
                        Email = "sstevenson@yahoo.com"
                    },
                    new IdentityUser() {
                        Id = "21",
                        NormalizedUserName = "Peter Peterson",
                        Email = "ppeterson@hotmail.com"
                    }
                };

                foreach (IdentityUser u in users)
                {
                    // add user
                    u.UserName = u.Email;
                    u.EmailConfirmed = true;
                    u.SecurityStamp = Guid.NewGuid().ToString();
                    await userManager.CreateAsync(u, "abcd3");

                    // add user to role based on id
                    switch (u.Id)
                    {
                        case string id when id[0].Equals('1'):
                            await userManager.AddToRoleAsync(u, "Admin");
                            break;
                        case string id when id[0].Equals('2'):
                            await userManager.AddToRoleAsync(u, "Client");
                            break;
                    }

                }

            }
            context.SaveChanges();




        }

    }
}
