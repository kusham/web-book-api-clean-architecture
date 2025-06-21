using BookStore.Application.Features.Books.Commands;
using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;

namespace BookStore.Tests.Application.Features.Books.Commands;

public class CreateBookCommandTests
{
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CreateBookCommandHandler _handler;

    public CreateBookCommandTests()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _mockMapper = new Mock<IMapper>();
        _handler = new CreateBookCommandHandler(_mockBookRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateBook()
    {
        // Arrange
        var command = new CreateBookCommand
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            ISBN = "9780132350884",
            Description = "A handbook of agile software craftsmanship",
            Price = 45.99m,
            StockQuantity = 50,
            Category = BookCategory.Technology,
            PublishedDate = new DateTime(2008, 8, 11),
            Publisher = "Prentice Hall",
            Pages = 464
        };

        var book = new Book(command.Title, command.Author, command.ISBN, command.Description,
                           command.Price, command.StockQuantity, command.Category,
                           command.PublishedDate, command.Publisher, command.Pages);

        var bookDto = new BookStore.Application.DTOs.BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            Description = book.Description,
            Price = book.Price,
            StockQuantity = book.StockQuantity,
            Category = book.Category,
            PublishedDate = book.PublishedDate,
            Publisher = book.Publisher,
            Pages = book.Pages,
            Status = book.Status
        };

        _mockBookRepository.Setup(x => x.AddAsync(It.IsAny<Book>()))
            .ReturnsAsync(book);

        _mockMapper.Setup(x => x.Map<BookStore.Application.DTOs.BookDto>(book))
            .Returns(bookDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(command.Title);
        result.Author.Should().Be(command.Author);
        result.ISBN.Should().Be(command.ISBN);
        result.Price.Should().Be(command.Price);
        result.StockQuantity.Should().Be(command.StockQuantity);

        _mockBookRepository.Verify(x => x.AddAsync(It.IsAny<Book>()), Times.Once);
        _mockMapper.Verify(x => x.Map<BookStore.Application.DTOs.BookDto>(book), Times.Once);
    }
} 