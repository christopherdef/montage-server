using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MontageServer.Data;
using MontageServer.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MontageServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            CreateDbIfNotExists(host);

            // TODO: resources points to wwwroot/py now, is this vv necessary?
            //WriteResourcesToDisk();

            host.Run();

        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        ///// <summary>
        ///// Delete written scripts to prevent versioning issues
        ///// </summary>
        //private static void DeleteResourcesFromDisk()
        //{
        //    File.Delete(Resources.analyze_transcript_pt);
        //    File.Delete(Resources.transcribe_audio_pt);

        //    Directory.Delete(Resources.script_dir);
        //}

        ///// <summary>
        ///// Write scripts to disk at predetermined locations so they can be run later
        ///// </summary>
        //private static void WriteResourcesToDisk()
        //{
        //    Directory.CreateDirectory(Resources.script_dir);
        //    using (var sw = File.CreateText(Path.Combine(Resources.script_dir, Resources.analyze_transcript_pt)))
        //        sw.Write(Encoding.UTF8.GetString(Resources.analyze_transcript));

        //    using (var sw = File.CreateText(Path.Combine(Resources.script_dir, Resources.transcribe_audio_pt)))
        //        sw.Write(Encoding.UTF8.GetString(Resources.transcribe_audio));

        //}

        private static void CreateDbIfNotExists(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var userRolesContext = services.GetRequiredService<UsersRolesDbContext>();
                    var montageContext = services.GetRequiredService<MontageDbContext>();
                    var manager = services.GetRequiredService<UserManager<IdentityUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    UsersRolesDbInitializer.Initialize(userRolesContext, manager, roleManager).Wait();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the Users portion of the database.");
                }

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
