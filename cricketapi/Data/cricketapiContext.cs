using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cricketapi.Models
{
    public class cricketapiContext : DbContext
    {
        public cricketapiContext (DbContextOptions<cricketapiContext> options)
            : base(options)
        {
        }

        public DbSet<cricketapi.Models.Player> Player { get; set; }
    }
}
