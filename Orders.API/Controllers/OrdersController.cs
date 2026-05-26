using Microsoft.AspNetCore.Mvc;
using Orders.API.DTOs;
using Orders.API.Services;

namespace Orders.API.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _service;

    public OrdersController(OrderService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] Guid? usuarioId)
    {
        var orders = _service.GetAll(usuarioId);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        var order = _service.GetById(id);
        return Ok(order);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateOrderRequest request)
    {
        var order = _service.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id}/status")]
    public IActionResult UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = _service.UpdateStatus(id, request);
        return Ok(order);
    }
}