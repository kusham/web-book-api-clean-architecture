using BookStore.Application.Interfaces;
using MediatR;
using FluentValidation;

namespace BookStore.Application.Features.Orders.Commands;

public class DeleteOrderCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteOrderCommandValidator : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Order ID is required");
    }
}

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        // Start transaction for complex business operation
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(request.Id);
            if (order == null)
                return false;

            // Only allow deletion of pending or cancelled orders
            if (order.Status != Domain.Enums.OrderStatus.Pending && 
                order.Status != Domain.Enums.OrderStatus.Cancelled)
            {
                throw new InvalidOperationException($"Cannot delete order with status {order.Status}. Only pending or cancelled orders can be deleted.");
            }

            // If order is pending, restore stock for all items
            if (order.Status == Domain.Enums.OrderStatus.Pending)
            {
                foreach (var orderItem in order.OrderItems)
                {
                    var book = await _unitOfWork.Books.GetByIdAsync(orderItem.BookId);
                    if (book != null)
                    {
                        book.UpdateStock(book.StockQuantity + orderItem.Quantity);
                        await _unitOfWork.Books.UpdateAsync(book);
                    }
                }
            }

            // Delete order
            await _unitOfWork.Orders.DeleteAsync(request.Id);
            await _unitOfWork.SaveChangesAsync();

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch
        {
            // Rollback transaction on any error
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
} 