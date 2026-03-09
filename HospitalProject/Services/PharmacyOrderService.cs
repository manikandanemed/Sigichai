using HospitalProject.Models;
using HospitalProject.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalProject.Services
{
    public class PharmacyOrderService
    {
        private readonly IRepository<PharmacyOrder> _order;
        private readonly IRepository<PharmacyOrderStatusLog> _statusLog;
        private readonly IRepository<PharmacyQuotation> _quotation;
        private readonly IRepository<PharmacyNotification> _notification;

        public PharmacyOrderService(
            IRepository<PharmacyOrder> order,
            IRepository<PharmacyOrderStatusLog> statusLog,
            IRepository<PharmacyQuotation> quotation,
            IRepository<PharmacyNotification> notification)
        {
            _order = order;
            _statusLog = statusLog;
            _quotation = quotation;
            _notification = notification;
        }

        // =====================================================================
        // 1️⃣ PATIENT — Place Order
        // =====================================================================
        public async Task<int> PlaceOrder(int patientId, PlaceOrderDto dto)
        {
            // Quotation selected-ஆ check பண்றோம்
            var quotation = await _quotation.Query()
                .Include(q => q.ExternalPharmacy)
                .FirstOrDefaultAsync(q =>
                    q.Id == dto.QuotationId &&
                    q.Status == "Selected")
                ?? throw new Exception("Quotation not found or not selected");

            // Already order இருக்கா check
            var existing = await _order.Query()
                .AnyAsync(o => o.QuotationId == dto.QuotationId);
            if (existing)
                throw new Exception("Order already placed for this quotation");

            // Delivery check
            if (dto.OrderType == "Delivery" && !quotation.OffersDelivery)
                throw new Exception("This pharmacy does not offer home delivery");

            if (dto.OrderType == "Delivery" &&
                string.IsNullOrWhiteSpace(dto.DeliveryAddress))
                throw new Exception("Delivery address is required");

            var order = new PharmacyOrder
            {
                QuotationId = dto.QuotationId,
                PatientId = patientId,
                OrderType = dto.OrderType,
                DeliveryAddress = dto.DeliveryAddress,
                PaymentMode = dto.PaymentMode,
                PaymentStatus = "Pending",
                Status = "Confirmed",
                TotalAmount = quotation.TotalAmount,
                DeliveryCharge = quotation.DeliveryCharge,
                CreatedAt = DateTime.UtcNow
            };

            // Initial status log
            order.StatusLogs.Add(new PharmacyOrderStatusLog
            {
                Status = "Confirmed",
                Remarks = "Order placed by patient",
                UpdatedAt = DateTime.UtcNow
            });

            await _order.AddAsync(order);

            // Notify patient
            await _notification.AddAsync(new PharmacyNotification
            {
                PatientId = patientId,
                Message = $"Your order has been confirmed with {quotation.ExternalPharmacy.PharmacyName}.",
                CreatedAt = DateTime.UtcNow
            });

            await _order.SaveAsync();
            await _notification.SaveAsync();

            return order.Id;
        }

        // =====================================================================
        // 2️⃣ EXTERNAL PHARMACY — Update Order Status
        // =====================================================================
        public async Task<string> UpdateStatus(
            int externalPharmacyId, UpdateOrderStatusDto dto)
        {
            var order = await _order.Query()
                .Include(o => o.Quotation)
                    .ThenInclude(q => q.ExternalPharmacy)
                .FirstOrDefaultAsync(o =>
                    o.Id == dto.OrderId &&
                    o.Quotation.ExternalPharmacyId == externalPharmacyId)
                ?? throw new Exception("Order not found");

            // Valid status transition check
            var validStatuses = new[]
            {
                "Preparing", "ReadyForPickup", "OutForDelivery",
                "Delivered", "Collected"
            };

            if (!validStatuses.Contains(dto.Status))
                throw new Exception("Invalid status");

            // Delivery type check
            if (dto.Status == "OutForDelivery" && order.OrderType != "Delivery")
                throw new Exception("Cannot set OutForDelivery for Pickup orders");

            if (dto.Status == "ReadyForPickup" && order.OrderType != "Pickup")
                throw new Exception("Cannot set ReadyForPickup for Delivery orders");

            // Update status
            order.Status = dto.Status;

            if (dto.Status == "Delivered" || dto.Status == "Collected")
                order.DeliveredAt = DateTime.UtcNow;

            // Status log add
            order.StatusLogs.Add(new PharmacyOrderStatusLog
            {
                Status = dto.Status,
                Remarks = dto.Remarks,
                UpdatedAt = DateTime.UtcNow
            });

            // Patient notification
            var message = dto.Status switch
            {
                "Preparing" => "Your medicines are being prepared.",
                "ReadyForPickup" => "Your medicines are ready for pickup!",
                "OutForDelivery" => "Your medicines are out for delivery!",
                "Delivered" => "Your medicines have been delivered!",
                "Collected" => "Your medicines have been collected.",
                _ => $"Order status updated to {dto.Status}"
            };

            await _notification.AddAsync(new PharmacyNotification
            {
                PatientId = order.PatientId,
                Message = message,
                CreatedAt = DateTime.UtcNow
            });

            await _order.SaveAsync();
            await _notification.SaveAsync();

            return $"Order status updated to {dto.Status}";
        }

        // =====================================================================
        // 3️⃣ PATIENT — View Order Details
        // =====================================================================
        public async Task<PharmacyOrderViewDto> GetOrder(int orderId, int patientId)
        {
            var order = await _order.Query()
                .Include(o => o.Quotation)
                    .ThenInclude(q => q.ExternalPharmacy)
                .Include(o => o.StatusLogs)
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId && o.PatientId == patientId)
                ?? throw new Exception("Order not found");

            return new PharmacyOrderViewDto(
                order.Id,
                order.QuotationId,
                order.Quotation.ExternalPharmacy.PharmacyName,
                order.OrderType,
                order.DeliveryAddress,
                order.PaymentMode,
                order.PaymentStatus,
                order.Status,
                order.TotalAmount,
                order.DeliveryCharge,
                order.CreatedAt,
                order.DeliveredAt,
                order.StatusLogs
                    .OrderBy(l => l.UpdatedAt)
                    .Select(l => new OrderStatusLogDto(
                        l.Status,
                        l.Remarks,
                        l.UpdatedAt
                    )).ToList()
            );
        }

        // =====================================================================
        // 4️⃣ EXTERNAL PHARMACY — View All Orders
        // =====================================================================
        public async Task<List<PharmacyOrderViewDto>> GetPharmacyOrders(
            int externalPharmacyId)
        {
            return await _order.Query()
                .Include(o => o.Quotation)
                    .ThenInclude(q => q.ExternalPharmacy)
                .Include(o => o.StatusLogs)
                .Where(o => o.Quotation.ExternalPharmacyId == externalPharmacyId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new PharmacyOrderViewDto(
                    o.Id,
                    o.QuotationId,
                    o.Quotation.ExternalPharmacy.PharmacyName,
                    o.OrderType,
                    o.DeliveryAddress,
                    o.PaymentMode,
                    o.PaymentStatus,
                    o.Status,
                    o.TotalAmount,
                    o.DeliveryCharge,
                    o.CreatedAt,
                    o.DeliveredAt,
                    o.StatusLogs
                        .OrderBy(l => l.UpdatedAt)
                        .Select(l => new OrderStatusLogDto(
                            l.Status,
                            l.Remarks,
                            l.UpdatedAt
                        )).ToList()
                ))
                .ToListAsync();
        }

        // =====================================================================
        // 5️⃣ PATIENT — Mark Payment Done (Direct to Pharmacy)
        // =====================================================================
        public async Task<string> MarkPaymentDone(int orderId, int patientId)
        {
            var order = await _order.Query()
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId && o.PatientId == patientId)
                ?? throw new Exception("Order not found");

            if (order.PaymentMode != "DirectToPharmacy")
                throw new Exception("This order uses app payment");

            order.PaymentStatus = "Paid";
            await _order.SaveAsync();

            return "Payment marked as done";
        }
    }
}