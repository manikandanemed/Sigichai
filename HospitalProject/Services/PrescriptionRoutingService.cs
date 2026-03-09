using HospitalProject.Models;
using HospitalProject.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalProject.Services
{
    public class PrescriptionRoutingService
    {
        private readonly IRepository<Prescription> _prescription;
        private readonly IRepository<PrescriptionRoute> _route;
        private readonly IRepository<PatientPreferredPharmacy> _preferred;
        private readonly IRepository<InternalPharmacy> _internalPharmacy;
        private readonly IRepository<ExternalPharmacy> _externalPharmacy;
        private readonly IRepository<Patient> _patient;

        public PrescriptionRoutingService(
            IRepository<Prescription> prescription,
            IRepository<PrescriptionRoute> route,
            IRepository<PatientPreferredPharmacy> preferred,
            IRepository<InternalPharmacy> internalPharmacy,
            IRepository<ExternalPharmacy> externalPharmacy,
            IRepository<Patient> patient)
        {
            _prescription = prescription;
            _route = route;
            _preferred = preferred;
            _internalPharmacy = internalPharmacy;
            _externalPharmacy = externalPharmacy;
            _patient = patient;
        }

        // =====================================================================
        // 1️⃣ ROUTE PRESCRIPTION — Doctor consult முடிஞ்சதும் auto call ஆகும்
        // =====================================================================
        public async Task RoutePrescription(int prescriptionId)
        {
            var prescription = await _prescription.Query()
                .Include(p => p.Patient)
                    .ThenInclude(pat => pat.User)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(p => p.Id == prescriptionId)
                ?? throw new Exception("Prescription not found");

            // Already routed check
            var alreadyRouted = await _route.Query()
                .AnyAsync(r => r.PrescriptionId == prescriptionId);
            if (alreadyRouted) return;

            // 1️⃣ Internal Pharmacy — Doctor-ஓட Hospital வச்சு எடுக்கறோம்
            var hospitalId = prescription.Appointment?.Doctor?.HospitalId;

            var internalPharmacy = hospitalId.HasValue
                ? await _internalPharmacy.GetAsync(
                    x => x.HospitalId == hospitalId.Value && x.IsActive)
                : null;

            if (internalPharmacy != null)
            {
                await _route.AddAsync(new PrescriptionRoute
                {
                    PrescriptionId = prescriptionId,
                    PharmacyType = "Internal",
                    InternalPharmacyId = internalPharmacy.Id,
                    Status = "Pending",
                    RoutedAt = DateTime.UtcNow
                });
            }

            // 2️⃣ Patient preferred external pharmacy
            var preferred = await _preferred.Query()
                .Include(p => p.ExternalPharmacy)
                .FirstOrDefaultAsync(p =>
                    p.PatientId == prescription.PatientId &&
                    p.ExternalPharmacy.IsActive &&
                    p.ExternalPharmacy.Status == "Approved");

            if (preferred != null)
            {
                await _route.AddAsync(new PrescriptionRoute
                {
                    PrescriptionId = prescriptionId,
                    PharmacyType = "External",
                    ExternalPharmacyId = preferred.ExternalPharmacyId,
                    Status = "Pending",
                    RoutedAt = DateTime.UtcNow
                });
            }
            else
            {
                // 3️⃣ No preferred pharmacy → 5 nearby pharmacies
                var patient = await _patient.Query()
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == prescription.PatientId);

                if (patient?.User?.Latitude != null && patient.User.Longitude != null)
                {
                    var nearbyPharmacies = await GetNearbyPharmacies(
                        patient.User.Latitude.Value,
                        patient.User.Longitude.Value,
                        count: 5);

                    foreach (var pharmacy in nearbyPharmacies)
                    {
                        await _route.AddAsync(new PrescriptionRoute
                        {
                            PrescriptionId = prescriptionId,
                            PharmacyType = "External",
                            ExternalPharmacyId = pharmacy.ExternalPharmacyId,
                            Status = "Pending",
                            RoutedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await _route.SaveAsync();
        }

        // =====================================================================
        // 2️⃣ PATIENT — Set Preferred Pharmacy
        // =====================================================================
        public async Task SetPreferredPharmacy(int patientId, int externalPharmacyId)
        {
            // Pharmacy approved-ஆ check பண்றோம்
            var pharmacy = await _externalPharmacy.GetAsync(
                x => x.Id == externalPharmacyId && x.Status == "Approved" && x.IsActive)
                ?? throw new Exception("Pharmacy not found or not approved");

            // Already set-ஆ இருந்தா update பண்றோம்
            var existing = await _preferred.Query()
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (existing != null)
            {
                existing.ExternalPharmacyId = externalPharmacyId;
                existing.SetAt = DateTime.UtcNow;
            }
            else
            {
                await _preferred.AddAsync(new PatientPreferredPharmacy
                {
                    PatientId = patientId,
                    ExternalPharmacyId = externalPharmacyId,
                    SetAt = DateTime.UtcNow
                });
            }

            await _preferred.SaveAsync();
        }

        // =====================================================================
        // 3️⃣ PATIENT — View Prescription Routes
        // =====================================================================
        public async Task<List<PrescriptionRouteViewDto>> GetRoutes(int prescriptionId)
        {
            return await _route.Query()
                .Include(r => r.InternalPharmacy)
                .Include(r => r.ExternalPharmacy)
                .Where(r => r.PrescriptionId == prescriptionId)
                .Select(r => new PrescriptionRouteViewDto(
                    r.Id,
                    r.PrescriptionId,
                    r.PharmacyType,
                    r.PharmacyType == "Internal"
                        ? r.InternalPharmacy!.PharmacyName
                        : r.ExternalPharmacy!.PharmacyName,
                    r.Status,
                    r.RoutedAt,
                    r.RespondedAt
                ))
                .ToListAsync();
        }

        // =====================================================================
        // 4️⃣ NEARBY PHARMACIES — Haversine formula
        // =====================================================================
        public async Task<List<NearbyPharmacyDto>> GetNearbyPharmacies(
            double lat, double lon, int count = 5)
        {
            var pharmacies = await _externalPharmacy.Query()
                .Where(x => x.IsActive && x.Status == "Approved")
                .ToListAsync();

            return pharmacies
                .Select(p => new NearbyPharmacyDto(
                    p.Id,
                    p.PharmacyName,
                    p.Address,
                    p.Latitude,
                    p.Longitude,
                    CalculateDistance(lat, lon, p.Latitude, p.Longitude),
                    p.OffersHomeDelivery,
                    p.AverageRating
                ))
                .OrderBy(p => p.DistanceKm)
                .Take(count)
                .ToList();
        }

        // =====================================================================
        // 🔧 PRIVATE — Haversine Distance Calculator (KM)
        // =====================================================================
        private static double CalculateDistance(
            double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth radius KM
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return Math.Round(R * c, 2);
        }

        private static double ToRad(double deg) => deg * Math.PI / 180;
    }
}