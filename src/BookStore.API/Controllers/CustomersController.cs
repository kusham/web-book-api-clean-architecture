using BookStore.Application.Features.Customers.Commands;
using BookStore.Application.Features.Customers.Queries;
using BookStore.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _mediator.Send(new GetAllCustomersQuery());
        return Ok(customers);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var customer = await _mediator.Send(new GetCustomerByIdQuery { Id = id });
        if (customer == null) return NotFound();
        return Ok(customer);
    }

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var customer = await _mediator.Send(new GetCustomerByEmailQuery { Email = email });
        if (customer == null) return NotFound();
        return Ok(customer);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
    {
        var customer = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        
        var customer = await _mediator.Send(command);
        return Ok(customer);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateCustomerStatusCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        
        var customer = await _mediator.Send(command);
        return Ok(customer);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteCustomerCommand { Id = id });
        if (!result) return NotFound();
        return NoContent();
    }
} 