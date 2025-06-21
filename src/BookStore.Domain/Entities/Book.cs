using BookStore.Domain.Enums;
using BookStore.Domain.Exceptions;
using BookStore.Domain.ValueObjects;

namespace BookStore.Domain.Entities;

public class Book : BaseEntity
{
    public string Title { get; private set; }
    public string Author { get; private set; }
    public string ISBN { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public BookCategory Category { get; private set; }
    public DateTime PublishedDate { get; private set; }
    public string Publisher { get; private set; }
    public int Pages { get; private set; }
    public BookStatus Status { get; private set; }

    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();

    private Book() { } // For EF Core

    public Book(string title, string author, string isbn, string description, 
                decimal price, int stockQuantity, BookCategory category, 
                DateTime publishedDate, string publisher, int pages)
    {
        ValidateBookData(title, author, isbn, price, stockQuantity, pages);
        
        Title = title;
        Author = author;
        ISBN = isbn;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        Category = category;
        PublishedDate = publishedDate;
        Publisher = publisher;
        Pages = pages;
        Status = BookStatus.Available;
    }

    public void UpdateStock(int newQuantity)
    {
        if (newQuantity < 0)
            throw new DomainException("Stock quantity cannot be negative.");
        
        StockQuantity = newQuantity;
        Status = newQuantity > 0 ? BookStatus.Available : BookStatus.OutOfStock;
    }

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Reservation quantity must be positive.");
        
        if (StockQuantity < quantity)
            throw new DomainException("Insufficient stock for reservation.");
        
        StockQuantity -= quantity;
        
        if (StockQuantity == 0)
            Status = BookStatus.OutOfStock;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new DomainException("Price cannot be negative.");
        
        Price = newPrice;
    }

    public void UpdateDetails(string title, string author, string description, 
                            BookCategory category, string publisher, int pages)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(author))
            throw new DomainException("Author cannot be empty.");
        
        if (pages <= 0)
            throw new DomainException("Pages must be positive.");
        
        Title = title;
        Author = author;
        Description = description;
        Category = category;
        Publisher = publisher;
        Pages = pages;
    }

    private static void ValidateBookData(string title, string author, string isbn, 
                                       decimal price, int stockQuantity, int pages)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(author))
            throw new DomainException("Author cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(isbn))
            throw new DomainException("ISBN cannot be empty.");
        
        if (price < 0)
            throw new DomainException("Price cannot be negative.");
        
        if (stockQuantity < 0)
            throw new DomainException("Stock quantity cannot be negative.");
        
        if (pages <= 0)
            throw new DomainException("Pages must be positive.");
    }
} 