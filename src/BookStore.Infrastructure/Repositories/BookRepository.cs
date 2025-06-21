using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly BookStoreDbContext _context;
    public BookRepository(BookStoreDbContext context)
    {
        _context = context;
    }

    public async Task<Book?> GetByIdAsync(Guid id)
        => await _context.Books.FindAsync(id);

    public async Task<IEnumerable<Book>> GetAllAsync()
        => await _context.Books.ToListAsync();

    public async Task<IEnumerable<Book>> GetByCategoryAsync(BookCategory category)
        => await _context.Books.Where(b => b.Category == category).ToListAsync();

    public async Task<IEnumerable<Book>> SearchAsync(string searchTerm)
        => await _context.Books.Where(b => b.Title.Contains(searchTerm) || b.Author.Contains(searchTerm)).ToListAsync();

    public async Task<IEnumerable<Book>> GetByStatusAsync(BookStatus status)
        => await _context.Books.Where(b => b.Status == status).ToListAsync();

    public async Task<Book> AddAsync(Book book)
    {
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<Book> UpdateAsync(Book book)
    {
        _context.Books.Update(book);
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task DeleteAsync(Guid id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book != null)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
        => await _context.Books.AnyAsync(b => b.Id == id);

    public async Task<bool> ExistsByIsbnAsync(string isbn)
        => await _context.Books.AnyAsync(b => b.ISBN == isbn);
} 