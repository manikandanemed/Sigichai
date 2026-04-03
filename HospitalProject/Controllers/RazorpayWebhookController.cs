//using System.Security.Cryptography;
//using System.Text;
//using System.Text.Json;
//using HospitalProject.Data;
//using HospitalProject.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace HospitalProject.Controllers
//{
//    [ApiController]
//    [Route("api/webhook/razorpay")]
//    public class RazorpayWebhookController : ControllerBase
//    {
//        private readonly ApplicationDbContext _db;
//        private readonly ITwilioService _twilio;
//        private readonly IConfiguration _config;

//        public RazorpayWebhookController(
//            ApplicationDbContext db,
//            ITwilioService twilio,
//            IConfiguration config)
//        {
//            _db = db;
//            _twilio = twilio;
//            _config = config;
//        }

//        [HttpPost]
//        public async Task<IActionResult> Handle()
//        {
//            // 1️⃣ Read Body
//            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

//            // 2️⃣ Get Razorpay Signature Header
//            var signature = Request.Headers["x-razorpay-signature"].ToString();

//            // 3️⃣ Verify Signature
//            if (!VerifySignature(json, signature))
//                return BadRequest("Invalid Signature");

//            var document = JsonDocument.Parse(json);
//            var root = document.RootElement;

//            string eventType = root.GetProperty("event").GetString();

//            // ============================================
//            // ✅ PAYMENT SUCCESS
//            // ============================================
//            if (eventType == "payment.captured")
//            {
//                var paymentEntity = root
//                    .GetProperty("payload")
//                    .GetProperty("payment")
//                    .GetProperty("entity");

//                string orderId = paymentEntity.GetProperty("order_id").GetString();
//                string paymentId = paymentEntity.GetProperty("id").GetString();

//                var appointment = await _db.Appointments
//                    .Include(a => a.Patient)
//                        .ThenInclude(p => p.User)
//                    .Include(a => a.FamilyMember)
//                    .FirstOrDefaultAsync(a => a.RazorpayOrderId == orderId);

//                if (appointment != null && appointment.Status == "PaymentPending")
//                {
//                    appointment.TempToken = "TK-" + Guid.NewGuid().ToString("N")[..6].ToUpper();
//                    appointment.Status = "Booked";
//                    appointment.PaymentStatus = "Success";
//                    appointment.RazorpayPaymentId = paymentId;

//                    var log = await _db.PaymentLogs
//                        .FirstOrDefaultAsync(x => x.RazorpayOrderId == orderId);

//                    if (log != null)
//                    {
//                        log.Status = "Captured";
//                        log.RazorpayPaymentId = paymentId;
//                        log.RawResponse = json;
//                    }

//                    await _db.SaveChangesAsync();

//                    // Send SMS only for success
//                    await _twilio.SendOtpAsync(
//                        appointment.Patient.User.MobileNumber,
//                        $"Payment Successful! Your Token: {appointment.TempToken}");
//                }
//            }

//            // ============================================
//            // ❌ PAYMENT FAILED
//            // ============================================
//            if (eventType == "payment.failed")
//            {
//                var paymentEntity = root
//                    .GetProperty("payload")
//                    .GetProperty("payment")
//                    .GetProperty("entity");

//                string orderId = paymentEntity.GetProperty("order_id").GetString();
//                string paymentId = paymentEntity.GetProperty("id").GetString();

//                string errorReason = "";

//                if (paymentEntity.TryGetProperty("error_description", out var errorProp))
//                {
//                    errorReason = errorProp.GetString();
//                }

//                var appointment = await _db.Appointments
//                    .FirstOrDefaultAsync(a => a.RazorpayOrderId == orderId);

//                if (appointment != null && appointment.Status == "PaymentPending")
//                {
//                    appointment.Status = "PaymentFailed";
//                    appointment.PaymentStatus = "Failed";
//                    appointment.RazorpayPaymentId = paymentId;

//                    var log = await _db.PaymentLogs
//                        .FirstOrDefaultAsync(x => x.RazorpayOrderId == orderId);

//                    if (log != null)
//                    {
//                        log.Status = "Failed";
//                        log.RazorpayPaymentId = paymentId;
//                        log.RawResponse = json;
//                        log.FailureReason = errorReason;   // 🔥 IMPORTANT
//                    }

//                    await _db.SaveChangesAsync();
//                }
//            }

//            return Ok();
//        }


//        // 🔐 Manual Razorpay Signature Verification
//        private bool VerifySignature(string payload, string razorpaySignature)
//        {
//            var secret = _config["Razorpay:WebhookSecret"];

//            var encoding = new UTF8Encoding();
//            byte[] keyBytes = encoding.GetBytes(secret);
//            byte[] messageBytes = encoding.GetBytes(payload);

//            using var hmac = new HMACSHA256(keyBytes);
//            byte[] hashBytes = hmac.ComputeHash(messageBytes);

//            var generatedSignature = BitConverter
//                .ToString(hashBytes)
//                .Replace("-", "")
//                .ToLower();

//            return generatedSignature == razorpaySignature;
//        }
//    }
//}


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
        private readonly IWebHostEnvironment _env;

        public RazorpayWebhookController(
            ApplicationDbContext db,
            ITwilioService twilio,
            IConfiguration config,
            IWebHostEnvironment env)
        {
            _db = db;
            _twilio = twilio;
            _config = config;
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            string json = "";
            string signature = "";
            string eventType = "";

            try
            {
                // 1️⃣ Read Razorpay payload
                json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

                // 2️⃣ Read Razorpay signature header
                signature = Request.Headers["x-razorpay-signature"].ToString();

                // 3️⃣ Log incoming webhook
                await WriteLog($"WEBHOOK HIT\nPayload: {json}");

                // 4️⃣ Verify Signature
                if (!VerifySignature(json, signature))
                {
                    await WriteLog("INVALID SIGNATURE");
                    return BadRequest("Invalid Signature");
                }

                // 5️⃣ Parse JSON
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                eventType = root.GetProperty("event").GetString() ?? "";

                await WriteLog($"EVENT TYPE: {eventType}");

                // ============================================
                // ✅ PAYMENT SUCCESS
                // ============================================
                if (eventType == "payment.captured")
                {
                    var paymentEntity = root
                        .GetProperty("payload")
                        .GetProperty("payment")
                        .GetProperty("entity");

                    string orderId = paymentEntity.GetProperty("order_id").GetString() ?? "";
                    string paymentId = paymentEntity.GetProperty("id").GetString() ?? "";

                    await WriteLog($"PAYMENT CAPTURED | OrderId: {orderId} | PaymentId: {paymentId}");

                    var appointment = await _db.Appointments
                        .Include(a => a.Patient)
                            .ThenInclude(p => p.User)
                        .Include(a => a.FamilyMember)
                        .FirstOrDefaultAsync(a => a.RazorpayOrderId == orderId);

                    if (appointment == null)
                    {
                        await WriteLog($"APPOINTMENT NOT FOUND | OrderId: {orderId}");
                        return Ok("Appointment not found");
                    }

                    // Already processed safe check
                    if (appointment.PaymentStatus == "Success")
                    {
                        await WriteLog($"ALREADY PROCESSED | OrderId: {orderId}");
                        return Ok("Already processed");
                    }

                    // Update appointment
                    appointment.TempToken = "TK-" + Guid.NewGuid().ToString("N")[..6].ToUpper();
                    appointment.Status = "Booked";
                    appointment.PaymentStatus = "Success";
                    appointment.RazorpayPaymentId = paymentId;

                    // Update payment log if exists
                    var log = await _db.PaymentLogs
                        .FirstOrDefaultAsync(x => x.RazorpayOrderId == orderId);

                    if (log != null)
                    {
                        log.Status = "Captured";
                        log.RazorpayPaymentId = paymentId;
                        log.RawResponse = json;
                        log.FailureReason = null;
                    }

                    await _db.SaveChangesAsync();

                    await WriteLog($"DB UPDATED SUCCESSFULLY | TempToken: {appointment.TempToken}");

                    // Send SMS only if mobile number exists
                    if (!string.IsNullOrWhiteSpace(appointment?.Patient?.User?.MobileNumber))
                    {
                        await _twilio.SendOtpAsync(
                            appointment.Patient.User.MobileNumber,
                            $"Payment Successful! Your Token: {appointment.TempToken}");

                        await WriteLog($"SMS SENT TO: {appointment.Patient.User.MobileNumber}");
                    }
                    else
                    {
                        await WriteLog("SMS NOT SENT - Mobile number missing");
                    }
                }

                // ============================================
                // ❌ PAYMENT FAILED
                // ============================================
                else if (eventType == "payment.failed")
                {
                    var paymentEntity = root
                        .GetProperty("payload")
                        .GetProperty("payment")
                        .GetProperty("entity");

                    string orderId = paymentEntity.GetProperty("order_id").GetString() ?? "";
                    string paymentId = paymentEntity.GetProperty("id").GetString() ?? "";

                    string errorReason = paymentEntity.TryGetProperty("error_description", out var errorProp)
                        ? errorProp.GetString() ?? "Unknown error"
                        : "Unknown error";

                    await WriteLog($"PAYMENT FAILED | OrderId: {orderId} | PaymentId: {paymentId} | Reason: {errorReason}");

                    var appointment = await _db.Appointments
                        .FirstOrDefaultAsync(a => a.RazorpayOrderId == orderId);

                    if (appointment != null && appointment.PaymentStatus != "Success")
                    {
                        appointment.Status = "PaymentFailed";
                        appointment.PaymentStatus = "Failed";
                        appointment.RazorpayPaymentId = paymentId;
                    }

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

                    await WriteLog($"DB UPDATED FOR FAILED PAYMENT | OrderId: {orderId}");
                }

                // ============================================
                // ⚠️ OTHER EVENTS
                // ============================================
                else
                {
                    await WriteLog($"UNHANDLED EVENT RECEIVED: {eventType}");
                }

                return Ok("Webhook processed");
            }
            catch (Exception ex)
            {
                await WriteLog($"ERROR: {ex.Message}\nSTACKTRACE: {ex.StackTrace}");
                return StatusCode(500, "Webhook Error");
            }
        }

        // 🔐 Razorpay Signature Verification
        private bool VerifySignature(string payload, string razorpaySignature)
        {
            var secret = _config["Razorpay:WebhookSecret"];

            if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(razorpaySignature))
                return false;

            var encoding = new UTF8Encoding();
            byte[] keyBytes = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(payload);

            using var hmac = new HMACSHA256(keyBytes);
            byte[] hashBytes = hmac.ComputeHash(messageBytes);

            var generatedSignature = BitConverter
                .ToString(hashBytes)
                .Replace("-", "")
                .ToLower();

            return generatedSignature == razorpaySignature.ToLower();
        }

        // 📝 Write logs into wwwroot/razorpay-log.txt
        private async Task WriteLog(string message)
        {
            try
            {
                var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var logPath = Path.Combine(rootPath, "razorpay-log.txt");

                if (!Directory.Exists(rootPath))
                    Directory.CreateDirectory(rootPath);

                await System.IO.File.AppendAllTextAsync(
                    logPath,
                    $"\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n"
                );
            }
            catch
            {
                // Ignore logging failures
            }
        }
    }
}
