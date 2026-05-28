using ManPower.Modelos;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ManPower.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Herramientas> Herramientas { get; set; }

    }
}