using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using BookStore.Domain.ValueObjects;
using BookStore.Infrastructure.Data;
using BookStore.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Infrastructure.Data.Seeder;

public class DatabaseSeederService : IDatabaseSeederService
{
    private readonly BookStoreDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<DatabaseSeederService> _logger;

    public DatabaseSeederService(
        BookStoreDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<DatabaseSeederService> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            if (_context.Database.IsMySql())
            {
                await _context.Database.MigrateAsync();
            }

            await SeedRolesAsync();
            await SeedAdminUserAsync();
            await SeedSampleUserAsync();
            await SeedSampleDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database seeding.");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });
            _logger.LogInformation("Admin role seeded.");
        }

        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new ApplicationRole { Name = "User" });
            _logger.LogInformation("User role seeded.");
        }
    }
    
    private async Task SeedAdminUserAsync()
    {
        if (await _userManager.FindByEmailAsync("admin@bookstore.com") == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin@bookstore.com",
                Email = "admin@bookstore.com",
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                _logger.LogInformation("Admin user seeded.");
            }
        }
    }
    
    private async Task SeedSampleUserAsync()
    {
        if (await _userManager.FindByEmailAsync("user@bookstore.com") == null)
        {
            var sampleUser = new ApplicationUser
            {
                UserName = "user@bookstore.com",
                Email = "user@bookstore.com",
                FirstName = "Sample",
                LastName = "User",
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(sampleUser, "User123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(sampleUser, "User");
                _logger.LogInformation("Sample user seeded.");
            }
        }
    }

    private async Task SeedSampleDataAsync()
    {
        if (!_context.Books.Any())
        {
            var books = new[]
            {
                new Book("The Hitchhiker's Guide to the Galaxy", "Douglas Adams", "9780345391803", 
                         "A comedic science fiction series created by Douglas Adams.", 10.99m, 100, 
                         BookCategory.ScienceFiction, new DateTime(1979, 10, 12), "Megadodo Publications", 224),
                new Book("The Lord of the Rings", "J.R.R. Tolkien", "9780618640157", 
                         "An epic high-fantasy novel.", 22.99m, 50, 
                         BookCategory.Fantasy, new DateTime(1954, 7, 29), "Allen & Unwin", 1178)
            };
            await _context.Books.AddRangeAsync(books);
            _logger.LogInformation("Sample books seeded.");
        }

        if (!_context.Customers.Any())
        {
            var customers = new[]
            {
                new Customer("Alice", "Smith", "alice.smith@example.com", "555-1234", 
                             new Address("123 Main St", "Anytown", "Anystate", "12345", "USA")),
                new Customer("Bob", "Johnson", "bob.johnson@example.com", "555-5678", 
                             new Address("456 Oak Ave", "Somecity", "Anystate", "54321", "USA"))
            };
            await _context.Customers.AddRangeAsync(customers);
            _logger.LogInformation("Sample customers seeded.");
        }
        
        await _context.SaveChangesAsync();

        if (!_context.Orders.Any())
        {
            var customer = await _context.Customers.FirstAsync();
            var book = await _context.Books.FirstAsync();
            
            var order = new Order(customer.Id, customer.Address, PaymentMethod.CreditCard, "Test order");
            order.AddOrderItem(book, 1);
            
            await _context.Orders.AddAsync(order);
            _logger.LogInformation("Sample order seeded.");
        }

        await _context.SaveChangesAsync();
    }
} 