using HospitalProject.Models;
using HospitalProject.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalProject.Services
{
    public class PharmacyRatingService
    {
        private readonly IRepository<PharmacyRating> _rating;
        private readonly IRepository<PharmacyOrder> _order;
        private readonly IRepository<ExternalPharmacy> _pharmacy;

        public PharmacyRatingService(
            IRepository<PharmacyRating> rating,
            IRepository<PharmacyOrder> order,
            IRepository<ExternalPharmacy> pharmacy)
        {
            _rating = rating;
            _order = order;
            _pharmacy = pharmacy;
        }

        // =====================================================================
        // 1️⃣ PATIENT — Submit Rating
        // =====================================================================
        public async Task<string> SubmitRating(int patientId, SubmitRatingDto dto)
        {
            // Rating 1-5 check
            if (dto.Rating < 1 || dto.Rating > 5)
                throw new Exception("Rating must be between 1 and 5");

            // Order valid-ஆ check
            var order = await _order.Query()
                .Include(o => o.Quotation)
                .FirstOrDefaultAsync(o =>
                    o.Id == dto.PharmacyOrderId &&
                    o.PatientId == patientId &&
                    (o.Status == "Delivered" || o.Status == "Collected"))
                ?? throw new Exception(
                    "Order not found or not yet delivered/collected");

            // Already rated check
            var alreadyRated = await _rating.Query()
                .AnyAsync(r =>
                    r.PharmacyOrderId == dto.PharmacyOrderId &&
                    r.PatientId == patientId);

            if (alreadyRated)
                throw new Exception("You have already rated this order");

            // Save rating
            var rating = new PharmacyRating
            {
                ExternalPharmacyId = order.Quotation.ExternalPharmacyId,
                PatientId = patientId,
                PharmacyOrderId = dto.PharmacyOrderId,
                Rating = dto.Rating,
                Review = dto.Review,
                RatedAt = DateTime.UtcNow
            };

            await _rating.AddAsync(rating);
            await _rating.SaveAsync();

            // Update pharmacy average rating
            await UpdatePharmacyRating(order.Quotation.ExternalPharmacyId);

            return "Rating submitted successfully";
        }

        // =====================================================================
        // 2️⃣ PUBLIC — Get Pharmacy Rating Summary
        // =====================================================================
        public async Task<PharmacyRatingSummaryDto> GetRatingSummary(
            int externalPharmacyId)
        {
            var pharmacy = await _pharmacy.GetAsync(
                p => p.Id == externalPharmacyId)
                ?? throw new Exception("Pharmacy not found");

            var recentReviews = await _rating.Query()
                .Include(r => r.Patient).ThenInclude(p => p.User)
                .Where(r => r.ExternalPharmacyId == externalPharmacyId)
                .OrderByDescending(r => r.RatedAt)
                .Take(10)
                .Select(r => new PharmacyRatingViewDto(
                    r.Id,
                    r.Patient.User.Name,
                    r.Rating,
                    r.Review,
                    r.RatedAt
                ))
                .ToListAsync();

            return new PharmacyRatingSummaryDto(
                pharmacy.Id,
                pharmacy.PharmacyName,
                pharmacy.AverageRating,
                pharmacy.TotalRatings,
                recentReviews
            );
        }

        // =====================================================================
        // 3️⃣ PRODUCT ADMIN — Get Low Rated Pharmacies
        // =====================================================================
        public async Task<List<ExternalPharmacyListDto>> GetLowRatedPharmacies(
            double threshold = 3.0)
        {
            return await _pharmacy.Query()
                .Where(p =>
                    p.IsActive &&
                    p.TotalRatings >= 5 &&        // minimum 5 ratings வேணும்
                    p.AverageRating < threshold)   // threshold கீழே
                .OrderBy(p => p.AverageRating)
                .Select(p => new ExternalPharmacyListDto(
                    p.Id,
                    p.PharmacyName,
                    p.Address,
                    p.OffersHomeDelivery,
                    p.AverageRating,
                    p.Status
                ))
                .ToListAsync();
        }

        // =====================================================================
        // 🔧 PRIVATE — Update Pharmacy Average Rating
        // =====================================================================
        private async Task UpdatePharmacyRating(int externalPharmacyId)
        {
            var pharmacy = await _pharmacy.GetAsync(
                p => p.Id == externalPharmacyId)
                ?? throw new Exception("Pharmacy not found");

            var ratings = await _rating.Query()
                .Where(r => r.ExternalPharmacyId == externalPharmacyId)
                .ToListAsync();

            pharmacy.TotalRatings = ratings.Count;
            pharmacy.AverageRating = ratings.Count > 0
                ? Math.Round(ratings.Average(r => r.Rating), 2)
                : 0.0;

            await _pharmacy.SaveAsync();
        }
    }
}