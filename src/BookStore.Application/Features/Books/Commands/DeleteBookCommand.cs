using BookStore.Application.Interfaces;
using MediatR;
using FluentValidation;

namespace BookStore.Application.Features.Books.Commands;

public class DeleteBookCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteBookCommandValidator : AbstractValidator<DeleteBookCommand>
{
    public DeleteBookCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Book ID is required");
    }
}

public class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBookCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _unitOfWork.Books.GetByIdAsync(request.Id);
        if (book == null)
            return false;

        await _unitOfWork.Books.DeleteAsync(request.Id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
} 