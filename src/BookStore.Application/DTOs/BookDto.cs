using BookStore.Domain.Enums;

namespace BookStore.Application.DTOs;

public class BookDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public BookCategory Category { get; set; }
    public DateTime PublishedDate { get; set; }
    public string Publisher { get; set; } = string.Empty;
    public int Pages { get; set; }
    public BookStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateBookDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public BookCategory Category { get; set; }
    public DateTime PublishedDate { get; set; }
    public string Publisher { get; set; } = string.Empty;
    public int Pages { get; set; }
}

public class UpdateBookDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public BookCategory Category { get; set; }
    public string Publisher { get; set; } = string.Empty;
    public int Pages { get; set; }
}

public class BookSearchDto
{
    public string? SearchTerm { get; set; }
    public BookCategory? Category { get; set; }
    public BookStatus? Status { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
} 