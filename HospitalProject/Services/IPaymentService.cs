using Razorpay.Api;

namespace HospitalProject.Services
{
    public interface IPaymentService
    {
        Task<string> CreateOrder(decimal amount, string receiptId);
        Task<bool> RefundPayment(string paymentId, decimal amount);
    }


    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _config;
        public PaymentService(IConfiguration config) => _config = config;

        //public async Task<string> CreateOrder(decimal amount, string receiptId)
        //{
        //    var client = new RazorpayClient(_config["Razorpay:Key"], _config["Razorpay:Secret"]);
        //    Dictionary<string, object> options = new Dictionary<string, object> {
        //     { "amount", (int)(amount * 100) }, // Paise
        //     { "currency", "INR" },
        //     { "receipt", receiptId }
        // };
        //    Order order = client.Order.Create(options);
        //    return order["id"].ToString();
        //}


        public async Task<string> CreateOrder(decimal amount, string receiptId)
        {
            var client = new RazorpayClient(
                _config["Razorpay:Key"],
                _config["Razorpay:Secret"]);

            Dictionary<string, object> options = new Dictionary<string, object> {
        { "amount", (int)(amount * 100) },
        { "currency", "INR" },
        { "receipt", receiptId },
        { "payment_capture", 1 }  // ✅ ADD THIS — Auto capture enable
         };

            Order order = client.Order.Create(options);
            return order["id"].ToString();
        }
        public async Task<bool> RefundPayment(string paymentId, decimal amount)
        {
            try
            {
                var client = new RazorpayClient(_config["Razorpay:Key"], _config["Razorpay:Secret"]);
                Dictionary<string, object> options = new Dictionary<string, object> { { "amount", (int)(amount * 100) } };
                client.Payment.Fetch(paymentId).Refund(options);
                return true;
            }
            catch { return false; }
        }

    }
}
