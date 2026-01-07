using HospitalProject.Models;
using HospitalProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalProject.Controllers
{
    [ApiController]
    [Route("api/doctor-verification")]
    public class DoctorManualVerificationController : ControllerBase
    {
        private readonly DoctorManualVerificationService _service;

        public DoctorManualVerificationController(
            DoctorManualVerificationService service)
        {
            _service = service;
        }

        // =========================
        // Doctor submits verification
        // =========================
        [Authorize(Roles = "Doctor")]
        [HttpPost("submit")]
        public async Task<IActionResult> Submit(
            DoctorVerificationDto dto)
        {
            int doctorUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _service.SubmitVerificationAsync(
                doctorUserId, dto);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Verification submitted. Approval may take up to 4 hours."
            });
        }


        // =========================
        // Admin – View doctor verification details
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpGet("details/{doctorId}")]
        public async Task<IActionResult> GetDoctorDetails(int doctorId)
        {
            var details = await _service
                .GetDoctorVerificationDetails(doctorId);

            return Ok(new ApiResponse
            {
                Success = true,
                Data = details
            });

        }


        // =========================
        // Admin approve / reject
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpPost("approve/{doctorId}")]
        public async Task<IActionResult> Approve(
            int doctorId,
            [FromQuery] bool approve)
        {
            await _service.ApproveDoctorAsync(
                doctorId, approve);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = approve ? "Doctor approved" : "Doctor rejected"
            });

        }

        // =========================
        // Admin – Pending doctors
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpGet("pending")]
        public IActionResult PendingDoctors()
        {
            int hospitalId = int.Parse(
                User.FindFirst("HospitalId")!.Value);

            return Ok(new ApiResponse
            {
                Success = true,
                Data = _service.GetPendingDoctors(hospitalId)
            });

        }
    }
}
