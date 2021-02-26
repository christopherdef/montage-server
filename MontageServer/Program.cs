using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MontageServer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MontageServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            CreateDbIfNotExists(host);

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void CreateDbIfNotExists(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                //try
                //{
                //    var userRolesContext = services.GetRequiredService<UsersRolesDbContext>();
                //    var manager = services.GetRequiredService<UserManager<IdentityUser>>();
                //    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                //    var montageContext = services.GetRequiredService<MontageDbContext>();

                //    UsersRolesDbInitializer.Initialize(userRolesContext, manager, roleManager, montageContext).Wait();
                //}
                //catch (Exception ex)
                //{
                //    var logger = services.GetRequiredService<ILogger<Program>>();
                //    logger.LogError(ex, "An error occurred while seeding the Users portion of the database.");
                //}

                try
                {
                    var montageContext = services.GetRequiredService<MontageDbContext>();
                    MontageDbInitializer.Initialize(montageContext);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred creating the Montage DB.");
                }
            }
        }
    }
}
