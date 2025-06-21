using Microsoft.EntityFrameworkCore;
using BookStore.Domain.Entities;
using BookStore.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BookStore.Domain.ValueObjects;

namespace BookStore.Infrastructure.Data;

public class BookStoreDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public BookStoreDbContext(DbContextOptions<BookStoreDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Book configuration
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Author).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ISBN).IsRequired().HasMaxLength(13);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Publisher).HasMaxLength(100);
            entity.Property(e => e.Category).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.PublishedDate);
            entity.Property(e => e.Pages);
            
            entity.HasIndex(e => e.ISBN).IsUnique();
        });

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.RegistrationDate);
            entity.Property(e => e.LastLoginDate);
            
            entity.HasIndex(e => e.Email).IsUnique();
            
            // Address value object configuration
            entity.OwnsOne(e => e.Address, address =>
            {
                address.Property(a => a.Street).IsRequired().HasMaxLength(200);
                address.Property(a => a.City).IsRequired().HasMaxLength(100);
                address.Property(a => a.State).IsRequired().HasMaxLength(50);
                address.Property(a => a.ZipCode).IsRequired().HasMaxLength(20);
                address.Property(a => a.Country).IsRequired().HasMaxLength(100);
            });
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.OrderDate);
            entity.Property(e => e.ShippedDate);
            entity.Property(e => e.DeliveredDate);
            entity.Property(e => e.PaymentMethod).HasConversion<int>();
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Tax).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ShippingCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasMaxLength(500);
            
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Orders)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Shipping address value object configuration
            entity.OwnsOne(e => e.ShippingAddress, address =>
            {
                address.Property(a => a.Street).IsRequired().HasMaxLength(200);
                address.Property(a => a.City).IsRequired().HasMaxLength(100);
                address.Property(a => a.State).IsRequired().HasMaxLength(50);
                address.Property(a => a.ZipCode).IsRequired().HasMaxLength(20);
                address.Property(a => a.Country).IsRequired().HasMaxLength(100);
            });
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderId).IsRequired();
            entity.Property(e => e.BookId).IsRequired();
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Quantity);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.Book)
                  .WithMany(b => b.OrderItems)
                  .HasForeignKey(e => e.BookId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
} 