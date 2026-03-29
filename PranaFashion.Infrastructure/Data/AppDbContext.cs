using Microsoft.EntityFrameworkCore;
using PranaFashion.Core.Models;
using System.Text.Json;

namespace PranaFashion.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product>            Products           { get; set; }
    public DbSet<User>               Users              { get; set; }
    public DbSet<Order>              Orders             { get; set; }
    public DbSet<OrderItem>          OrderItems         { get; set; }
    public DbSet<Enquiry>            Enquiries          { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Price).HasColumnType("decimal(10,2)");
            e.Property(p => p.OriginalPrice).HasColumnType("decimal(10,2)");
            e.Property(p => p.Images).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());
            e.Property(p => p.Sizes).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());
            e.Property(p => p.Colors).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());
        });

        b.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
        });

        b.Entity<PasswordResetToken>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Subtotal).HasColumnType("decimal(10,2)");
            e.Property(o => o.Discount).HasColumnType("decimal(10,2)");
            e.Property(o => o.Total).HasColumnType("decimal(10,2)");
            e.HasOne(o => o.User).WithMany(u => u.Orders).HasForeignKey(o => o.UserId);
            e.HasMany(o => o.Items).WithOne().HasForeignKey(i => i.OrderId);
        });

        b.Entity<OrderItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.UnitPrice).HasColumnType("decimal(10,2)");
        });

        b.Entity<Product>().HasData(
            new Product { Id=1, Name="Kanjivaram Silk Saree",    Category="women",   SubCategory="Sarees",            Price=4500, OriginalPrice=5800, Badge="New Arrival", InStock=true, StockCount=12, Rating=4.8, ReviewCount=34, IsFeatured=true,  Images=new(){"women1.jpg"} },
            new Product { Id=2, Name="Banarasi Georgette Saree", Category="women",   SubCategory="Wedding Collection", Price=3200, OriginalPrice=null, Badge="Bestseller",  InStock=true, StockCount=8,  Rating=4.9, ReviewCount=51, IsFeatured=true,  Images=new(){"women2.jpg"} },
            new Product { Id=3, Name="Cotton Salwar Suit Set",   Category="women",   SubCategory="Salwar Kameez",      Price=1800, OriginalPrice=2200, Badge="Sale",         InStock=true, StockCount=20, Rating=4.5, ReviewCount=28, IsFeatured=true,  Images=new(){"women3.jpg"} },
            new Product { Id=4, Name="Embroidered Sherwani",     Category="men",     SubCategory="Festive Collection", Price=6500, OriginalPrice=null, Badge="New Arrival",  InStock=true, StockCount=6,  Rating=4.7, ReviewCount=19, IsFeatured=true,  Images=new(){"men1.jpg"} },
            new Product { Id=5, Name="Cotton Kurta Set",         Category="men",     SubCategory="Everyday Wear",      Price=1200, OriginalPrice=1500, Badge="Sale",         InStock=true, StockCount=25, Rating=4.4, ReviewCount=42, IsFeatured=true,  Images=new(){"men2.jpg"} },
            new Product { Id=6, Name="Floral Maxi Dress",        Category="western", SubCategory="Western Collection", Price=2100, OriginalPrice=null, Badge="Trending",     InStock=true, StockCount=15, Rating=4.6, ReviewCount=23, IsFeatured=true,  Images=new(){"western1.jpg"} },
            new Product { Id=7, Name="Co-ord Set",               Category="western", SubCategory="Fusion Wear",        Price=2800, OriginalPrice=3200, Badge="Hot Pick",     InStock=true, StockCount=10, Rating=4.5, ReviewCount=16, IsFeatured=false, Images=new(){"western2.jpg"} },
            new Product { Id=8, Name="Pattu Pavadai Set",        Category="kids",    SubCategory="Girls Collection",   Price=1400, OriginalPrice=null, Badge="New Arrival",  InStock=true, StockCount=18, Rating=4.8, ReviewCount=31, IsFeatured=true,  Images=new(){"kids1.jpg"} }
        );
    }
}
