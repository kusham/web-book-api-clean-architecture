using BookStore.Application.Interfaces;
using MediatR;
using FluentValidation;

namespace BookStore.Application.Features.Customers.Commands;

public class DeleteCustomerCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Customer ID is required");
    }
}

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCustomerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        // Start transaction for complex business operation
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id);
            if (customer == null)
                return false;

            // Check if customer has any active orders
            var customerOrders = await _unitOfWork.Orders.GetByCustomerIdAsync(request.Id);
            var activeOrders = customerOrders.Where(o => o.Status != Domain.Enums.OrderStatus.Delivered && 
                                                        o.Status != Domain.Enums.OrderStatus.Cancelled);

            if (activeOrders.Any())
                throw new InvalidOperationException($"Cannot delete customer with ID {request.Id}. Customer has {activeOrders.Count()} active orders.");

            // Delete customer
            await _unitOfWork.Customers.DeleteAsync(request.Id);
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