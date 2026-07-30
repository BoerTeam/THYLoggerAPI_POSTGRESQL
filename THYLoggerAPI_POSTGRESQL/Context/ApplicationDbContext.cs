using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Model;

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

        // 1. Audit Trail için eklenen tablo
        public DbSet<AuditLog> AuditLogs { get; set; }

        // 2. PostgreSQL JSONB Tipi Yapılandırması
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuditLog>(builder =>
            {
                builder.Property(a => a.OldValues).HasColumnType("jsonb");
                builder.Property(a => a.NewValues).HasColumnType("jsonb");
            });
        }
    }
}