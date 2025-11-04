using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerVehicle> CustomerVehicles { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<PartItem> PartItems { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<VehiclePart > VehicleParts { get; set; }
        public DbSet<WarrantyClaim> WarrantyClaims { get; set; }
        public DbSet<WarrantyPolicy> Policies { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<ClaimDetail> ClaimDetails { get; set; }
        public DbSet<ClaimImage> ClaimImages { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // --- ServiceCenter - User 
            modelBuilder.Entity<ServiceCenter>()
                .HasMany(sc => sc.Users)
                .WithOne(u => u.ServiceCenter)
                .HasForeignKey(u => u.ServiceCenterId)
                .OnDelete(DeleteBehavior.Restrict);
            // --- Customer - Vehicle 
            modelBuilder.Entity<Customer>()
                .HasMany(cv => cv.CustomerVehicles)
                .WithOne(c => c.Customer)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Vehicle - VehiclePart
            modelBuilder.Entity<CustomerVehicle>()
                .HasMany(vp => vp.VehicleParts)
                .WithOne(cv => cv.Vehicle)
                .HasForeignKey(v => v.VIN)
                .OnDelete(DeleteBehavior.Restrict);

            // ---  Inventory - Part items
            modelBuilder.Entity<Inventory>()
                .HasMany(p => p.PartItems)
                .WithOne(i => i.Inventory)
                .HasForeignKey(i => i.InventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            //--- User- WarrantyClaim
            modelBuilder.Entity<User>()
                .HasMany(u => u.WarrantyClaims)
                .WithOne(wc => wc.User)
                .HasForeignKey(wc => wc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //--- WarrantyClaim - ClaimDetail
            modelBuilder.Entity<WarrantyClaim>()
                .HasMany(wc => wc.ClaimDetails)
                .WithOne(cd => cd.WarrantyClaim)
                .HasForeignKey(cd => cd.ClaimId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<WarrantyClaim>()
                .HasMany(wc => wc.Images)
                .WithOne(ci => ci.WarrantyClaim)
                .HasForeignKey(ci => ci.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);
            //--- PartItem - ClaimDetail
            modelBuilder.Entity<PartItem>()
                .HasMany(pi => pi.ClaimDetails)
                .WithOne(cd => cd.PartItem)
                .HasForeignKey(cd => cd.PartItemId)
                .OnDelete(DeleteBehavior.Restrict);

            //--- WarrantyPolicy - WarrantyClaim
            modelBuilder.Entity<WarrantyPolicy>()
                .HasMany(wc => wc.warrantyClaims)
                .WithOne(wp => wp.WarrantyPolicy)
                .HasForeignKey(wc => wc.PolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            //--- WorkOrder - Part
            modelBuilder.Entity<WorkOrder>()
                .HasMany(p => p.Parts)
                .WithOne(wo => wo.WorkOrder)
                .HasForeignKey(wo => wo.WorkOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            //--- User - Report
            modelBuilder.Entity<User>()
                .HasMany(u => u.Reports)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

           
            //--- Part - VehiclePart
            modelBuilder.Entity<Part>()
                .HasMany(vp => vp.VehicleParts)
                .WithOne(p => p.Part)
                .HasForeignKey(p => p.PartId)
                .OnDelete(DeleteBehavior.Restrict);

            //--- Part - PartItem
            modelBuilder.Entity<Part>()
                .HasMany(pi => pi.PartItems)
                .WithOne(p => p.Part)
                .HasForeignKey(p => p.PartId)
                .OnDelete(DeleteBehavior.Restrict);

            //--- Campaign - CustomerVehicle
            modelBuilder.Entity<Campaign>()
                .HasMany(cv => cv.CustomerVehicles)
                .WithOne(c => c.Campaign)
                .HasForeignKey(c => c.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);
            // --- User - Campaign (one-to-one)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Campaign)
                .WithOne(c => c.User)
                .HasForeignKey<Campaign>(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            // --- Some optional text fields max length
            modelBuilder.Entity<User>()
                .Property(u => u.UserName)
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.Password)
                .HasMaxLength(100);
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                            v => v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                        ));
                    }
                }
            }
        }
    }
}
