using BookStore.API;
using BookStore.Application.DTOs;
using BookStore.Application.Features.Books.Commands;
using BookStore.Application.Features.Books.Queries;
using BookStore.Domain.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BookStore.Tests.API;

public class BooksControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly BooksController _controller;

    public BooksControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new BooksController(_mockMediator.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkResult()
    {
        // Arrange
        var books = new List<BookDto>
        {
            new BookDto
            {
                Id = Guid.NewGuid(),
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ISBN = "9780132350884",
                Price = 45.99m,
                Category = BookCategory.Technology,
                Status = BookStatus.Available
            }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<GetAllBooksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedBooks = okResult.Value.Should().BeOfType<List<BookDto>>().Subject;
        returnedBooks.Should().HaveCount(1);
        returnedBooks[0].Title.Should().Be("Clean Code");
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = new BookDto
        {
            Id = bookId,
            Title = "Clean Code",
            Author = "Robert C. Martin",
            ISBN = "9780132350884",
            Price = 45.99m,
            Category = BookCategory.Technology,
            Status = BookStatus.Available
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<GetBookByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        // Act
        var result = await _controller.GetById(bookId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedBook = okResult.Value.Should().BeOfType<BookDto>().Subject;
        returnedBook.Id.Should().Be(bookId);
        returnedBook.Title.Should().Be("Clean Code");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _mockMediator.Setup(x => x.Send(It.IsAny<GetBookByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookDto?)null);

        // Act
        var result = await _controller.GetById(bookId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_WithValidCommand_ShouldReturnCreatedAtAction()
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

        var createdBook = new BookDto
        {
            Id = Guid.NewGuid(),
            Title = command.Title,
            Author = command.Author,
            ISBN = command.ISBN,
            Price = command.Price,
            Category = command.Category,
            Status = BookStatus.Available
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<CreateBookCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdBook);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var createdAtActionResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAtActionResult.ActionName.Should().Be(nameof(BooksController.GetById));
        createdAtActionResult.RouteValues!["id"].Should().Be(createdBook.Id);
        
        var returnedBook = createdAtActionResult.Value.Should().BeOfType<BookDto>().Subject;
        returnedBook.Title.Should().Be(command.Title);
        returnedBook.Author.Should().Be(command.Author);
    }
} 