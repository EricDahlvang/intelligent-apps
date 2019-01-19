namespace ContosoHelpdeskChatBot.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Configuration;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    public partial class ContosoHelpdeskContext : DbContext
    {
        public ContosoHelpdeskContext()
        {
            //"name=ContosoHelpdeskContext"
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppMsi>()
               .Property(e => e.AppName)
               .IsUnicode(false);

            modelBuilder.Entity<AppMsi>()
                .Property(e => e.MsiPackage)
                .IsUnicode(false);

            modelBuilder.Entity<InstallApp>()
                .Property(e => e.AppName)
                .IsUnicode(false);

            modelBuilder.Entity<InstallApp>()
                .Property(e => e.MachineName)
                .IsUnicode(false);

            modelBuilder.Entity<ResetPassword>()
                .Property(e => e.EmailAddress)
                .IsUnicode(false);
        }
        public virtual DbSet<AppMsi> AppMsis { get; set; }
        public virtual DbSet<InstallApp> InstallApps { get; set; }
        public virtual DbSet<LocalAdmin> LocalAdmins { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<ResetPassword> ResetPasswords { get; set; }
        
    }
}
