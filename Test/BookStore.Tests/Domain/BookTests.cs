using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using BookStore.Domain.Exceptions;
using BookStore.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace BookStore.Tests.Domain;

public class BookTests
{
    [Fact]
    public void CreateBook_WithValidData_ShouldCreateBook()
    {
        // Arrange
        var title = "Clean Code";
        var author = "Robert C. Martin";
        var isbn = "9780132350884";
        var description = "A handbook of agile software craftsmanship";
        var price = 45.99m;
        var stockQuantity = 50;
        var category = BookCategory.Technology;
        var publishedDate = new DateTime(2008, 8, 11);
        var publisher = "Prentice Hall";
        var pages = 464;

        // Act
        var book = new Book(title, author, isbn, description, price, stockQuantity, 
                           category, publishedDate, publisher, pages);

        // Assert
        book.Title.Should().Be(title);
        book.Author.Should().Be(author);
        book.ISBN.Should().Be(isbn);
        book.Description.Should().Be(description);
        book.Price.Should().Be(price);
        book.StockQuantity.Should().Be(stockQuantity);
        book.Category.Should().Be(category);
        book.PublishedDate.Should().Be(publishedDate);
        book.Publisher.Should().Be(publisher);
        book.Pages.Should().Be(pages);
        book.Status.Should().Be(BookStatus.Available);
    }

    [Theory]
    [InlineData("", "Author", "ISBN", "Description", 45.99, 50, 464, "Publisher")]
    [InlineData("Title", "", "ISBN", "Description", 45.99, 50, 464, "Publisher")]
    [InlineData("Title", "Author", "", "Description", 45.99, 50, 464, "Publisher")]
    [InlineData("Title", "Author", "ISBN", "Description", -1, 50, 464, "Publisher")]
    [InlineData("Title", "Author", "ISBN", "Description", 45.99, -1, 464, "Publisher")]
    [InlineData("Title", "Author", "ISBN", "Description", 45.99, 50, 0, "Publisher")]
    public void CreateBook_WithInvalidData_ShouldThrowDomainException(string title, string author, string isbn, 
        string description, decimal price, int stockQuantity, int pages, string publisher)
    {
        // Act & Assert
        var action = () => new Book(title, author, isbn, description, price, stockQuantity, 
                                   BookCategory.Technology, DateTime.Now, publisher, pages);
        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateStock_WithValidQuantity_ShouldUpdateStock()
    {
        // Arrange
        var book = new Book("Title", "Author", "ISBN", "Description", 45.99m, 50, 
                           BookCategory.Technology, DateTime.Now, "Publisher", 464);

        // Act
        book.UpdateStock(25);

        // Assert
        book.StockQuantity.Should().Be(25);
        book.Status.Should().Be(BookStatus.Available);
    }

    [Fact]
    public void UpdateStock_WithZeroQuantity_ShouldSetStatusToOutOfStock()
    {
        // Arrange
        var book = new Book("Title", "Author", "ISBN", "Description", 45.99m, 50, 
                           BookCategory.Technology, DateTime.Now, "Publisher", 464);

        // Act
        book.UpdateStock(0);

        // Assert
        book.StockQuantity.Should().Be(0);
        book.Status.Should().Be(BookStatus.OutOfStock);
    }

    [Fact]
    public void UpdateStock_WithNegativeQuantity_ShouldThrowDomainException()
    {
        // Arrange
        var book = new Book("Title", "Author", "ISBN", "Description", 45.99m, 50, 
                           BookCategory.Technology, DateTime.Now, "Publisher", 464);

        // Act & Assert
        var action = () => book.UpdateStock(-1);
        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void ReserveStock_WithValidQuantity_ShouldReserveStock()
    {
        // Arrange
        var book = new Book("Title", "Author", "ISBN", "Description", 45.99m, 50, 
                           BookCategory.Technology, DateTime.Now, "Publisher", 464);

        // Act
        book.ReserveStock(10);

        // Assert
        book.StockQuantity.Should().Be(40);
    }

    [Fact]
    public void ReserveStock_WithInsufficientStock_ShouldThrowDomainException()
    {
        // Arrange
        var book = new Book("Title", "Author", "ISBN", "Description", 45.99m, 50, 
                           BookCategory.Technology, DateTime.Now, "Publisher", 464);

        // Act & Assert
        var action = () => book.ReserveStock(60);
        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdatePrice_WithValidPrice_ShouldUpdatePrice()
    {
        // Arrange
        var book = new Book("Title", "Author", "ISBN", "Description", 45.99m, 50, 
                           BookCategory.Technology, DateTime.Now, "Publisher", 464);

        // Act
        book.UpdatePrice(39.99m);

        // Assert
        book.Price.Should().Be(39.99m);
    }

    [Fact]
    public void UpdatePrice_WithNegativePrice_ShouldThrowDomainException()
    {
        // Arrange
        var book = new Book("Title", "Author", "ISBN", "Description", 45.99m, 50, 
                           BookCategory.Technology, DateTime.Now, "Publisher", 464);

        // Act & Assert
        var action = () => book.UpdatePrice(-1m);
        action.Should().Throw<DomainException>();
    }
} 