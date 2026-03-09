using HospitalProject.Models;
using HospitalProject.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalProject.Services
{
    public class PharmacyQuotationService
    {
        private readonly IRepository<PharmacyQuotation> _quotation;
        private readonly IRepository<PharmacyQuotationItem> _quotationItem;
        private readonly IRepository<PrescriptionRoute> _route;
        private readonly IRepository<PharmacyNotification> _notification;
        private readonly IRepository<Patient> _patient;

        public PharmacyQuotationService(
            IRepository<PharmacyQuotation> quotation,
            IRepository<PharmacyQuotationItem> quotationItem,
            IRepository<PrescriptionRoute> route,
            IRepository<PharmacyNotification> notification,
            IRepository<Patient> patient)
        {
            _quotation = quotation;
            _quotationItem = quotationItem;
            _route = route;
            _notification = notification;
            _patient = patient;
        }

        // =====================================================================
        // 1️⃣ EXTERNAL PHARMACY — Submit Quotation
        // =====================================================================
        public async Task<string> SubmitQuotation(
            int externalPharmacyId, SubmitQuotationDto dto)
        {
            // Route valid-ஆ check பண்றோம்
            var route = await _route.Query()
                .Include(r => r.Prescription)
                .FirstOrDefaultAsync(r =>
                    r.Id == dto.PrescriptionRouteId &&
                    r.ExternalPharmacyId == externalPharmacyId &&
                    r.Status == "Pending")
                ?? throw new Exception("Route not found or already processed");

            // Already quoted check
            var alreadyQuoted = await _quotation.Query()
                .AnyAsync(q =>
                    q.PrescriptionRouteId == dto.PrescriptionRouteId &&
                    q.ExternalPharmacyId == externalPharmacyId);

            if (alreadyQuoted)
                throw new Exception("Quotation already submitted for this prescription");

            // Calculate total
            decimal total = dto.Items
                .Where(i => i.IsAvailable)
                .Sum(i => i.PricePerUnit * i.QuantityAvailable);

            if (dto.OffersDelivery && dto.DeliveryCharge.HasValue)
                total += dto.DeliveryCharge.Value;

            // Create quotation
            var quotation = new PharmacyQuotation
            {
                PrescriptionRouteId = dto.PrescriptionRouteId,
                ExternalPharmacyId = externalPharmacyId,
                TotalAmount = total,
                OffersDelivery = dto.OffersDelivery,
                DeliveryCharge = dto.DeliveryCharge,
                Notes = dto.Notes,
                Status = "Pending",
                QuotedAt = DateTime.UtcNow
            };

            foreach (var item in dto.Items)
            {
                quotation.Items.Add(new PharmacyQuotationItem
                {
                    MedicineId = item.MedicineId,
                    IsAvailable = item.IsAvailable,
                    PricePerUnit = item.PricePerUnit,
                    QuantityAvailable = item.QuantityAvailable
                });
            }

            await _quotation.AddAsync(quotation);

            // Route status → Accepted
            route.Status = "Accepted";
            route.RespondedAt = DateTime.UtcNow;

            // Patient-க்கு notification
            await _notification.AddAsync(new PharmacyNotification
            {
                PatientId = route.Prescription.PatientId,
                Message = $"New price quote received from pharmacy. Check and select your preferred option.",
                CreatedAt = DateTime.UtcNow
            });

            await _quotation.SaveAsync();
            await _route.SaveAsync();
            await _notification.SaveAsync();

            return "Quotation submitted successfully";
        }

        // =====================================================================
        // 2️⃣ PATIENT — View All Quotations for a Prescription
        // =====================================================================
        public async Task<List<QuotationViewDto>> GetQuotations(int prescriptionId)
        {
            return await _quotation.Query()
                .Include(q => q.ExternalPharmacy)
                .Include(q => q.Items).ThenInclude(i => i.Medicine)
                .Where(q => q.PrescriptionRoute.PrescriptionId == prescriptionId)
                .OrderBy(q => q.TotalAmount)
                .Select(q => new QuotationViewDto(
                    q.Id,
                    q.PrescriptionRouteId,
                    q.ExternalPharmacy.PharmacyName,
                    q.ExternalPharmacy.Address,
                    q.OffersDelivery,
                    q.DeliveryCharge,
                    q.TotalAmount,
                    q.Notes,
                    q.Status,
                    q.QuotedAt,
                    q.Items.Select(i => new QuotationItemViewDto(
                        i.MedicineId,
                        i.Medicine.GenericName,
                        i.Medicine.BrandName,
                        i.IsAvailable,
                        i.PricePerUnit,
                        i.QuantityAvailable
                    )).ToList()
                ))
                .ToListAsync();
        }

        // =====================================================================
        // 3️⃣ PATIENT — Select Quotation
        // =====================================================================
        public async Task<string> SelectQuotation(
            int patientId, SelectQuotationDto dto)
        {
            // Selected quotation
            var selected = await _quotation.Query()
                .Include(q => q.PrescriptionRoute)
                    .ThenInclude(r => r.Prescription)
                .Include(q => q.ExternalPharmacy)
                .FirstOrDefaultAsync(q => q.Id == dto.QuotationId)
                ?? throw new Exception("Quotation not found");

            // Verify patient owns this prescription
            if (selected.PrescriptionRoute.Prescription.PatientId != patientId)
                throw new Exception("Access denied");

            var prescriptionId = selected.PrescriptionRoute.PrescriptionId;

            // Mark selected
            selected.Status = "Selected";

            // Mark all other quotations as Rejected
            var otherQuotations = await _quotation.Query()
                .Include(q => q.PrescriptionRoute)
                .Where(q =>
                    q.PrescriptionRoute.PrescriptionId == prescriptionId &&
                    q.Id != dto.QuotationId &&
                    q.Status == "Pending")
                .ToListAsync();

            foreach (var other in otherQuotations)
            {
                other.Status = "Rejected";

                // Notify other pharmacies
                await _notification.AddAsync(new PharmacyNotification
                {
                    PatientId = patientId,
                    Message = $"Prescription has been filled by another pharmacy.",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Cancel pending routes for other pharmacies
            var otherRoutes = await _route.Query()
                .Where(r =>
                    r.PrescriptionId == prescriptionId &&
                    r.Id != selected.PrescriptionRouteId &&
                    r.Status == "Pending")
                .ToListAsync();

            foreach (var route in otherRoutes)
            {
                route.Status = "Cancelled";
                route.RespondedAt = DateTime.UtcNow;
            }

            await _quotation.SaveAsync();
            await _route.SaveAsync();
            await _notification.SaveAsync();

            return $"Pharmacy '{selected.ExternalPharmacy.PharmacyName}' selected successfully";
        }

        // =====================================================================
        // 4️⃣ EXTERNAL PHARMACY — View Pending Prescriptions to Quote
        // =====================================================================
        public async Task<List<PrescriptionRouteViewDto>> GetPendingRoutes(
            int externalPharmacyId)
        {
            return await _route.Query()
                .Include(r => r.Prescription)
                .Include(r => r.ExternalPharmacy)
                .Where(r =>
                    r.ExternalPharmacyId == externalPharmacyId &&
                    r.Status == "Pending" &&
                    r.PharmacyType == "External")
                .OrderBy(r => r.RoutedAt)
                .Select(r => new PrescriptionRouteViewDto(
                    r.Id,
                    r.PrescriptionId,
                    r.PharmacyType,
                    r.ExternalPharmacy!.PharmacyName,
                    r.Status,
                    r.RoutedAt,
                    r.RespondedAt
                ))
                .ToListAsync();
        }
    }
}