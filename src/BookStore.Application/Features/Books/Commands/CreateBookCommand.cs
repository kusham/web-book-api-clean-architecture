using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using AutoMapper;
using MediatR;
using FluentValidation;

namespace BookStore.Application.Features.Books.Commands;

public class CreateBookCommand : IRequest<BookDto>
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public BookCategory Category { get; set; }
    public DateTime PublishedDate { get; set; }
    public string Publisher { get; set; } = string.Empty;
    public int Pages { get; set; }
}

public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required")
            .MaximumLength(100).WithMessage("Author cannot exceed 100 characters");

        RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("ISBN is required")
            .MaximumLength(13).WithMessage("ISBN cannot exceed 13 characters");

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

public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, BookDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateBookCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BookDto> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        var book = new Book(
            request.Title,
            request.Author,
            request.ISBN,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.Category,
            request.PublishedDate,
            request.Publisher,
            request.Pages
        );

        var createdBook = await _unitOfWork.Books.AddAsync(book);
        await _unitOfWork.SaveChangesAsync();
        
        return _mapper.Map<BookDto>(createdBook);
    }
} 