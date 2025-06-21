using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using AutoMapper;
using MediatR;
using FluentValidation;

namespace BookStore.Application.Features.Orders.Queries;

public class GetOrdersByCustomerQuery : IRequest<IEnumerable<OrderDto>>
{
    public Guid CustomerId { get; set; }
}

public class GetOrdersByCustomerQueryValidator : AbstractValidator<GetOrdersByCustomerQuery>
{
    public GetOrdersByCustomerQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");
    }
}

public class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, IEnumerable<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOrdersByCustomerQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.Orders.GetByCustomerIdAsync(request.CustomerId);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }
} 