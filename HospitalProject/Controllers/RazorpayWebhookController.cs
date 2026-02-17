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

        public RazorpayWebhookController(
            ApplicationDbContext db,
            ITwilioService twilio,
            IConfiguration config)
        {
            _db = db;
            _twilio = twilio;
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            // 1️⃣ Read Body
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            // 2️⃣ Get Razorpay Signature Header
            var signature = Request.Headers["x-razorpay-signature"].ToString();

            // 3️⃣ Verify Signature
            if (!VerifySignature(json, signature))
                return BadRequest("Invalid Signature");

            var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            string eventType = root.GetProperty("event").GetString();

            // ============================================
            // ✅ PAYMENT SUCCESS
            // ============================================
            if (eventType == "payment.captured")
            {
                var paymentEntity = root
                    .GetProperty("payload")
                    .GetProperty("payment")
                    .GetProperty("entity");

                string orderId = paymentEntity.GetProperty("order_id").GetString();
                string paymentId = paymentEntity.GetProperty("id").GetString();

                var appointment = await _db.Appointments
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.User)
                    .Include(a => a.FamilyMember)
                    .FirstOrDefaultAsync(a => a.RazorpayOrderId == orderId);

                if (appointment != null && appointment.Status == "PaymentPending")
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

                    // Send SMS only for success
                    await _twilio.SendOtpAsync(
                        appointment.Patient.User.MobileNumber,
                        $"Payment Successful! Your Token: {appointment.TempToken}");
                }
            }

            // ============================================
            // ❌ PAYMENT FAILED
            // ============================================
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
                {
                    errorReason = errorProp.GetString();
                }

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
                        log.FailureReason = errorReason;   // 🔥 IMPORTANT
                    }

                    await _db.SaveChangesAsync();
                }
            }

            return Ok();
        }


        // 🔐 Manual Razorpay Signature Verification
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
