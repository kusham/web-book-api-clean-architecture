using BookStore.Application.Features.Customers.Commands;
using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;
using BookStore.Domain.ValueObjects;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;

namespace BookStore.Tests.Application.Features.Customers.Commands;

public class CreateCustomerCommandTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _handler = new CreateCustomerCommandHandler(_mockUnitOfWork.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateCustomer()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@email.com",
            PhoneNumber = "+1234567890",
            Address = new BookStore.Application.DTOs.AddressDto
            {
                Street = "123 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                Country = "USA"
            }
        };

        var address = new Address(
            command.Address.Street,
            command.Address.City,
            command.Address.State,
            command.Address.ZipCode,
            command.Address.Country
        );

        var customer = new Customer(
            command.FirstName,
            command.LastName,
            command.Email,
            command.PhoneNumber,
            address
        );

        var customerDto = new BookStore.Application.DTOs.CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            Status = customer.Status
        };

        _mockUnitOfWork.Setup(x => x.Customers.AddAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockMapper.Setup(x => x.Map<BookStore.Application.DTOs.CustomerDto>(customer))
            .Returns(customerDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be(command.FirstName);
        result.LastName.Should().Be(command.LastName);
        result.Email.Should().Be(command.Email);
        result.PhoneNumber.Should().Be(command.PhoneNumber);

        _mockUnitOfWork.Verify(x => x.Customers.AddAsync(It.IsAny<Customer>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        _mockMapper.Verify(x => x.Map<BookStore.Application.DTOs.CustomerDto>(customer), Times.Once);
    }
} 