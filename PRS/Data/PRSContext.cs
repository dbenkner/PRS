using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRS.Models;

namespace PRS.Data
{
    public class PRSContext : DbContext
    {
        public PRSContext (DbContextOptions<PRSContext> options)
            : base(options)
        {
        }

        public DbSet<PRS.Models.User> Users { get; set; } = default!;
    }
}
