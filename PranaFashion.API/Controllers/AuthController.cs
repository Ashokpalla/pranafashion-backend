using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PranaFashion.Core.DTOs;
using PranaFashion.Core.Models;
using PranaFashion.Infrastructure.Data;
using PranaFashion.API.Services;

namespace PranaFashion.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    AppDbContext db,
    IConfiguration config,
    IEmailService email,
    ILogger<AuthController> logger) : ControllerBase
{
    // ── REGISTER ─────────────────────────────────────────
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (req.Password != req.ConfirmPassword)
            return BadRequest(new { message = "Passwords do not match." });

        if (await db.Users.AnyAsync(u => u.Email == req.Email.ToLower()))
            return Conflict(new { message = "This email is already registered. Please sign in." });

        var user = new User
        {
            Name         = req.Name.Trim(),
            Email        = req.Email.ToLower().Trim(),
            Phone        = req.Phone.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role         = "customer"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Build auth response first so we can return immediately
        var authResponse = BuildAuthResponse(user);

        // Send welcome email — awaited properly so it doesn't get GC'd
        // Use Task.Run so it doesn't block the HTTP response
        var userName  = user.Name;
        var userEmail = user.Email;
        _ = Task.Run(async () =>
        {
            try { await email.SendWelcomeEmailAsync(userEmail, userName); }
            catch (Exception ex) { logger.LogError(ex, "Welcome email failed for {Email}", userEmail); }
        });

        return Ok(authResponse);
    }

    // ── LOGIN ─────────────────────────────────────────────
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLower().Trim());
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(BuildAuthResponse(user));
    }

    // ── REFRESH TOKEN ─────────────────────────────────────
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.RefreshToken == req.Token && u.RefreshTokenExpiry > DateTime.UtcNow);
        if (user is null)
            return Unauthorized(new { message = "Session expired. Please sign in again." });

        return Ok(BuildAuthResponse(user));
    }

    // ── FORGOT PASSWORD — sends OTP ───────────────────────
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLower().Trim());

        // Always return same message to prevent email enumeration
        if (user is null)
            return Ok(new { message = "If this email is registered, an OTP has been sent." });

        // Invalidate any existing active tokens
        var existing = await db.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed)
            .ToListAsync();
        existing.ForEach(t => t.IsUsed = true);

        // Generate 6-digit OTP
        var otp     = new Random().Next(100000, 999999).ToString();
        var otpHash = BCrypt.Net.BCrypt.HashPassword(otp);

        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId    = user.Id,
            Token     = otp,
            TokenHash = otpHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed    = false
        });
        await db.SaveChangesAsync();

        // Always log OTP for dev visibility
        logger.LogInformation("OTP for {Email}: {Otp}", user.Email, otp);

        // Send OTP email — awaited so it completes reliably
        await email.SendPasswordResetOtpAsync(user.Email, user.Name, otp);

        return Ok(new { message = "If this email is registered, an OTP has been sent." });
    }

    // ── VERIFY OTP ────────────────────────────────────────
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLower().Trim());
        if (user is null) return BadRequest(new { message = "Invalid request." });

        var record = await db.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        if (record is null || !BCrypt.Net.BCrypt.Verify(req.Otp, record.TokenHash))
            return BadRequest(new { message = "Invalid or expired OTP." });

        return Ok(new { message = "OTP verified.", valid = true });
    }

    // ── RESET PASSWORD ────────────────────────────────────
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        if (req.NewPassword != req.ConfirmPassword)
            return BadRequest(new { message = "Passwords do not match." });
        if (req.NewPassword.Length < 8)
            return BadRequest(new { message = "Password must be at least 8 characters." });

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLower().Trim());
        if (user is null) return BadRequest(new { message = "Invalid request." });

        var record = await db.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        if (record is null || !BCrypt.Net.BCrypt.Verify(req.Otp, record.TokenHash))
            return BadRequest(new { message = "Invalid or expired OTP. Please request a new one." });

        user.PasswordHash       = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        user.RefreshToken       = null;
        user.RefreshTokenExpiry = null;
        record.IsUsed           = true;
        await db.SaveChangesAsync();

        return Ok(new { message = "Password reset successfully. You can now sign in." });
    }

    // ── CHANGE PASSWORD (logged-in) ───────────────────────
    [HttpPost("change-password"), Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        if (req.NewPassword != req.ConfirmPassword)
            return BadRequest(new { message = "New passwords do not match." });
        if (req.NewPassword.Length < 8)
            return BadRequest(new { message = "Password must be at least 8 characters." });

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user   = await db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
            return BadRequest(new { message = "Current password is incorrect." });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        await db.SaveChangesAsync();
        return Ok(new { message = "Password changed successfully." });
    }

    // ── Helpers ───────────────────────────────────────────
    private AuthResponse BuildAuthResponse(User user)
    {
        var token        = GenerateJwt(user);
        var refreshToken = Guid.NewGuid().ToString("N");
        user.RefreshToken       = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        db.SaveChanges();
        return new AuthResponse(
            Token:        token,
            RefreshToken: refreshToken,
            ExpiresAt:    DateTime.UtcNow.AddHours(8),
            User: new UserDto(user.Id, user.Name, user.Email, user.Phone, user.Role, user.CreatedAt)
        );
    }

    private string GenerateJwt(User user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Role,           user.Role),
            new Claim(ClaimTypes.Name,           user.Name)
        };
        var token = new JwtSecurityToken(
            issuer:             config["Jwt:Issuer"],
            audience:           config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record RefreshRequest(string Token);
