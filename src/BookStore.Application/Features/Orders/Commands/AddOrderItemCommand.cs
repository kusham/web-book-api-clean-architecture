using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using MediatR;
using FluentValidation;
using AutoMapper;

namespace BookStore.Application.Features.Orders.Commands;

public class AddOrderItemCommand : IRequest<OrderDto>
{
    public Guid OrderId { get; set; }
    public Guid BookId { get; set; }
    public int Quantity { get; set; }
}

public class AddOrderItemCommandValidator : AbstractValidator<AddOrderItemCommand>
{
    public AddOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.BookId)
            .NotEmpty().WithMessage("Book ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");
    }
}

public class AddOrderItemCommandHandler : IRequestHandler<AddOrderItemCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AddOrderItemCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderDto> Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
        // Start transaction for complex business operation
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
            if (order == null)
                throw new InvalidOperationException($"Order with ID {request.OrderId} not found");

            // Only allow adding items to pending orders
            if (order.Status != Domain.Enums.OrderStatus.Pending)
                throw new InvalidOperationException($"Cannot add items to order with status {order.Status}. Only pending orders can be modified.");

            var book = await _unitOfWork.Books.GetByIdAsync(request.BookId);
            if (book == null)
                throw new InvalidOperationException($"Book with ID {request.BookId} not found");

            if (book.StockQuantity < request.Quantity)
                throw new InvalidOperationException($"Insufficient stock for book '{book.Title}'. Available: {book.StockQuantity}, Requested: {request.Quantity}");

            // Add item to order (this will also reserve stock)
            order.AddOrderItem(book, request.Quantity);

            // Update book stock
            await _unitOfWork.Books.UpdateAsync(book);
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            return _mapper.Map<OrderDto>(order);
        }
        catch
        {
            // Rollback transaction on any error
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
} 