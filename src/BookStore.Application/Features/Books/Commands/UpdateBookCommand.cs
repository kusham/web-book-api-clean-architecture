using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using AutoMapper;
using MediatR;
using FluentValidation;

namespace BookStore.Application.Features.Books.Commands;

public class UpdateBookCommand : IRequest<BookDto>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public BookCategory Category { get; set; }
    public string Publisher { get; set; } = string.Empty;
    public int Pages { get; set; }
}

public class UpdateBookCommandValidator : AbstractValidator<UpdateBookCommand>
{
    public UpdateBookCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Book ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required")
            .MaximumLength(100).WithMessage("Author cannot exceed 100 characters");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be non-negative");

        RuleFor(x => x.Pages)
            .GreaterThan(0).WithMessage("Pages must be positive");

        RuleFor(x => x.Publisher)
            .NotEmpty().WithMessage("Publisher is required")
            .MaximumLength(100).WithMessage("Publisher cannot exceed 100 characters");
    }
}

public class UpdateBookCommandHandler : IRequestHandler<UpdateBookCommand, BookDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateBookCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BookDto> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _unitOfWork.Books.GetByIdAsync(request.Id);
        if (book == null)
            throw new InvalidOperationException($"Book with ID {request.Id} not found");

        book.UpdateDetails(
            request.Title,
            request.Author,
            request.Description,
            request.Category,
            request.Publisher,
            request.Pages
        );

        book.UpdatePrice(request.Price);
        book.UpdateStock(request.StockQuantity);

        var updatedBook = await _unitOfWork.Books.UpdateAsync(book);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<BookDto>(updatedBook);
    }
} 