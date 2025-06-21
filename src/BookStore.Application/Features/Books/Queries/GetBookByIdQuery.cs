using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace BookStore.Application.Features.Books.Queries;

public class GetBookByIdQuery : IRequest<BookDto?>
{
    public Guid Id { get; set; }
}

public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, BookDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetBookByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BookDto?> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        var book = await _unitOfWork.Books.GetByIdAsync(request.Id);
        return _mapper.Map<BookDto>(book);
    }
} 