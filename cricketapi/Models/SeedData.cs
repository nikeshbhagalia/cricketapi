using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cricketapi.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new cricketapiContext(
                serviceProvider.GetRequiredService<DbContextOptions<cricketapiContext>>()))
            {
                // Look for any movies.
                if (context.Player.Count() > 0)
                {
                    return;   // DB has been seeded
                }

                context.Player.AddRange(
                    new Player
                    {
                        Name = "Brendon McCullum",
                        Country = "New Zealand",
                        Runs = "6000",
                        Wickets = "0",
                        Catches = "200",
                        Url = "https://i.pinimg.com/originals/38/50/7e/38507edd2df178149cbf1cb444ea198c.jpg"
                    }


                );
                context.SaveChanges();
            }
        }
    }
}

