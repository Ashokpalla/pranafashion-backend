using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PranaFashion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Enquiries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enquiries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    OriginalPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: false),
                    SubCategory = table.Column<string>(type: "text", nullable: false),
                    Images = table.Column<string>(type: "text", nullable: false),
                    Badge = table.Column<string>(type: "text", nullable: true),
                    InStock = table.Column<bool>(type: "boolean", nullable: false),
                    StockCount = table.Column<int>(type: "integer", nullable: false),
                    Sizes = table.Column<string>(type: "text", nullable: false),
                    Colors = table.Column<string>(type: "text", nullable: false),
                    Fabric = table.Column<string>(type: "text", nullable: true),
                    Rating = table.Column<double>(type: "double precision", nullable: false),
                    ReviewCount = table.Column<int>(type: "integer", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    AddressPhone = table.Column<string>(type: "text", nullable: false),
                    AddressLine1 = table.Column<string>(type: "text", nullable: false),
                    AddressLine2 = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    Pincode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    PaymentStatus = table.Column<string>(type: "text", nullable: false),
                    PaymentMethod = table.Column<string>(type: "text", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<string>(type: "text", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ProductName = table.Column<string>(type: "text", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Size = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Badge", "Category", "Colors", "CreatedAt", "Description", "Fabric", "Images", "InStock", "IsFeatured", "Name", "OriginalPrice", "Price", "Rating", "ReviewCount", "Sizes", "StockCount", "SubCategory", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "New Arrival", "women", "[]", new DateTime(2026, 3, 29, 18, 22, 34, 609, DateTimeKind.Utc).AddTicks(4136), "", null, "[\"women1.jpg\"]", true, true, "Kanjivaram Silk Saree", 5800m, 4500m, 4.7999999999999998, 34, "[]", 12, "Sarees", new DateTime(2026, 3, 29, 18, 22, 34, 609, DateTimeKind.Utc).AddTicks(4140) },
                    { 2, "Bestseller", "women", "[]", new DateTime(2026, 3, 29, 18, 22, 34, 609, DateTimeKind.Utc).AddTicks(4156), "", null, "[\"women2.jpg\"]", true, true, "Banarasi Georgette Saree", null, 3200m, 4.9000000000000004, 51, "[]", 8, "Wedding Collection", new DateTime(2026, 3, 29, 18, 22, 34, 609, DateTimeKind.Utc).AddTicks(4157) },
                    { 3, "Sale", "women", "[]", new DateTime(2026, 3, 29, 18, 22, 34, 609, DateTimeKind.Utc).AddTicks(4164), "", null, "[\"women3.jpg\"]", true, true, "Cotton Salwar Suit Set", 2200m, 1800m, 4.5, 28, "[]", 20, "Salwar Kameez", new DateTime(2026, 3, 29, 18, 22, 34, 609, DateTimeKind.Utc).AddTicks(4165) },
                    { 4, "New Arrival", "men", "[]", new DateTime(2026, 3, 29, 18, 22, 34, 609, DateTimeKind.Utc).AddTicks(4168), "", null, "[\"men1.jpg\"]", true, true, "Embroidered Sherwani", null, 6500m, 4.7000000000000002, 19, "[]", 6, "Festive Collection", new DateTime(2026, 3, 29, 18, 22, 34, 609, DateTimeKind.Utc).AddTicks(4168) },
                    { 5, "Sale", "men", "[]", new DateTime(2026, 3, 29, 18, 22, 34, 609, DateTimeKind.Utc).AddTicks(4171), "", null, "[\"men2.jpg\"]", true, true, "Cotton Kurta Set", 1500m, 1200m, 4.4000000000000004, 42, "[]", 25, "Everyday Wear", new DateTime(2026, 3, 29, 18, 22, 34, 609, DateTimeKind.Utc).AddTicks(4171) },
                    { 6, "Trending", "western", "[]", new DateTime(2026, 3, 29, 18, 22, 34, 609, DateTimeKind.Utc).AddTicks(4175), "", null, "[\"western1.jpg\"]", true, true, "Floral Maxi Dress", null, 2100m, 4.5999999999999996, 23, "[]", 15, "Western Collection", new DateTime(2026, 3, 29, 18, 22, 34, 609, DateTimeKind.Utc).AddTicks(4175) },
                    { 7, "Hot Pick", "western", "[]", new DateTime(2026, 3, 29, 18, 22, 34, 609, DateTimeKind.Utc).AddTicks(4179), "", null, "[\"western2.jpg\"]", true, false, "Co-ord Set", 3200m, 2800m, 4.5, 16, "[]", 10, "Fusion Wear", new DateTime(2026, 3, 29, 18, 22, 34, 609, DateTimeKind.Utc).AddTicks(4179) },
                    { 8, "New Arrival", "kids", "[]", new DateTime(2026, 3, 29, 18, 22, 34, 609, DateTimeKind.Utc).AddTicks(4182), "", null, "[\"kids1.jpg\"]", true, true, "Pattu Pavadai Set", null, 1400m, 4.7999999999999998, 31, "[]", 18, "Girls Collection", new DateTime(2026, 3, 29, 18, 22, 34, 609, DateTimeKind.Utc).AddTicks(4183) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                table: "PasswordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enquiries");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
