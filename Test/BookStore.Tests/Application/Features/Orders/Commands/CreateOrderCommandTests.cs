using BookStore.Application.Features.Orders.Commands;
using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using BookStore.Domain.ValueObjects;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;

namespace BookStore.Tests.Application.Features.Orders.Commands;

public class CreateOrderCommandTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _handler = new CreateOrderCommandHandler(_mockUnitOfWork.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateOrderWithTransaction()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var command = new CreateOrderCommand
        {
            CustomerId = customerId,
            ShippingAddress = new BookStore.Application.DTOs.AddressDto
            {
                Street = "123 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                Country = "USA"
            },
            PaymentMethod = PaymentMethod.CreditCard,
            Notes = "Test order",
            OrderItems = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { BookId = bookId, Quantity = 2 }
            }
        };

        var customer = new Customer("John", "Doe", "john@email.com", "+1234567890", 
            new Address("123 Main St", "New York", "NY", "10001", "USA"));

        var book = new Book("Clean Code", "Robert Martin", "9780132350884", 
            "A handbook of agile software craftsmanship", 45.99m, 10, 
            BookCategory.Technology, DateTime.Now, "Prentice Hall", 464);

        var order = new Order(customerId, new Address("123 Main St", "New York", "NY", "10001", "USA"), 
            PaymentMethod.CreditCard, "Test order");

        var orderDto = new BookStore.Application.DTOs.OrderDto
        {
            Id = orderId,
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            Total = 91.98m
        };

        // Setup mocks
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.Customers.GetByIdAsync(customerId)).ReturnsAsync(customer);
        _mockUnitOfWork.Setup(x => x.Books.GetByIdAsync(bookId)).ReturnsAsync(book);
        _mockUnitOfWork.Setup(x => x.Orders.AddAsync(It.IsAny<Order>())).ReturnsAsync(order);
        _mockUnitOfWork.Setup(x => x.Books.UpdateAsync(It.IsAny<Book>())).ReturnsAsync(book);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        _mockUnitOfWork.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);
        _mockMapper.Setup(x => x.Map<BookStore.Application.DTOs.OrderDto>(order)).Returns(orderDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
        result.CustomerId.Should().Be(customerId);
        result.Status.Should().Be(OrderStatus.Pending);

        // Verify transaction was used
        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Never);

        // Verify all operations were called
        _mockUnitOfWork.Verify(x => x.Customers.GetByIdAsync(customerId), Times.Once);
        _mockUnitOfWork.Verify(x => x.Books.GetByIdAsync(bookId), Times.Once);
        _mockUnitOfWork.Verify(x => x.Orders.AddAsync(It.IsAny<Order>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.Books.UpdateAsync(It.IsAny<Book>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCustomer_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var command = new CreateOrderCommand
        {
            CustomerId = customerId,
            ShippingAddress = new BookStore.Application.DTOs.AddressDto
            {
                Street = "123 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                Country = "USA"
            },
            PaymentMethod = PaymentMethod.CreditCard,
            OrderItems = new List<CreateOrderItemDto>()
        };

        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.Customers.GetByIdAsync(customerId)).ReturnsAsync((Customer?)null);
        _mockUnitOfWork.Setup(x => x.RollbackTransactionAsync()).Returns(Task.CompletedTask);

        // Act & Assert
        var action = () => _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Customer with ID {customerId} not found");

        _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInsufficientStock_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        var command = new CreateOrderCommand
        {
            CustomerId = customerId,
            ShippingAddress = new BookStore.Application.DTOs.AddressDto
            {
                Street = "123 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                Country = "USA"
            },
            PaymentMethod = PaymentMethod.CreditCard,
            OrderItems = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { BookId = bookId, Quantity = 5 }
            }
        };

        var customer = new Customer("John", "Doe", "john@email.com", "+1234567890", 
            new Address("123 Main St", "New York", "NY", "10001", "USA"));

        var book = new Book("Clean Code", "Robert Martin", "9780132350884", 
            "A handbook of agile software craftsmanship", 45.99m, 2, // Only 2 in stock
            BookCategory.Technology, DateTime.Now, "Prentice Hall", 464);

        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.Customers.GetByIdAsync(customerId)).ReturnsAsync(customer);
        _mockUnitOfWork.Setup(x => x.Books.GetByIdAsync(bookId)).ReturnsAsync(book);
        _mockUnitOfWork.Setup(x => x.RollbackTransactionAsync()).Returns(Task.CompletedTask);

        // Act & Assert
        var action = () => _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Insufficient stock for book 'Clean Code'. Available: 2, Requested: 5");

        _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }
} 