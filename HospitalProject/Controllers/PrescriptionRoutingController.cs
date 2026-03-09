using HospitalProject.Models;
using HospitalProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HospitalProject.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalProject.Controllers
{
    [ApiController]
    [Route("api/prescription-routing")]
    public class PrescriptionRoutingController : ControllerBase
    {
        private readonly PrescriptionRoutingService _service;
        private readonly IRepository<User> _user;

        public PrescriptionRoutingController(
            PrescriptionRoutingService service,
            IRepository<User> user)
        {
            _service = service;
            _user = user;
        }

        // =====================================================================
        // 👤 PATIENT — Set Preferred Pharmacy
        // POST /api/prescription-routing/preferred-pharmacy
        // =====================================================================
        [Authorize(Roles = "Patient")]
        [HttpPost("preferred-pharmacy")]
        public async Task<IActionResult> SetPreferred(
            [FromBody] SetPreferredPharmacyDto dto)
        {
            try
            {
                var userId = int.Parse(
                    User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var user = await _user.Query()
                    .Include(u => u.Patient)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user?.Patient == null)
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Patient profile not found"
                    });

                await _service.SetPreferredPharmacy(
                    user.Patient.Id, dto.ExternalPharmacyId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Preferred pharmacy set successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // =====================================================================
        // 👤 PATIENT — View Prescription Routes
        // GET /api/prescription-routing/{prescriptionId}
        // =====================================================================
        [Authorize(Roles = "Patient,Doctor,Admin")]
        [HttpGet("{prescriptionId}")]
        public async Task<IActionResult> GetRoutes(int prescriptionId)
        {
            try
            {
                var result = await _service.GetRoutes(prescriptionId);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // =====================================================================
        // 👤 PATIENT — View Nearby Pharmacies
        // GET /api/prescription-routing/nearby?lat=xx&lon=xx
        // =====================================================================
        [Authorize(Roles = "Patient")]
        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearby(
            [FromQuery] double lat,
            [FromQuery] double lon)
        {
            try
            {
                var result = await _service.GetNearbyPharmacies(lat, lon);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}