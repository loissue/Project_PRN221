using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace PRN221_SE1729_Group11_Project.Models
{
    public partial class PRN_PROJECTContext : DbContext
    {
        public PRN_PROJECTContext()
        {
        }

        public PRN_PROJECTContext(DbContextOptions<PRN_PROJECTContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Booking> Bookings { get; set; } = null!;
        public virtual DbSet<Customer> Customers { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var conf = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();
                optionsBuilder.UseSqlServer(conf.GetConnectionString("DefaultConnection"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.Property(e => e.BookingId).HasColumnName("BookingID");

                entity.Property(e => e.Cid).HasColumnName("CID");

                entity.Property(e => e.Note).HasMaxLength(200);

                entity.Property(e => e.Pid).HasColumnName("PID");

                entity.Property(e => e.RentDate).HasColumnType("date");

                entity.HasOne(d => d.CidNavigation)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.Cid)
                    .HasConstraintName("FK_Bookings_Customers");

                entity.HasOne(d => d.PidNavigation)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.Pid)
                    .HasConstraintName("FK_Bookings_Products");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Cid);

                entity.Property(e => e.Cid).HasColumnName("CID");

                entity.Property(e => e.CustomerName).HasMaxLength(100);

                entity.Property(e => e.Dob).HasColumnType("date");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Pid);

                entity.Property(e => e.Pid).HasColumnName("PID");

                entity.Property(e => e.ProductName).HasMaxLength(200);

                entity.Property(e => e.Relate).HasMaxLength(200);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
