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
    [Route("api/pharmacy-rating")]
    public class PharmacyRatingController : ControllerBase
    {
        private readonly PharmacyRatingService _service;
        private readonly IRepository<User> _user;

        public PharmacyRatingController(
            PharmacyRatingService service,
            IRepository<User> user)
        {
            _service = service;
            _user = user;
        }

        // =====================================================================
        // 👤 PATIENT — Submit Rating
        // POST /api/pharmacy-rating/submit
        // =====================================================================
        [Authorize(Roles = "Patient")]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitRating(
            [FromBody] SubmitRatingDto dto)
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

                var result = await _service.SubmitRating(
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

        // =====================================================================
        // 🌐 PUBLIC — Get Pharmacy Rating Summary
        // GET /api/pharmacy-rating/{externalPharmacyId}
        // =====================================================================
        [HttpGet("{externalPharmacyId}")]
        public async Task<IActionResult> GetRatingSummary(int externalPharmacyId)
        {
            try
            {
                var result = await _service.GetRatingSummary(externalPharmacyId);
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
        // 🔐 PRODUCT ADMIN — Get Low Rated Pharmacies
        // GET /api/pharmacy-rating/low-rated?threshold=3.0
        // =====================================================================
        [Authorize(Roles = "ProductAdmin")]
        [HttpGet("low-rated")]
        public async Task<IActionResult> GetLowRated(
            [FromQuery] double threshold = 3.0)
        {
            try
            {
                var result = await _service.GetLowRatedPharmacies(threshold);
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