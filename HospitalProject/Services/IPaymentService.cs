using HospitalProject.Data;
using HospitalProject.Models;
using HospitalProject.Repositories;
using Microsoft.EntityFrameworkCore;
using Razorpay.Api;

namespace HospitalProject.Services
{
    public record RefundResult(
        bool Success,
        string Status,
        string? FailureReason,
        string? RazorpayResponse
    );

    public interface IPaymentService
    {
        Task<string> CreateOrder(decimal amount, string receiptId);
        Task<RefundResult> RefundPayment(string paymentId, decimal amount, int appointmentId);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _config;
        private readonly IRepository<RefundLog> _refundLog;

        public PaymentService(
            IConfiguration config,
            IRepository<RefundLog> refundLog)
        {
            _config = config;
            _refundLog = refundLog;
        }

        public async Task<string> CreateOrder(decimal amount, string receiptId)
        {
            var client = new RazorpayClient(
                _config["Razorpay:Key"],
                _config["Razorpay:Secret"]);

            Dictionary<string, object> options = new()
        {
            { "amount",          (int)(amount * 100) },
            { "currency",        "INR"               },
            { "receipt",         receiptId           },
            { "payment_capture", 1                   }
        };

            Order order = client.Order.Create(options);
            return order["id"].ToString();
        }

        public async Task<RefundResult> RefundPayment(
            string paymentId, decimal amount, int appointmentId)
        {
            // ✅ Double refund protection
            var alreadyRefunded = await _refundLog.Query()
                .AnyAsync(r => r.AppointmentId == appointmentId
                            && r.Status == "Success");

            if (alreadyRefunded)
            {
                var dupLog = new RefundLog
                {
                    AppointmentId = appointmentId,
                    RazorpayPaymentId = paymentId,
                    RefundAmount = amount,
                    Status = "AlreadyRefunded",
                    FailureReason = "Refund already processed for this appointment.",
                    InitiatedAt = DateTime.Now
                };
                await _refundLog.AddAsync(dupLog);
                await _refundLog.SaveAsync();

                return new RefundResult(
                    false,
                    "AlreadyRefunded",
                    "Refund already processed for this appointment.",
                    null);
            }

            // Initiated log
            var log = new RefundLog
            {
                AppointmentId = appointmentId,
                RazorpayPaymentId = paymentId,
                RefundAmount = amount,
                Status = "Initiated",
                InitiatedAt = DateTime.Now
            };
            await _refundLog.AddAsync(log);
            await _refundLog.SaveAsync();

            try
            {
                var client = new RazorpayClient(
                    _config["Razorpay:Key"],
                    _config["Razorpay:Secret"]);

                Dictionary<string, object> options = new()
            {
                { "amount", (int)(amount * 100) }
            };

                var payment = client.Payment.Fetch(paymentId);
                var refund = payment.Refund(options);
                string rawResp = refund.Attributes.ToString();

                log.Status = "Success";
                log.RazorpayResponse = rawResp;
                log.CompletedAt = DateTime.Now;
                await _refundLog.SaveAsync();

                return new RefundResult(true, "Success", null, rawResp);
            }
            catch (Exception ex)
            {
                log.Status = "Failed";
                log.FailureReason = ex.Message;
                log.CompletedAt = DateTime.Now;
                await _refundLog.SaveAsync();

                return new RefundResult(false, "Failed", ex.Message, null);
            }
        }
    }
}