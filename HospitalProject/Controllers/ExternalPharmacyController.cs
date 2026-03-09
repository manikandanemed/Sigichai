using HospitalProject.Models;
using HospitalProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalProject.Controllers
{
    [ApiController]
    [Route("api/external-pharmacy")]
    public class ExternalPharmacyController : ControllerBase
    {
        private readonly ExternalPharmacyService _service;

        public ExternalPharmacyController(ExternalPharmacyService service)
        {
            _service = service;
        }

        // =====================================================================
        // 🌐 PUBLIC — Register
        // POST /api/external-pharmacy/register
        // =====================================================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] ExternalPharmacyRegisterDto dto)
        {
            try
            {
                var result = await _service.Register(dto);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        // =====================================================================
        // 🌐 PUBLIC — Login
        // POST /api/external-pharmacy/login
        // =====================================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ExternalPharmacyLoginDto dto)
        {
            try
            {
                var token = await _service.Login(dto);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Data = new { Token = token }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        // =====================================================================
        // 🔐 PRODUCT ADMIN — Get Pending List
        // GET /api/external-pharmacy/pending
        // =====================================================================
        [Authorize(Roles = "ProductAdmin")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            try
            {
                var result = await _service.GetPendingPharmacies();
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        // =====================================================================
        // 🔐 PRODUCT ADMIN — Get All
        // GET /api/external-pharmacy/all
        // =====================================================================
        [Authorize(Roles = "ProductAdmin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllPharmacies();
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        // =====================================================================
        // 🔐 PRODUCT ADMIN — Approve / Reject
        // POST /api/external-pharmacy/approve
        // =====================================================================
        [Authorize(Roles = "ProductAdmin")]
        [HttpPost("approve")]
        public async Task<IActionResult> ApproveOrReject([FromBody] ExternalPharmacyApproveDto dto)
        {
            try
            {
                var result = await _service.ApproveOrReject(dto);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        // =====================================================================
        // 🔐 PRODUCT ADMIN / ExternalPharmacy — Get Details
        // GET /api/external-pharmacy/{id}
        // =====================================================================
        [Authorize(Roles = "ProductAdmin,ExternalPharmacy")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetById(id);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }
    }
}