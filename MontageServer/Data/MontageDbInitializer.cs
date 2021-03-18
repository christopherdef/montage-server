using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MontageServer.Data
{
    public class MontageDbInitializer
    {

        public static void Initialize(MontageDbContext context, ILogger logger)
        {
            // robust migration application
            var Migrate = new Action(() => {
                context.Database.Migrate();
                context.SaveChanges();
                logger.LogInformation("Applied migrations to MontageDB");
            });

            try
            {
                Migrate();
            }
            catch (SqlException e)
            {
                logger.LogWarning("Unable to apply migrations to existing MontageDB", e);
                //context.Database.EnsureDeleted();
                //context.SaveChanges();
                //logger.LogWarning("Deleted old MontageDB");

                Migrate();
                throw e;
            }


        }
    }
}
