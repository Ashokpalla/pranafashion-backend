namespace PranaFashion.Core.DTOs;

// ── Auth DTOs ─────────────────────────────────────────────
public record LoginRequest(string Email, string Password);

public record RegisterRequest(
    string Name,
    string Email,
    string Phone,
    string Password,
    string ConfirmPassword
);

public record AuthResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record UserDto(int Id, string Name, string Email, string Phone, string Role, DateTime CreatedAt);

// ── Product DTOs ──────────────────────────────────────────
public record ProductDto(
    int Id, string Name, string Description,
    decimal Price, decimal? OriginalPrice,
    string Category, string SubCategory,
    List<string> Images, string? Badge,
    bool InStock, int StockCount,
    List<string> Sizes, List<string> Colors,
    string? Fabric, double Rating, int ReviewCount,
    DateTime CreatedAt
);

public record CreateProductRequest(
    string Name, string Description,
    decimal Price, decimal? OriginalPrice,
    string Category, string SubCategory,
    List<string>? Images, string? Badge,
    bool InStock, int StockCount,
    List<string>? Sizes, List<string>? Colors,
    string? Fabric, bool IsFeatured
);

public record UpdateProductRequest(
    string? Name, string? Description,
    decimal? Price, decimal? OriginalPrice,
    string? Category, string? SubCategory,
    List<string>? Images, string? Badge,
    bool? InStock, int? StockCount,
    List<string>? Sizes, List<string>? Colors,
    string? Fabric, bool? IsFeatured
);

public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

// ── Order DTOs ────────────────────────────────────────────
public record PlaceOrderRequest(
    List<OrderItemRequest> Items,
    AddressRequest Address,
    string PaymentMethod  // "cod" | "online"
);

public record OrderItemRequest(
    int ProductId,
    int Quantity,
    string? Size,
    string? Color
);

public record AddressRequest(
    string FullName,
    string Phone,
    string Line1,
    string? Line2,
    string City,
    string State,
    string Pincode
);

public record OrderDto(
    string Id,
    List<OrderItemDto> Items,
    AddressRequest Address,
    string Status,
    string PaymentStatus,
    string PaymentMethod,
    decimal Subtotal,
    decimal Discount,
    decimal Total,
    DateTime CreatedAt
);

public record OrderItemDto(
    int ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    string? Size,
    string? Color
);

// ── Enquiry DTO ───────────────────────────────────────────
public record EnquiryRequest(
    string Name,
    string Phone,
    string? Email,
    string? Category,
    string Message
);

// ── Reset Password DTOs ───────────────────────────────────
public record ForgotPasswordRequest(string Email);
public record VerifyOtpRequest(string Email, string Otp);
public record ResetPasswordRequest(string Email, string Otp, string NewPassword, string ConfirmPassword);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
