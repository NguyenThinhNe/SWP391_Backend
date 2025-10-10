using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Entities;

namespace WarrantyManagement.DAL.Data.Context
{
    public class WarrantyDbContext : DbContext
    {
        public WarrantyDbContext(DbContextOptions<WarrantyDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ServiceCenter> ServiceCenters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ServiceCenter>()
                .HasMany(sc => sc.Users)
                .WithOne(u => u.ServiceCenter)
                .HasForeignKey(u => u.ServiceCenterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
