using BookStore.Application.Features.Orders.Commands;
using BookStore.Application.Features.Orders.Queries;
using BookStore.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BookStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _mediator.Send(new GetAllOrdersQuery());
        return Ok(orders);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery { Id = id });
        return order != null ? Ok(order) : NotFound();
    }

    [HttpGet("customer/{customerId}")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
    {
        var orders = await _mediator.Send(new GetOrdersByCustomerQuery { CustomerId = customerId });
        return Ok(orders);
    }

    [HttpGet("status/{status}")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetByStatus(int status)
    {
        var orderStatus = (Domain.Enums.OrderStatus)status;
        var orders = await _mediator.Send(new GetOrdersByStatusQuery { Status = orderStatus });
        return Ok(orders);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
    {
        var order = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPost("{id}/items")]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddOrderItemCommand command)
    {
        if (id != command.OrderId) return BadRequest("ID mismatch");
        
        var order = await _mediator.Send(command);
        return Ok(order);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusCommand command)
    {
        if (id != command.OrderId) return BadRequest("ID mismatch");
        
        var order = await _mediator.Send(command);
        return Ok(order);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        
        var order = await _mediator.Send(command);
        return Ok(order);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteOrderCommand { Id = id });
        return NoContent();
    }
} 