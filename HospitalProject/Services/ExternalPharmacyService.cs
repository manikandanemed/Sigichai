using HospitalProject.Models;
using HospitalProject.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HospitalProject.Services
{
    public class ExternalPharmacyService
    {
        private readonly IRepository<ExternalPharmacy> _pharmacy;
        private readonly IRepository<ExternalPharmacyDocument> _document;
        private readonly IConfiguration _config;

        public ExternalPharmacyService(
            IRepository<ExternalPharmacy> pharmacy,
            IRepository<ExternalPharmacyDocument> document,
            IConfiguration config)
        {
            _pharmacy = pharmacy;
            _document = document;
            _config = config;
        }

        // =====================================================================
        // 1️⃣ REGISTER (Public)
        // =====================================================================
        public async Task<string> Register(ExternalPharmacyRegisterDto dto)
        {
            // Duplicate check
            var exists = await _pharmacy.Query()
                .FirstOrDefaultAsync(x => x.MobileNumber == dto.MobileNumber);
            if (exists != null)
                throw new Exception("Mobile number already registered");

            var pharmacy = new ExternalPharmacy
            {
                PharmacyName = dto.PharmacyName,
                OwnerName = dto.OwnerName,
                MobileNumber = dto.MobileNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Address = dto.Address,
                LicenseNumber = dto.LicenseNumber,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                OffersHomeDelivery = dto.OffersHomeDelivery,
                DeliveryRadius = dto.DeliveryRadius,
                Status = "Pending",
                IsActive = false,
                RegisteredAt = DateTime.UtcNow
            };

            await _pharmacy.AddAsync(pharmacy);
            await _pharmacy.SaveAsync();

            return "Registration successful. Waiting for admin approval.";
        }

        // =====================================================================
        // 2️⃣ LOGIN
        // =====================================================================
        public async Task<string> Login(ExternalPharmacyLoginDto dto)
        {
            var pharmacy = await _pharmacy.Query()
                .FirstOrDefaultAsync(x => x.MobileNumber == dto.MobileNumber);

            if (pharmacy == null)
                throw new Exception("Pharmacy not found");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, pharmacy.Password))
                throw new Exception("Invalid password");

            if (pharmacy.Status != "Approved")
                throw new Exception("Pharmacy not approved yet");

            if (!pharmacy.IsActive)
                throw new Exception("Pharmacy account is inactive");

            return GenerateToken(pharmacy);
        }

        // =====================================================================
        // 3️⃣ PRODUCT ADMIN — Get All Pending
        // =====================================================================
        public async Task<List<ExternalPharmacyListDto>> GetPendingPharmacies()
        {
            return await _pharmacy.Query()
                .Where(x => x.Status == "Pending")
                .OrderBy(x => x.RegisteredAt)
                .Select(x => new ExternalPharmacyListDto(
                    x.Id,
                    x.PharmacyName,
                    x.Address,
                    x.OffersHomeDelivery,
                    x.AverageRating,
                    x.Status
                ))
                .ToListAsync();
        }

        // =====================================================================
        // 4️⃣ PRODUCT ADMIN — Get All Pharmacies
        // =====================================================================
        public async Task<List<ExternalPharmacyListDto>> GetAllPharmacies()
        {
            return await _pharmacy.Query()
                .OrderByDescending(x => x.RegisteredAt)
                .Select(x => new ExternalPharmacyListDto(
                    x.Id,
                    x.PharmacyName,
                    x.Address,
                    x.OffersHomeDelivery,
                    x.AverageRating,
                    x.Status
                ))
                .ToListAsync();
        }

        // =====================================================================
        // 5️⃣ PRODUCT ADMIN — Approve / Reject
        // =====================================================================
        public async Task<string> ApproveOrReject(ExternalPharmacyApproveDto dto)
        {
            var pharmacy = await _pharmacy.GetAsync(x => x.Id == dto.ExternalPharmacyId)
                ?? throw new Exception("Pharmacy not found");

            if (pharmacy.Status != "Pending")
                throw new Exception("Pharmacy already processed");

            if (dto.IsApproved)
            {
                pharmacy.Status = "Approved";
                pharmacy.IsActive = true;
                await _pharmacy.SaveAsync();
                return "Pharmacy approved successfully";
            }
            else
            {
                pharmacy.Status = "Rejected";
                pharmacy.IsActive = false;
                pharmacy.RejectionReason = dto.RejectionReason;
                await _pharmacy.SaveAsync();
                return "Pharmacy rejected";
            }
        }

        // =====================================================================
        // 6️⃣ GET SINGLE PHARMACY DETAILS
        // =====================================================================
        public async Task<ExternalPharmacyViewDto> GetById(int id)
        {
            var x = await _pharmacy.GetAsync(p => p.Id == id)
                ?? throw new Exception("Pharmacy not found");

            return new ExternalPharmacyViewDto(
                x.Id,
                x.PharmacyName,
                x.OwnerName,
                x.MobileNumber,
                x.Address,
                x.LicenseNumber,
                x.Latitude,
                x.Longitude,
                x.OffersHomeDelivery,
                x.DeliveryRadius,
                x.Status,
                x.RejectionReason,
                x.RegisteredAt,
                x.IsActive,
                x.AverageRating,
                x.TotalRatings
            );
        }

        // =====================================================================
        // 🔧 PRIVATE — JWT Token Generate
        // =====================================================================
        private string GenerateToken(ExternalPharmacy pharmacy)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, pharmacy.Id.ToString()),
                new Claim(ClaimTypes.Name,           pharmacy.PharmacyName),
                new Claim(ClaimTypes.Role,           "ExternalPharmacy")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}