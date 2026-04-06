using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HospitalProject.Data;
using HospitalProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalProject.Controllers
{
    [ApiController]
    [Route("api/webhook/razorpay")]
    public class RazorpayWebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ITwilioService _twilio;
        private readonly IConfiguration _config;
        private readonly ILogger<RazorpayWebhookController> _logger;
        private readonly IMsg91Service _msg91;



        public RazorpayWebhookController(
            ApplicationDbContext db,
            IMsg91Service msg91,     // ✅ Twilio → MSG91
            ITwilioService twilio,
            IConfiguration config,
            ILogger<RazorpayWebhookController> logger)
        {
            _db = db;
            _twilio = twilio;
            _msg91 = msg91;
            _config = config;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            _logger.LogInformation("🔔 Webhook received");

            var signature = Request.Headers["x-razorpay-signature"].ToString();

            if (!VerifySignature(json, signature))
            {
                _logger.LogWarning("❌ Webhook signature verification failed");
                return BadRequest("Invalid Signature");
            }

            _logger.LogInformation("✅ Webhook signature verified");

            var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            string eventType = root.GetProperty("event").GetString();

            _logger.LogInformation("📣 Webhook event: {EventType}", eventType);

            if (eventType == "payment.captured")
            {
                var paymentEntity = root
                    .GetProperty("payload")
                    .GetProperty("payment")
                    .GetProperty("entity");

                string orderId = paymentEntity.GetProperty("order_id").GetString();
                string paymentId = paymentEntity.GetProperty("id").GetString();

                _logger.LogInformation("💳 Payment captured — OrderId: {OrderId}, PaymentId: {PaymentId}",
                    orderId, paymentId);

                // ✅ Doctor + Hospital include add 
                var appointment = await _db.Appointments
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.User)
                    .Include(a => a.FamilyMember)
                    .Include(a => a.Doctor)        // ✅ Add
                        .ThenInclude(d => d.User)  // ✅ Add
                    .FirstOrDefaultAsync(a => a.RazorpayOrderId == orderId);

                if (appointment == null)
                {
                    _logger.LogWarning("❌ Appointment not found for OrderId: {OrderId}", orderId);
                    return Ok();
                }

                _logger.LogInformation("✅ Appointment found — Id: {AppointmentId}, Status: {Status}",
                    appointment.Id, appointment.Status);

                if (appointment.Status == "PaymentPending")
                {
                    appointment.TempToken = "TK-" + Guid.NewGuid().ToString("N")[..6].ToUpper();
                    appointment.Status = "Booked";
                    appointment.PaymentStatus = "Success";
                    appointment.RazorpayPaymentId = paymentId;

                    var log = await _db.PaymentLogs
                        .FirstOrDefaultAsync(x => x.RazorpayOrderId == orderId);

                    if (log != null)
                    {
                        log.Status = "Captured";
                        log.RazorpayPaymentId = paymentId;
                        log.RawResponse = json;
                    }

                    await _db.SaveChangesAsync();

                    _logger.LogInformation("✅ Appointment updated — TempToken: {TempToken}",
                        appointment.TempToken);

                    // ✅Changed
                    // ✅ TentativeTime UTC → IST convert
                    var istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

                    var tentativeDisplay = appointment.TentativeTime.HasValue
                        ? TimeZoneInfo.ConvertTimeFromUtc(
                            appointment.TentativeTime.Value, istZone)
                            .ToString("hh:mm tt")
                        : "Will be updated soon";

                    var dateDisplay = appointment.AppointmentDate
                        .ToString("yyyy-MM-dd");

                    // Doctor name
                    var doctorName = appointment.Doctor?.User?.Name ?? "Doctor";

                    // Hospital name — Appointment-ல் Hospital include பண்ணணும்
                    var hospitalName = "Hospital"; // default

                    // ✅ MSG91 send
                    await _msg91.SendAppointmentConfirmationAsync(
                        appointment.Patient.User.MobileNumber,
                        appointment.TempToken,
                        tentativeDisplay,
                        dateDisplay,
                        doctorName,
                        hospitalName);

                    _logger.LogInformation("✅ WhatsApp sent via MSG91 — Token: {Token}, Time: {Time}",
                        appointment.TempToken, tentativeDisplay);
                }
                else
                {
                    _logger.LogWarning("⚠️ Appointment status is not PaymentPending: {Status}",
                        appointment.Status);
                }
            }

            if (eventType == "payment.failed")
            {
                var paymentEntity = root
                    .GetProperty("payload")
                    .GetProperty("payment")
                    .GetProperty("entity");

                string orderId = paymentEntity.GetProperty("order_id").GetString();
                string paymentId = paymentEntity.GetProperty("id").GetString();

                string errorReason = "";
                if (paymentEntity.TryGetProperty("error_description", out var errorProp))
                    errorReason = errorProp.GetString();

                _logger.LogWarning("❌ Payment failed — OrderId: {OrderId}, Reason: {Reason}",
                    orderId, errorReason);

                var appointment = await _db.Appointments
                    .FirstOrDefaultAsync(a => a.RazorpayOrderId == orderId);

                if (appointment != null && appointment.Status == "PaymentPending")
                {
                    appointment.Status = "PaymentFailed";
                    appointment.PaymentStatus = "Failed";
                    appointment.RazorpayPaymentId = paymentId;

                    var log = await _db.PaymentLogs
                        .FirstOrDefaultAsync(x => x.RazorpayOrderId == orderId);

                    if (log != null)
                    {
                        log.Status = "Failed";
                        log.RazorpayPaymentId = paymentId;
                        log.RawResponse = json;
                        log.FailureReason = errorReason;
                    }

                    await _db.SaveChangesAsync();

                    _logger.LogInformation("✅ Payment failed status updated for OrderId: {OrderId}", orderId);
                }
            }

            return Ok();
        }

        private bool VerifySignature(string payload, string razorpaySignature)
        {
            var secret = _config["Razorpay:WebhookSecret"];
            var encoding = new UTF8Encoding();
            byte[] keyBytes = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(payload);

            using var hmac = new HMACSHA256(keyBytes);
            byte[] hashBytes = hmac.ComputeHash(messageBytes);

            var generatedSignature = BitConverter
                .ToString(hashBytes)
                .Replace("-", "")
                .ToLower();

            return generatedSignature == razorpaySignature;
        }
    }
}