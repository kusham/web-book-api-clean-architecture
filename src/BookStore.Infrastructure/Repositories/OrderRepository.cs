using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly BookStoreDbContext _context;
    public OrderRepository(BookStoreDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
        => await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == id);

    public async Task<IEnumerable<Order>> GetAllAsync()
        => await _context.Orders.Include(o => o.OrderItems).ToListAsync();

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId)
        => await _context.Orders.Include(o => o.OrderItems).Where(o => o.CustomerId == customerId).ToListAsync();

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
        => await _context.Orders.Include(o => o.OrderItems).Where(o => o.Status == status).ToListAsync();

    public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        => await _context.Orders.Include(o => o.OrderItems).Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate).ToListAsync();

    public async Task<Order> AddAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task DeleteAsync(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
        => await _context.Orders.AnyAsync(o => o.Id == id);
} 