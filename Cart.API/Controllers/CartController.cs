using Microsoft.AspNetCore.Mvc;
using Cart.API.DTOs;
using Cart.API.Services;

namespace Cart.API.Controllers;

[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly CartService _service;

    public CartController(CartService service)
    {
        _service = service;
    }

    [HttpGet("{userId}")]
    public IActionResult GetCart(Guid userId)
    {
        var cart = _service.GetByUserId(userId);
        return Ok(cart);
    }

    [HttpPost("{userId}/items")]
    public IActionResult AddItem(Guid userId, [FromBody] AddItemRequest request)
    {
        var cart = _service.AddItem(userId, request);
        return Ok(cart);
    }

    [HttpPut("{userId}/items/{productId}")]
    public IActionResult UpdateItem(Guid userId, Guid productId, [FromBody] UpdateItemRequest request)
    {
        var cart = _service.UpdateItem(userId, productId, request);
        return Ok(cart);
    }

    [HttpDelete("{userId}/items/{productId}")]
    public IActionResult RemoveItem(Guid userId, Guid productId)
    {
        _service.RemoveItem(userId, productId);
        return NoContent();
    }

    [HttpDelete("{userId}")]
    public IActionResult ClearCart(Guid userId)
    {
        _service.ClearCart(userId);
        return NoContent();
    }
}