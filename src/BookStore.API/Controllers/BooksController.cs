using BookStore.Application.Features.Books.Commands;
using BookStore.Application.Features.Books.Queries;
using BookStore.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly IMediator _mediator;

    public BooksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetAllBooks()
    {
        var books = await _mediator.Send(new GetAllBooksQuery());
        return Ok(books);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetBookById(Guid id)
    {
        var book = await _mediator.Send(new GetBookByIdQuery { Id = id });
        if (book == null) return NotFound();
        return Ok(book);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookCommand command)
    {
        var book = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateBook(Guid id, [FromBody] UpdateBookCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        
        var book = await _mediator.Send(command);
        return Ok(book);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteBook(Guid id)
    {
        var result = await _mediator.Send(new DeleteBookCommand { Id = id });
        if (!result) return NotFound();
        return NoContent();
    }
} 