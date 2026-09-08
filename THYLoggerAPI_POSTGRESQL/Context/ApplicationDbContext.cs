using Microsoft.EntityFrameworkCore;
using THYLoggerAPI_POSTGRESQL.Model;

namespace THYLoggerAPI_POSTGRESQL.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<BosDolu> BosDolu { get; set; }
        public DbSet<Gpsdatum> Gpsdatum { get; set; }
        public DbSet<Nem> Nem { get; set; }
        public DbSet<Sicaklik> Sicaklik { get; set; }
        public DbSet<Dolly> Dolly { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Authorization
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Page> Pages { get; set; }

        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // ==========================================
            // AUDIT LOG
            // ==========================================

            modelBuilder.Entity<AuditLog>(builder =>
            {
                builder.Property(a => a.OldValues)
                    .HasColumnType("jsonb");

                builder.Property(a => a.NewValues)
                    .HasColumnType("jsonb");
            });


            // ==========================================
            // USER
            // ==========================================

            modelBuilder.Entity<User>(builder =>
            {
                builder.HasKey(x => x.Id);

                builder.HasIndex(x => x.UserName)
                    .IsUnique();

                builder.HasIndex(x => x.Email)
                    .IsUnique();

                builder.Property(x => x.UserName)
                    .HasMaxLength(100)
                    .IsRequired();

                builder.Property(x => x.Email)
                    .HasMaxLength(200)
                    .IsRequired();

                builder.Property(x => x.PasswordHash)
                    .IsRequired();
            });


            // ==========================================
            // ROLE
            // ==========================================

            modelBuilder.Entity<Role>(builder =>
            {
                builder.HasKey(x => x.Id);

                builder.HasIndex(x => x.Name)
                    .IsUnique();

                builder.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();
            });


            // ==========================================
            // PERMISSION
            // ==========================================

            modelBuilder.Entity<Permission>(builder =>
            {
                builder.HasKey(x => x.Id);

                builder.HasIndex(x => x.Code)
                    .IsUnique();

                builder.Property(x => x.Name)
                    .HasMaxLength(150)
                    .IsRequired();

                builder.Property(x => x.Code)
                    .HasMaxLength(150)
                    .IsRequired();
            });


            // ==========================================
            // PAGE
            // ==========================================

            modelBuilder.Entity<Page>(builder =>
            {
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Name)
                    .HasMaxLength(150)
                    .IsRequired();

                builder.Property(x => x.Route)
                    .HasMaxLength(300)
                    .IsRequired();

                builder.Property(x => x.PermissionCode)
                    .HasMaxLength(150)
                    .IsRequired();
            });


            // ==========================================
            // USER ROLE
            // ==========================================

            modelBuilder.Entity<UserRole>(builder =>
            {
                builder.HasKey(x => new
                {
                    x.UserId,
                    x.RoleId
                });

                builder.HasOne(x => x.User)
                    .WithMany(x => x.UserRoles)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(x => x.Role)
                    .WithMany(x => x.UserRoles)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // ==========================================
            // ROLE PERMISSION
            // ==========================================

            modelBuilder.Entity<RolePermission>(builder =>
            {
                builder.HasKey(x => new
                {
                    x.RoleId,
                    x.PermissionId
                });

                builder.HasOne(x => x.Role)
                    .WithMany(x => x.RolePermissions)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(x => x.Permission)
                    .WithMany(x => x.RolePermissions)
                    .HasForeignKey(x => x.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}