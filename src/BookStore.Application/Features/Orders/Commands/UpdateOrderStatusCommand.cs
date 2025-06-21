using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Enums;
using AutoMapper;
using MediatR;
using FluentValidation;

namespace BookStore.Application.Features.Orders.Commands;

public class UpdateOrderStatusCommand : IRequest<OrderDto>
{
    public Guid OrderId { get; set; }
    public OrderStatus NewStatus { get; set; }
}

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");
    }
}

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateOrderStatusCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderDto> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        if (order == null)
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found");

        // Update order status based on the new status
        switch (request.NewStatus)
        {
            case OrderStatus.Confirmed:
                order.ConfirmOrder();
                break;
            case OrderStatus.Shipped:
                order.ShipOrder();
                break;
            case OrderStatus.Delivered:
                order.DeliverOrder();
                break;
            case OrderStatus.Cancelled:
                order.CancelOrder();
                break;
            default:
                throw new InvalidOperationException($"Invalid status transition to {request.NewStatus}");
        }

        var updatedOrder = await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<OrderDto>(updatedOrder);
    }
} 