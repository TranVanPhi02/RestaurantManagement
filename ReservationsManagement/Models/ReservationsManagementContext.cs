using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ReservationsManagement.Models;

public partial class ReservationsManagementContext : DbContext
{
    public ReservationsManagementContext()
    {
    }

    public ReservationsManagementContext(DbContextOptions<ReservationsManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryMenu> CategoryMenus { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Restaurant> Restaurants { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var ConnectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(ConnectionString);
        }

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A0B2A77CE4C");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryName).HasMaxLength(255);
        });

        modelBuilder.Entity<CategoryMenu>(entity =>
        {
            entity.HasKey(e => e.CategoryMenuId).HasName("PK__Category__D203517D85FA0706");

            entity.ToTable("CategoryMenu");

            entity.Property(e => e.CategoryMenuName).HasMaxLength(255);

            entity.HasOne(d => d.Restaurant).WithMany(p => p.CategoryMenus)
                .HasForeignKey(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CategoryM__Resta__5CD6CB2B");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK__Contact__5C66259B36FE212F");

            entity.ToTable("Contact");

            entity.Property(e => e.ContactDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Topic).HasMaxLength(255);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64D8BF3643B2");

            entity.ToTable("Customer");

            entity.Property(e => e.Dob).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(255);
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__Image__7516F70C339E8B42");

            entity.ToTable("Image");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.Images)
                .HasForeignKey(d => d.RestaurantId)
                .HasConstraintName("FK__Image__Restauran__3F466844");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.MenuId).HasName("PK__Menu__C99ED23023645540");

            entity.ToTable("Menu");

            entity.Property(e => e.DishName).HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.CategoryMenu).WithMany(p => p.Menus)
                .HasForeignKey(d => d.CategoryMenuId)
                .HasConstraintName("FK__Menu__CategoryMe__60A75C0F");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.Menus)
                .HasForeignKey(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Menu__Restaurant__5FB337D6");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCFC307213B");

            entity.Property(e => e.AcceptDate).HasColumnType("datetime");
            entity.Property(e => e.CancelDate).HasColumnType("datetime");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.DoneDate).HasColumnType("datetime");
            entity.Property(e => e.NameReceiver).HasMaxLength(255);
            entity.Property(e => e.PhoneReceiver).HasMaxLength(50);
            entity.Property(e => e.ProcessDate).HasColumnType("datetime");
            entity.Property(e => e.StatusOrder).HasMaxLength(50);
            entity.Property(e => e.StatusPayment).HasMaxLength(50);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Orders__Customer__4222D4EF");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.Orders)
                .HasForeignKey(d => d.RestaurantId)
                .HasConstraintName("FK__Orders__Restaura__4316F928");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D36CB118A96E");

            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalPrice)
                .HasComputedColumnSql("([Quantity]*[Price])", true)
                .HasColumnType("decimal(29, 2)");

            entity.HasOne(d => d.Menu).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.MenuId)
                .HasConstraintName("FK_OrderDetails_Menu");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderDetails_Order");
        });

        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(e => e.ResId).HasName("PK__Restaura__297882F626E19D75");

            entity.ToTable("Restaurant");

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Discount).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Location).HasMaxLength(500);
            entity.Property(e => e.NameOwner).HasMaxLength(255);
            entity.Property(e => e.NameRes).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(255);

            entity.HasOne(d => d.Category).WithMany(p => p.Restaurants)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Restauran__Categ__38996AB5");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.RateId).HasName("PK__Review__58A7CF5CF83C1BDE");

            entity.ToTable("Review");

            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Customer).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Review__Customer__46E78A0C");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.RestaurantId)
                .HasConstraintName("FK__Review__Restaura__47DBAE45");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
