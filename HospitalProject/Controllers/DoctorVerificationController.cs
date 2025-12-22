using HospitalProject.Models;
using HospitalProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HospitalProject.Controllers
{
    [ApiController]
    [Route("api/doctor-verification")]
    public class DoctorVerificationController : ControllerBase
    {
        private readonly DoctorVerificationService _service;

        public DoctorVerificationController(
            DoctorVerificationService service)
        {
            _service = service;
        }

        // ADMIN ONLY
        [Authorize(Roles = "Admin")]
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyDoctor(
            DoctorVerificationDto dto)
        {
            await _service.VerifyDoctorAsync(dto);
            return Ok("Doctor verified successfully");
        }
    }
}
