using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Enums;
using AutoMapper;
using MediatR;
using FluentValidation;

namespace BookStore.Application.Features.Orders.Queries;

public class GetOrdersByStatusQuery : IRequest<IEnumerable<OrderDto>>
{
    public OrderStatus Status { get; set; }
}

public class GetOrdersByStatusQueryValidator : AbstractValidator<GetOrdersByStatusQuery>
{
    public GetOrdersByStatusQueryValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid order status");
    }
}

public class GetOrdersByStatusQueryHandler : IRequestHandler<GetOrdersByStatusQuery, IEnumerable<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOrdersByStatusQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.Orders.GetByStatusAsync(request.Status);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }
} 