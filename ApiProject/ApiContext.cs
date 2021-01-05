using ApiProject.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiProject
{
    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options) 
            : base(options)
        {

        }

        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);
                e.Property(x => x.LastName)
                    .HasMaxLength(50);
                e.Property(x => x.Username)
                    .IsRequired()
                    .HasMaxLength(50);
                e.Property(x => x.Password)
                    .IsRequired()
                    .HasMaxLength(50);
            });         

        }
    }
}
