using HospitalProject.Services;
using HospitalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalProject.Controllers
{
    [ApiController]
    [Route("api/medicalrep")]
    public class MedicalRepController : ControllerBase
    {
        private readonly HospitalService _service;

        public MedicalRepController(HospitalService service)
        {
            _service = service;
        }

        // ======================================
        // 🔐 MEDICAL REP SIGNUP + PROFILE CREATE
        // ======================================
        [HttpPost("register")]
        public async Task<IActionResult> RegisterMedicalRep(
            [FromBody] MedicalRepProfileDto dto)
        {
            await _service.RegisterMedicalRep(dto);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Medical Representative registered successfully"
            });
        }

        // =====================================
        // 1️⃣ VIEW AVAILABLE SLOTS
        // doctorId + date
        // =====================================
        [Authorize(Roles = "MedicalRep")]
        [HttpGet("slots")]
        public async Task<IActionResult> GetAvailableSlots(
            [FromQuery] int doctorId,
            [FromQuery] DateOnly date)
        {
            var slots = await _service.GetMedicalRepSlots(doctorId, date);

            var data = slots.Select(s => new MedicalRepSlotResponseDto
            {
                SlotId = s.Id,
                TimeSlot = s.TimeSlot
            }).ToList();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Available medical rep slots fetched",
                Data = data
            });
        }


        // ======================================
        // 👨‍⚕️ GET DOCTOR LIST FOR MEDICAL REP
        // ======================================
        [Authorize(Roles = "MedicalRep")]
        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var data = await _service.GetDoctorsForMedicalRep();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Doctors fetched successfully",
                Data = data
            });
        }


        // =====================================
        // 2️⃣ BOOK SLOT BY TIME
        // doctorId + date + timeSlot
        // =====================================
        [Authorize(Roles = "MedicalRep")]
        [HttpPost("book")]
        public async Task<IActionResult> Book(
        MedicalRepTimeBookingDto dto)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var token = await _service.BookMedicalRepByTime(userId, dto);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Medical rep appointment booked",
                Data = new { TempToken = token }
            });
        }


        // CHECK-IN
        [Authorize(Roles = "MedicalRep")]
        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromQuery] string tempToken)
        {
            var queue = await _service.CheckInMedicalRep(tempToken);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Checked in successfully",
                Data = new { QueueToken = queue }
            });
        }



        // ======================================
        // ➕ ADD MEDICAL REP SLOT (Doctor / Admin)
        // ======================================
        [Authorize(Roles = "Doctor,Admin")]
        [HttpPost("add-slot")]
        public async Task<IActionResult> AddMedicalRepSlot(
            [FromBody] MedicalRepSlotCreateDto dto)
        {
            await _service.AddMedicalRepSlot(dto);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Medical rep slot created successfully"
            });
        }


        // ======================================
        // 🩺 DOCTOR CONSULT – MEDICAL REP
        // ======================================
        [Authorize(Roles = "Doctor")]
        [HttpPost("consult")]
        public async Task<IActionResult> ConsultMedicalRep(
            [FromBody] MedicalRepConsultDto dto)
        {
            await _service.ConsultMedicalRep(dto);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Medical representative consultation completed",
                Data = null
            });
        }


        // ======================================
        // 👨‍⚕️👩‍💼 VIEW MEDICAL REP BOOKINGS
        // Doctor / Admin
        // ======================================
        [Authorize(Roles = "Doctor,Admin")]
        [HttpGet("bookings")]
        public async Task<IActionResult> GetMedicalRepBookings(
    [FromQuery] int? doctorId,
    [FromQuery] DateOnly? date,
    [FromQuery] string? timeSlot)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            string role = User.FindFirstValue(ClaimTypes.Role)!;

            var data = await _service.GetMedicalRepBookings(
                userId,
                role,
                doctorId,
                date,
                timeSlot);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Medical rep bookings fetched successfully",
                Data = data
            });
        }



        // ======================================
        // 📋 MEDICAL REP - MY APPOINTMENTS
        // ======================================
   

        [Authorize(Roles = "MedicalRep")]
        [HttpGet("my-appointments")]
        public async Task<IActionResult> GetMyAppointments(
    [FromQuery] DateOnly? date)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var data = await _service
                .GetMedicalRepAppointments(userId, date);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Booked appointments fetched",
                Data = data
            });
        }




        // ======================================
        // ⛔ END MEDICAL REP SESSION BY SLOT
        // Doctor / Admin
        // ======================================
        [Authorize(Roles = "Doctor,Admin")]
        [HttpPost("session/end-by-slot")]
        public async Task<IActionResult> EndMedicalRepSessionBySlot(
            [FromBody] EndMedicalRepSessionBySlotDto dto)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            string role = User.FindFirstValue(ClaimTypes.Role)!;

            await _service.EndMedicalRepSessionBySlot(
                userId,
                role,
                dto.SlotId);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Medical rep session ended successfully"
            });
        }



    }
}
