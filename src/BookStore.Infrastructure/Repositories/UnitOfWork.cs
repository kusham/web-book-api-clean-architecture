using BookStore.Application.Interfaces;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookStore.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly BookStoreDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public IBookRepository Books { get; }
    public ICustomerRepository Customers { get; }
    public IOrderRepository Orders { get; }

    public UnitOfWork(BookStoreDbContext context, IBookRepository books, ICustomerRepository customers, IOrderRepository orders)
    {
        _context = context;
        Books = books;
        Customers = customers;
        Orders = orders;
    }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Log the exception here
            throw;
        }
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction == null)
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch (Exception)
        {
            await RollbackTransactionAsync();
            throw;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
        _disposed = true;
    }
} 