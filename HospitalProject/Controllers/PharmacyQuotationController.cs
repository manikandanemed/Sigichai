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
    [Route("api/pharmacy-quotation")]
    public class PharmacyQuotationController : ControllerBase
    {
        private readonly PharmacyQuotationService _service;
        private readonly IRepository<User> _user;

        public PharmacyQuotationController(
            PharmacyQuotationService service,
            IRepository<User> user)
        {
            _service = service;
            _user = user;
        }

        // =====================================================================
        // 🏥 EXTERNAL PHARMACY — View Pending Prescriptions to Quote
        // GET /api/pharmacy-quotation/pending
        // =====================================================================
        [Authorize(Roles = "ExternalPharmacy")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRoutes()
        {
            try
            {
                var pharmacyId = int.Parse(
                    User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var result = await _service.GetPendingRoutes(pharmacyId);
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
        // 🏥 EXTERNAL PHARMACY — Submit Quotation
        // POST /api/pharmacy-quotation/submit
        // =====================================================================
        [Authorize(Roles = "ExternalPharmacy")]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitQuotation(
            [FromBody] SubmitQuotationDto dto)
        {
            try
            {
                var pharmacyId = int.Parse(
                    User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var result = await _service.SubmitQuotation(pharmacyId, dto);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = result
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
        // 👤 PATIENT — View All Quotations
        // GET /api/pharmacy-quotation/{prescriptionId}
        // =====================================================================
        [Authorize(Roles = "Patient")]
        [HttpGet("{prescriptionId}")]
        public async Task<IActionResult> GetQuotations(int prescriptionId)
        {
            try
            {
                var result = await _service.GetQuotations(prescriptionId);
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
        // 👤 PATIENT — Select Quotation
        // POST /api/pharmacy-quotation/select
        // =====================================================================
        [Authorize(Roles = "Patient")]
        [HttpPost("select")]
        public async Task<IActionResult> SelectQuotation(
            [FromBody] SelectQuotationDto dto)
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

                var result = await _service.SelectQuotation(
                    user.Patient.Id, dto);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = result
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