using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Enums;
using AutoMapper;
using MediatR;
using FluentValidation;

namespace BookStore.Application.Features.Customers.Commands;

public class UpdateCustomerStatusCommand : IRequest<CustomerDto>
{
    public Guid Id { get; set; }
    public CustomerStatus NewStatus { get; set; }
}

public class UpdateCustomerStatusCommandValidator : AbstractValidator<UpdateCustomerStatusCommand>
{
    public UpdateCustomerStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Customer ID is required");
    }
}

public class UpdateCustomerStatusCommandHandler : IRequestHandler<UpdateCustomerStatusCommand, CustomerDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCustomerStatusCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CustomerDto> Handle(UpdateCustomerStatusCommand request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id);
        if (customer == null)
            throw new InvalidOperationException($"Customer with ID {request.Id} not found");

        // Update customer status based on the new status
        switch (request.NewStatus)
        {
            case CustomerStatus.Active:
                customer.Activate();
                break;
            case CustomerStatus.Inactive:
                customer.Deactivate();
                break;
            case CustomerStatus.Suspended:
                customer.Deactivate(); // For now, treat suspended as inactive
                break;
            case CustomerStatus.Banned:
                customer.Deactivate(); // For now, treat banned as inactive
                break;
            default:
                throw new InvalidOperationException($"Invalid status transition to {request.NewStatus}");
        }

        var updatedCustomer = await _unitOfWork.Customers.UpdateAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CustomerDto>(updatedCustomer);
    }
} 