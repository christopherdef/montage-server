using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MontageServer.Data
{
    public class MontageDbInitializer
    {

        public static void Initialize(MontageDbContext context)
        {
            context.Database.Migrate();

            context.SaveChanges();
        }
    }
}
