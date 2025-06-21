using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace BookStore.Application.Features.Books.Queries;

public class GetAllBooksQuery : IRequest<IEnumerable<BookDto>>
{
}

public class GetAllBooksQueryHandler : IRequestHandler<GetAllBooksQuery, IEnumerable<BookDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllBooksQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BookDto>> Handle(GetAllBooksQuery request, CancellationToken cancellationToken)
    {
        var books = await _unitOfWork.Books.GetAllAsync();
        return _mapper.Map<IEnumerable<BookDto>>(books);
    }
} 