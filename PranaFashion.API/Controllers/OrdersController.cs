using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using PranaFashion.Core.DTOs;
using PranaFashion.Core.Models;
using PranaFashion.Infrastructure.Data;
using PranaFashion.API.Services;

namespace PranaFashion.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(AppDbContext db, IEmailService email) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest req)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user   = await db.Users.FindAsync(userId);

        decimal subtotal = 0, discount = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in req.Items)
        {
            var product = await db.Products.FindAsync(item.ProductId);
            if (product is null)  return BadRequest(new { message = $"Product {item.ProductId} not found." });
            if (!product.InStock) return BadRequest(new { message = $"{product.Name} is out of stock." });
            if (product.StockCount < item.Quantity)
                return BadRequest(new { message = $"Only {product.StockCount} units of {product.Name} available." });

            subtotal += product.Price * item.Quantity;
            if (product.OriginalPrice.HasValue)
                discount += (product.OriginalPrice.Value - product.Price) * item.Quantity;

            orderItems.Add(new OrderItem
            {
                ProductId   = product.Id,
                Product     = product,
                ProductName = product.Name,
                UnitPrice   = product.Price,
                Quantity    = item.Quantity,
                Size        = item.Size,
                Color       = item.Color
            });

            product.StockCount -= item.Quantity;
            if (product.StockCount == 0) product.InStock = false;
        }

        var order = new Order
        {
            UserId        = userId,
            Items         = orderItems,
            FullName      = req.Address.FullName,
            AddressPhone  = req.Address.Phone,
            AddressLine1  = req.Address.Line1,
            AddressLine2  = req.Address.Line2,
            City          = req.Address.City,
            State         = req.Address.State,
            Pincode       = req.Address.Pincode,
            PaymentMethod = req.PaymentMethod,
            Subtotal      = subtotal,
            Discount      = discount,
            Total         = subtotal - discount
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        // Send order confirmation email (non-blocking)
        if (user is not null)
            _ = email.SendOrderConfirmationAsync(user.Email, user.Name, order.Id, order.Total);

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, ToDto(order));
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var orders = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return Ok(orders.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(string id)
    {
        var userId  = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole("admin");

        var order = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)                              return NotFound();
        if (!isAdmin && order.UserId != userId)         return Forbid();

        return Ok(ToDto(order));
    }

    [HttpPatch("{id}/status"), Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateStatusRequest req)
    {
        var order = await db.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();

        order.Status    = req.Status;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // Send status update email
        if (order.User is not null)
            _ = email.SendOrderStatusUpdateAsync(order.User.Email, order.User.Name, order.Id, order.Status);

        return Ok(new { order.Id, order.Status });
    }

    private static OrderDto ToDto(Order o) => new(
        o.Id,
        o.Items.Select(i => new OrderItemDto(
            i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.Size, i.Color
        )).ToList(),
        new AddressRequest(o.FullName, o.AddressPhone, o.AddressLine1, o.AddressLine2, o.City, o.State, o.Pincode),
        o.Status, o.PaymentStatus, o.PaymentMethod,
        o.Subtotal, o.Discount, o.Total, o.CreatedAt
    );
}

public record UpdateStatusRequest(string Status);
