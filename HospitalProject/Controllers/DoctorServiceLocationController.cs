using HospitalProject.Models;
using HospitalProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalProject.Controllers
{
    [ApiController]
    [Route("api/doctor-service-location")]
    public class DoctorServiceLocationController : ControllerBase
    {
        private readonly DoctorServiceLocationService _service;

        public DoctorServiceLocationController(
            DoctorServiceLocationService service)
        {
            _service = service;
        }

        // =====================================================================
        // 🌐 PUBLIC — Get States
        // GET /api/doctor-service-location/states
        // =====================================================================
        [HttpGet("states")]
        public async Task<IActionResult> GetStates()
        {
            try
            {
                var result = await _service.GetStates();
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
        // 🌐 PUBLIC — Get Areas by State
        // GET /api/doctor-service-location/areas?state=TamilNadu
        // =====================================================================
        [HttpGet("areas")]
        public async Task<IActionResult> GetAreas([FromQuery] string state)
        {
            try
            {
                var result = await _service.GetAreasByState(state);
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
        // 🌐 PUBLIC — Get Hospitals by State + Area
        // GET /api/doctor-service-location/hospitals?state=TamilNadu&area=Chennai
        // =====================================================================
        [HttpGet("hospitals")]
        public async Task<IActionResult> GetHospitals(
            [FromQuery] string state,
            [FromQuery] string area)
        {
            try
            {
                var result = await _service.GetHospitals(state, area);
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
        // 👨‍⚕️ DOCTOR — Save Service Locations
        // POST /api/doctor-service-location/save
        // =====================================================================
        [Authorize(Roles = "Doctor")]
        [HttpPost("save")]
        public async Task<IActionResult> SaveLocations(
            [FromBody] SaveServiceLocationsDto dto)
        {
            try
            {
                var userId = int.Parse(
                    User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                await _service.SaveServiceLocations(userId, dto);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Service locations saved successfully"
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
        // 👨‍⚕️ DOCTOR — View My Service Locations
        // GET /api/doctor-service-location/my
        // =====================================================================
        [Authorize(Roles = "Doctor")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyLocations()
        {
            try
            {
                var userId = int.Parse(
                    User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var result = await _service.GetMyLocations(userId);

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