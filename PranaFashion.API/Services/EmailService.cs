using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PranaFashion.API.Services;

public interface IEmailService
{
    Task SendPasswordResetOtpAsync(string toEmail, string toName, string otp);
    Task SendWelcomeEmailAsync(string toEmail, string toName);
    Task SendOrderConfirmationAsync(string toEmail, string toName, string orderId, decimal total);
    Task SendOrderStatusUpdateAsync(string toEmail, string toName, string orderId, string status);
    Task SendEnquiryNotificationAsync(string fromName, string fromPhone, string fromEmail, string message);
}

public class EmailService(IConfiguration config, ILogger<EmailService> logger) : IEmailService
{
    // ── SMTP credentials per mailbox ──────────────────────
    private string SmtpHost     => config["Email:SmtpHost"]        ?? "mail.privateemail.com";
    private int    SmtpPort     => int.Parse(config["Email:SmtpPort"] ?? "465");
    private string DisplayName  => config["Email:DisplayName"]      ?? "Prana Fashion Studio";

    private string NoReplyEmail    => config["Email:NoReply"]           ?? "";
    private string NoReplyPassword => config["Email:NoReplyPassword"]   ?? "";
    private string OrdersEmail     => config["Email:Orders"]            ?? "";
    private string OrdersPassword  => config["Email:OrdersPassword"]    ?? "";
    private string ContactEmail    => config["Email:Contact"]           ?? "";

    // ── OTP / Password Reset  →  no-reply@ ───────────────
    public async Task SendPasswordResetOtpAsync(string toEmail, string toName, string otp)
    {
        var subject = "Reset Your Password — Prana Fashion Studio";
        var body = $@"<!DOCTYPE html>
<html>
<body style=""font-family:Arial,sans-serif;background:#FAF6EE;margin:0;padding:40px 0;"">
<div style=""max-width:520px;margin:0 auto;background:#FDF9F2;border:1px solid #E2D5B8;"">

  <!-- Header -->
  <div style=""background:#1A1208;padding:28px 36px;"">
    <div style=""font-family:Georgia,serif;font-size:26px;letter-spacing:4px;color:#C9A84C;"">PRANA</div>
    <div style=""font-size:9px;letter-spacing:4px;color:rgba(250,246,238,0.45);text-transform:uppercase;margin-top:2px;"">Fashion Studio · Kakinada</div>
  </div>

  <!-- Body -->
  <div style=""padding:36px;"">
    <h2 style=""font-family:Georgia,serif;font-weight:300;font-size:26px;color:#1A1208;margin:0 0 14px;"">Reset Your Password</h2>
    <p style=""color:#4A3728;font-size:14px;line-height:1.8;margin:0 0 28px;"">
      Hello <strong>{toName}</strong>,<br><br>
      We received a request to reset the password for your Prana account.
      Use the OTP below — it expires in <strong>15 minutes</strong>.
    </p>

    <!-- OTP Box -->
    <div style=""background:#F5EDD6;border:1px solid #E2D5B8;padding:28px;text-align:center;margin:0 0 28px;"">
      <div style=""font-size:10px;letter-spacing:4px;text-transform:uppercase;color:#8B7355;margin-bottom:12px;"">Your One-Time Password</div>
      <div style=""font-family:Georgia,serif;font-size:52px;font-weight:300;letter-spacing:16px;color:#1A1208;line-height:1;"">{otp}</div>
      <div style=""font-size:11px;color:#8B7355;margin-top:12px;"">Valid for 15 minutes only</div>
    </div>

    <p style=""color:#8B7355;font-size:12px;line-height:1.7;"">
      If you did not request a password reset, please ignore this email.
      Your password will remain unchanged.
    </p>
  </div>

  <!-- Footer -->
  <div style=""background:#F5EDD6;padding:16px 36px;border-top:1px solid #E2D5B8;"">
    <p style=""color:#8B7355;font-size:11px;margin:0;"">
      © 2026 Prana Fashion Studio · D.No: 69-3-19/2, Rajendra Nagar, Kakinada-2<br>
      📞 8019304566 | 9492704566 · <a href=""mailto:contact@pranafashionstudio.com"" style=""color:#C9A84C;"">contact@pranafashionstudio.com</a>
    </p>
  </div>
</div>
</body>
</html>";
        await SendAsync(NoReplyEmail, NoReplyPassword, toEmail, toName, subject, body);
    }

    // ── Welcome Email  →  no-reply@ ──────────────────────
    public async Task SendWelcomeEmailAsync(string toEmail, string toName)
    {
        var subject = $"Welcome to Prana Fashion Studio, {toName}!";
        var body = $@"<!DOCTYPE html>
<html>
<body style=""font-family:Arial,sans-serif;background:#FAF6EE;margin:0;padding:40px 0;"">
<div style=""max-width:520px;margin:0 auto;background:#FDF9F2;border:1px solid #E2D5B8;"">

  <div style=""background:#1A1208;padding:28px 36px;"">
    <div style=""font-family:Georgia,serif;font-size:26px;letter-spacing:4px;color:#C9A84C;"">PRANA</div>
    <div style=""font-size:9px;letter-spacing:4px;color:rgba(250,246,238,0.45);text-transform:uppercase;margin-top:2px;"">Fashion Studio · Kakinada</div>
  </div>

  <div style=""padding:36px;"">
    <h2 style=""font-family:Georgia,serif;font-weight:300;font-size:28px;color:#1A1208;margin:0 0 14px;"">Welcome, {toName}! 🪷</h2>
    <p style=""color:#4A3728;font-size:14px;line-height:1.8;margin:0 0 20px;"">
      Thank you for joining <strong>Prana Fashion Studio</strong>. 
      We're delighted to have you as part of our family.
    </p>
    <p style=""color:#4A3728;font-size:14px;line-height:1.8;margin:0 0 28px;"">
      Explore our curated collections — Women's Ethnic, Men's Wear, Western Wear, and Kids Fashion — 
      all crafted with elegance and tradition in mind.
    </p>

    <div style=""text-align:center;margin:28px 0;"">
      <a href=""http://pranafashionstudio.com/products""
         style=""background:#1A1208;color:#E8C97A;font-size:11px;letter-spacing:3px;text-transform:uppercase;padding:14px 36px;text-decoration:none;display:inline-block;"">
        Browse Collections →
      </a>
    </div>

    <div style=""background:#F5EDD6;border:1px solid #E2D5B8;padding:20px;margin-top:8px;"">
      <div style=""font-size:10px;letter-spacing:3px;text-transform:uppercase;color:#C9A84C;margin-bottom:10px;"">Visit Us</div>
      <p style=""font-size:13px;color:#4A3728;margin:0;line-height:1.7;"">
        D.No: 69-3-19/2, Ground Floor, Rajendra Nagar<br>
        Kakinada – 533 002, Andhra Pradesh<br>
        📞 8019304566 &nbsp;|&nbsp; 9492704566
      </p>
    </div>
  </div>

  <div style=""background:#F5EDD6;padding:16px 36px;border-top:1px solid #E2D5B8;"">
    <p style=""color:#8B7355;font-size:11px;margin:0;"">
      © 2026 Prana Fashion Studio · <a href=""mailto:contact@pranafashionstudio.com"" style=""color:#C9A84C;"">contact@pranafashionstudio.com</a>
    </p>
  </div>
</div>
</body>
</html>";
        await SendAsync(NoReplyEmail, NoReplyPassword, toEmail, toName, subject, body);
    }

    // ── Order Confirmation  →  orders@ ───────────────────
    public async Task SendOrderConfirmationAsync(string toEmail, string toName, string orderId, decimal total)
    {
        var subject = $"Order Confirmed #{orderId} — Prana Fashion Studio";
        var body = $@"<!DOCTYPE html>
<html>
<body style=""font-family:Arial,sans-serif;background:#FAF6EE;margin:0;padding:40px 0;"">
<div style=""max-width:520px;margin:0 auto;background:#FDF9F2;border:1px solid #E2D5B8;"">

  <div style=""background:#1A1208;padding:28px 36px;"">
    <div style=""font-family:Georgia,serif;font-size:26px;letter-spacing:4px;color:#C9A84C;"">PRANA</div>
    <div style=""font-size:9px;letter-spacing:4px;color:rgba(250,246,238,0.45);text-transform:uppercase;margin-top:2px;"">Fashion Studio · Kakinada</div>
  </div>

  <div style=""padding:36px;"">
    <div style=""text-align:center;margin-bottom:24px;"">
      <div style=""width:52px;height:52px;background:#C9A84C;border-radius:50%;display:inline-flex;align-items:center;justify-content:center;font-size:22px;color:#1A1208;font-weight:700;"">✓</div>
    </div>
    <h2 style=""font-family:Georgia,serif;font-weight:300;font-size:26px;color:#1A1208;margin:0 0 8px;text-align:center;"">Order Confirmed!</h2>
    <p style=""text-align:center;color:#8B7355;font-size:13px;margin:0 0 28px;"">Thank you, {toName}. We've received your order.</p>

    <div style=""background:#F5EDD6;border:1px solid #E2D5B8;padding:20px;margin-bottom:20px;"">
      <div style=""display:flex;justify-content:space-between;margin-bottom:8px;"">
        <span style=""font-size:11px;letter-spacing:2px;text-transform:uppercase;color:#8B7355;"">Order ID</span>
        <span style=""font-size:14px;font-weight:500;color:#1A1208;"">#{orderId}</span>
      </div>
      <div style=""display:flex;justify-content:space-between;"">
        <span style=""font-size:11px;letter-spacing:2px;text-transform:uppercase;color:#8B7355;"">Total</span>
        <span style=""font-size:16px;font-weight:500;color:#C9A84C;"">₹{total:N0}</span>
      </div>
    </div>

    <p style=""color:#4A3728;font-size:13px;line-height:1.7;"">
      Our team will process your order shortly. 
      You will receive an update when your order is shipped.
      For any queries, reply to this email or call us.
    </p>

    <div style=""text-align:center;margin:24px 0 0;"">
      <a href=""http://pranafashionstudio.com/account/orders""
         style=""background:#1A1208;color:#E8C97A;font-size:11px;letter-spacing:3px;text-transform:uppercase;padding:12px 28px;text-decoration:none;display:inline-block;"">
        Track My Order →
      </a>
    </div>
  </div>

  <div style=""background:#F5EDD6;padding:16px 36px;border-top:1px solid #E2D5B8;"">
    <p style=""color:#8B7355;font-size:11px;margin:0;"">
      © 2026 Prana Fashion Studio · 
      <a href=""mailto:orders@pranafashionstudio.com"" style=""color:#C9A84C;"">orders@pranafashionstudio.com</a> · 
      📞 8019304566
    </p>
  </div>
</div>
</body>
</html>";
        await SendAsync(OrdersEmail, OrdersPassword, toEmail, toName, subject, body);
    }

    // ── Order Status Update  →  orders@ ──────────────────
    public async Task SendOrderStatusUpdateAsync(string toEmail, string toName, string orderId, string status)
    {
        var (emoji, headline, message) = status.ToLower() switch
        {
            "confirmed"  => ("✅", "Order Confirmed",   "Your order has been confirmed and is being prepared."),
            "processing" => ("📦", "Being Packed",      "Your order is being carefully packed and will be dispatched soon."),
            "shipped"    => ("🚚", "On Its Way!",       "Your order has been shipped and is on its way to you."),
            "delivered"  => ("🎉", "Delivered!",        "Your order has been delivered. We hope you love it! Please share your feedback."),
            "cancelled"  => ("❌", "Order Cancelled",   "Your order has been cancelled. If you paid online, a refund will be processed within 5-7 days."),
            _            => ("📋", "Order Update",      $"Your order status has been updated to: {status}.")
        };

        var subject = $"Order #{orderId} — {headline} | Prana Fashion Studio";
        var body = $@"<!DOCTYPE html>
<html>
<body style=""font-family:Arial,sans-serif;background:#FAF6EE;margin:0;padding:40px 0;"">
<div style=""max-width:520px;margin:0 auto;background:#FDF9F2;border:1px solid #E2D5B8;"">
  <div style=""background:#1A1208;padding:28px 36px;"">
    <div style=""font-family:Georgia,serif;font-size:26px;letter-spacing:4px;color:#C9A84C;"">PRANA</div>
    <div style=""font-size:9px;letter-spacing:4px;color:rgba(250,246,238,0.45);text-transform:uppercase;margin-top:2px;"">Fashion Studio · Kakinada</div>
  </div>
  <div style=""padding:36px;"">
    <div style=""text-align:center;font-size:40px;margin-bottom:16px;"">{emoji}</div>
    <h2 style=""font-family:Georgia,serif;font-weight:300;font-size:26px;color:#1A1208;margin:0 0 8px;text-align:center;"">{headline}</h2>
    <p style=""text-align:center;color:#8B7355;font-size:12px;letter-spacing:1px;margin:0 0 24px;"">Order #{orderId}</p>
    <p style=""color:#4A3728;font-size:14px;line-height:1.8;margin:0 0 24px;"">Hello {toName},<br><br>{message}</p>
    <div style=""text-align:center;"">
      <a href=""http://pranafashionstudio.com/account/orders""
         style=""background:#1A1208;color:#E8C97A;font-size:11px;letter-spacing:3px;text-transform:uppercase;padding:12px 28px;text-decoration:none;display:inline-block;"">
        View Order →
      </a>
    </div>
  </div>
  <div style=""background:#F5EDD6;padding:16px 36px;border-top:1px solid #E2D5B8;"">
    <p style=""color:#8B7355;font-size:11px;margin:0;"">
      © 2026 Prana Fashion Studio · <a href=""mailto:orders@pranafashionstudio.com"" style=""color:#C9A84C;"">orders@pranafashionstudio.com</a>
    </p>
  </div>
</div>
</body>
</html>";
        await SendAsync(OrdersEmail, OrdersPassword, toEmail, toName, subject, body);
    }

    // ── Enquiry Notification  →  contact@ inbox ───────────
    public async Task SendEnquiryNotificationAsync(string fromName, string fromPhone, string fromEmail, string message)
    {
        var subject = $"New Enquiry from {fromName} — Prana Fashion Studio";
        var body = $@"<!DOCTYPE html>
<html>
<body style=""font-family:Arial,sans-serif;background:#FAF6EE;margin:0;padding:40px 0;"">
<div style=""max-width:520px;margin:0 auto;background:#FDF9F2;border:1px solid #E2D5B8;"">
  <div style=""background:#1A1208;padding:24px 32px;"">
    <div style=""font-family:Georgia,serif;font-size:22px;letter-spacing:4px;color:#C9A84C;"">PRANA</div>
    <div style=""font-size:9px;letter-spacing:3px;color:rgba(250,246,238,0.4);text-transform:uppercase;"">New Website Enquiry</div>
  </div>
  <div style=""padding:32px;"">
    <div style=""background:#F5EDD6;border:1px solid #E2D5B8;padding:20px;margin-bottom:20px;"">
      <table style=""width:100%;font-size:13px;"">
        <tr><td style=""color:#8B7355;padding:5px 0;width:80px;"">Name</td><td style=""color:#1A1208;font-weight:500;"">{fromName}</td></tr>
        <tr><td style=""color:#8B7355;padding:5px 0;"">Phone</td><td style=""color:#1A1208;""><a href=""tel:{fromPhone}"" style=""color:#C9A84C;"">{fromPhone}</a></td></tr>
        <tr><td style=""color:#8B7355;padding:5px 0;"">Email</td><td style=""color:#1A1208;""><a href=""mailto:{fromEmail}"" style=""color:#C9A84C;"">{fromEmail}</a></td></tr>
      </table>
    </div>
    <div style=""font-size:11px;letter-spacing:2px;text-transform:uppercase;color:#8B7355;margin-bottom:10px;"">Message</div>
    <div style=""font-size:14px;color:#4A3728;line-height:1.8;background:#FAF6EE;padding:16px;border-left:3px solid #C9A84C;"">{message}</div>
    <div style=""margin-top:24px;"">
      <a href=""tel:{fromPhone}"" style=""background:#1A1208;color:#E8C97A;font-size:11px;letter-spacing:2px;text-transform:uppercase;padding:12px 24px;text-decoration:none;display:inline-block;margin-right:10px;"">
        📞 Call Back
      </a>
      <a href=""mailto:{fromEmail}"" style=""background:transparent;color:#1A1208;font-size:11px;letter-spacing:2px;text-transform:uppercase;padding:11px 24px;text-decoration:none;display:inline-block;border:1px solid #1A1208;"">
        ✉ Reply by Email
      </a>
    </div>
  </div>
</div>
</body>
</html>";
        // Send notification to the contact inbox itself
        await SendAsync(ContactEmail, NoReplyPassword, ContactEmail, DisplayName, subject, body);
    }

    // ── Core send method ─────────────────────────────────
    private async Task SendAsync(
        string fromEmail, string fromPassword,
        string toEmail,   string toName,
        string subject,   string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(fromEmail) || string.IsNullOrWhiteSpace(fromPassword))
        {
            logger.LogWarning(
                "[EMAIL SKIPPED] No credentials for sender. Subject: '{Subject}' | To: {To}",
                subject, toEmail);
            return;
        }

        try
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(DisplayName, fromEmail));
            msg.To.Add(new MailboxAddress(toName, toEmail));
            msg.Subject  = subject;
            msg.Priority = MessagePriority.Normal;

            // Deliverability headers — reduces spam score
            msg.Headers.Add("X-Mailer",        "PranaFashion/1.0");
            msg.Headers.Add("Reply-To",         ContactEmail.Length > 0 ? ContactEmail : fromEmail);
            msg.Headers.Add("X-Entity-Ref-ID",  Guid.NewGuid().ToString());

            // Build multipart body: plain text fallback + HTML
            var plain = new TextPart("plain")
            {
                Text = $"Hello {toName},\n\n{subject}\n\nVisit: https://pranafashionstudio.com\n\nPrana Fashion Studio\nD.No: 69-3-19/2, Rajendra Nagar, Kakinada\n8019304566 | 9492704566"
            };
            var html = new TextPart("html") { Text = htmlBody };

            msg.Body = new Multipart("alternative") { plain, html };

            using var client = new SmtpClient();
            // Disable certificate validation issues on some hosts
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(SmtpHost, SmtpPort, SecureSocketOptions.Auto);
            await client.AuthenticateAsync(fromEmail, fromPassword);
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);

            logger.LogInformation("[EMAIL SENT] '{Subject}' → {To}", subject, toEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EMAIL FAILED] Subject: '{Subject}' | To: {To}", subject, toEmail);
        }
    }
}
