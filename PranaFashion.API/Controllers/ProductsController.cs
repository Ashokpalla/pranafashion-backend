using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PranaFashion.Core.DTOs;
using PranaFashion.Core.Models;
using PranaFashion.Infrastructure.Data;

namespace PranaFashion.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? category,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool? inStock,
        [FromQuery] string? search,
        [FromQuery] string sortBy = "newest",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var query = db.Products.AsQueryable();

        if (!string.IsNullOrEmpty(category))  query = query.Where(p => p.Category == category);
        if (minPrice.HasValue)                 query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue && maxPrice > 0) query = query.Where(p => p.Price <= maxPrice.Value);
        if (inStock.HasValue && inStock.Value) query = query.Where(p => p.InStock);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));

        query = sortBy switch
        {
            "price_asc"  => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "rating"     => query.OrderByDescending(p => p.Rating),
            _            => query.OrderByDescending(p => p.CreatedAt) // newest
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new PagedResult<ProductDto>(
            items.Select(ToDto).ToList(),
            total, page, pageSize,
            (int)Math.Ceiling(total / (double)pageSize)
        ));
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured()
    {
        var items = await db.Products
            .Where(p => p.IsFeatured && p.InStock)
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)
            .ToListAsync();
        return Ok(items.Select(ToDto));
    }

    [HttpGet("category/{cat}")]
    public async Task<IActionResult> GetByCategory(string cat)
    {
        var items = await db.Products.Where(p => p.Category == cat && p.InStock).ToListAsync();
        return Ok(items.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await db.Products.FindAsync(id);
        return p is null ? NotFound() : Ok(ToDto(p));
    }

    [HttpPost, Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest req)
    {
        var p = new Product
        {
            Name         = req.Name,          Description  = req.Description,
            Price        = req.Price,         OriginalPrice= req.OriginalPrice,
            Category     = req.Category,      SubCategory  = req.SubCategory,
            Images       = req.Images ?? new(),
            Badge        = req.Badge,         InStock      = req.InStock,
            StockCount   = req.StockCount,    Sizes        = req.Sizes ?? new(),
            Colors       = req.Colors ?? new(), Fabric     = req.Fabric,
            IsFeatured   = req.IsFeatured
        };
        db.Products.Add(p);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = p.Id }, ToDto(p));
    }

    [HttpPut("{id:int}"), Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest req)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return NotFound();

        if (req.Name        is not null) p.Name         = req.Name;
        if (req.Description is not null) p.Description  = req.Description;
        if (req.Price       is not null) p.Price         = req.Price.Value;
        if (req.OriginalPrice.HasValue)  p.OriginalPrice = req.OriginalPrice;
        if (req.Category    is not null) p.Category     = req.Category;
        if (req.SubCategory is not null) p.SubCategory  = req.SubCategory;
        if (req.Images      is not null) p.Images        = req.Images;
        if (req.Badge       is not null) p.Badge         = req.Badge;
        if (req.InStock     is not null) p.InStock       = req.InStock.Value;
        if (req.StockCount  is not null) p.StockCount    = req.StockCount.Value;
        if (req.Sizes       is not null) p.Sizes         = req.Sizes;
        if (req.Colors      is not null) p.Colors        = req.Colors;
        if (req.Fabric      is not null) p.Fabric        = req.Fabric;
        if (req.IsFeatured  is not null) p.IsFeatured    = req.IsFeatured.Value;
        p.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(ToDto(p));
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return NotFound();
        db.Products.Remove(p);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static ProductDto ToDto(Product p) => new(
        p.Id, p.Name, p.Description, p.Price, p.OriginalPrice,
        p.Category, p.SubCategory, p.Images, p.Badge,
        p.InStock, p.StockCount, p.Sizes, p.Colors, p.Fabric,
        p.Rating, p.ReviewCount, p.CreatedAt
    );
}
