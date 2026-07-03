using Microsoft.EntityFrameworkCore;

namespace THYLoggerAPI_POSTGRESQL.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<THYLoggerAPI_POSTGRESQL.Model.BosDolu> BosDolu { get; set; }
        public DbSet<THYLoggerAPI_POSTGRESQL.Model.Gpsdatum> Gpsdatum { get; set; }
        public DbSet<THYLoggerAPI_POSTGRESQL.Model.Nem> Nem { get; set; }
        public DbSet<THYLoggerAPI_POSTGRESQL.Model.Sicaklik> Sicaklik { get; set; }
        public DbSet<THYLoggerAPI_POSTGRESQL.Model.Dolly> Dolly { get; set; }
    }
}
