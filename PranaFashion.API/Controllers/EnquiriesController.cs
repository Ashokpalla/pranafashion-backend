using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PranaFashion.Core.DTOs;
using PranaFashion.Core.Models;
using PranaFashion.Infrastructure.Data;
using PranaFashion.API.Services;

namespace PranaFashion.API.Controllers;

[ApiController]
[Route("api/enquiries")]
public class EnquiriesController(AppDbContext db, IEmailService email) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EnquiryRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Phone))
            return BadRequest(new { message = "Name and phone are required." });

        var enquiry = new Enquiry
        {
            Name     = req.Name.Trim(),
            Phone    = req.Phone.Trim(),
            Email    = req.Email?.Trim(),
            Category = req.Category?.Trim(),
            Message  = req.Message.Trim()
        };
        db.Enquiries.Add(enquiry);
        await db.SaveChangesAsync();

        // Notify contact@pranafashionstudio.com
        _ = email.SendEnquiryNotificationAsync(
            enquiry.Name,
            enquiry.Phone,
            enquiry.Email ?? "not provided",
            enquiry.Message
        );

        return Ok(new { message = "Enquiry received. We will contact you shortly!" });
    }

    [HttpGet, Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAll([FromQuery] bool? unresolved)
    {
        var query = db.Enquiries.AsQueryable();
        if (unresolved == true) query = query.Where(e => !e.IsResolved);
        var list = await query.OrderByDescending(e => e.CreatedAt).ToListAsync();
        return Ok(list);
    }

    [HttpPatch("{id:int}/resolve"), Authorize(Roles = "admin")]
    public async Task<IActionResult> Resolve(int id)
    {
        var e = await db.Enquiries.FindAsync(id);
        if (e is null) return NotFound();
        e.IsResolved = true;
        await db.SaveChangesAsync();
        return Ok(new { message = "Marked as resolved." });
    }
}
