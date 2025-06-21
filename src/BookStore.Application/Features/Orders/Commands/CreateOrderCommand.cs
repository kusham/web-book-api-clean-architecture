using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using BookStore.Domain.ValueObjects;
using AutoMapper;
using MediatR;
using FluentValidation;

namespace BookStore.Application.Features.Orders.Commands;

public class CreateOrderCommand : IRequest<OrderDto>
{
    public Guid CustomerId { get; set; }
    public AddressDto ShippingAddress { get; set; } = new();
    public PaymentMethod PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public List<CreateOrderItemDto> OrderItems { get; set; } = new();
}

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping address is required");

        RuleFor(x => x.OrderItems)
            .NotEmpty().WithMessage("Order must contain at least one item");

        RuleForEach(x => x.OrderItems).ChildRules(item =>
        {
            item.RuleFor(x => x.BookId)
                .NotEmpty().WithMessage("Book ID is required");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");
        });

        RuleFor(x => x.ShippingAddress.Street)
            .NotEmpty().WithMessage("Street is required")
            .MaximumLength(200).WithMessage("Street cannot exceed 200 characters");

        RuleFor(x => x.ShippingAddress.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.ShippingAddress.State)
            .NotEmpty().WithMessage("State is required")
            .MaximumLength(50).WithMessage("State cannot exceed 50 characters");

        RuleFor(x => x.ShippingAddress.ZipCode)
            .NotEmpty().WithMessage("Zip code is required")
            .MaximumLength(20).WithMessage("Zip code cannot exceed 20 characters");

        RuleFor(x => x.ShippingAddress.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters");
    }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Start transaction for complex business operation
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Validate customer exists
            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
            if (customer == null)
                throw new InvalidOperationException($"Customer with ID {request.CustomerId} not found");

            // Create shipping address
            var shippingAddress = new Address(
                request.ShippingAddress.Street,
                request.ShippingAddress.City,
                request.ShippingAddress.State,
                request.ShippingAddress.ZipCode,
                request.ShippingAddress.Country
            );

            // Create order
            var order = new Order(request.CustomerId, shippingAddress, request.PaymentMethod, request.Notes);

            // Process each order item
            foreach (var itemDto in request.OrderItems)
            {
                var book = await _unitOfWork.Books.GetByIdAsync(itemDto.BookId);
                if (book == null)
                    throw new InvalidOperationException($"Book with ID {itemDto.BookId} not found");

                if (book.StockQuantity < itemDto.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for book '{book.Title}'. Available: {book.StockQuantity}, Requested: {itemDto.Quantity}");

                // Add item to order (this will also reserve stock)
                order.AddOrderItem(book, itemDto.Quantity);

                // Update book stock
                await _unitOfWork.Books.UpdateAsync(book);
            }

            // Save order
            var createdOrder = await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            return _mapper.Map<OrderDto>(createdOrder);
        }
        catch
        {
            // Rollback transaction on any error
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
} 