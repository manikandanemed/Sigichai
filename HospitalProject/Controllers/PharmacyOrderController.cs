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
    [Route("api/pharmacy-order")]
    public class PharmacyOrderController : ControllerBase
    {
        private readonly PharmacyOrderService _service;
        private readonly IRepository<User> _user;

        public PharmacyOrderController(
            PharmacyOrderService service,
            IRepository<User> user)
        {
            _service = service;
            _user = user;
        }

        // =====================================================================
        // 👤 PATIENT — Place Order
        // POST /api/pharmacy-order/place
        // =====================================================================
        [Authorize(Roles = "Patient")]
        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderDto dto)
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

                var orderId = await _service.PlaceOrder(user.Patient.Id, dto);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Order placed successfully",
                    Data = new { OrderId = orderId }
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
        // 👤 PATIENT — View Order
        // GET /api/pharmacy-order/{orderId}
        // =====================================================================
        [Authorize(Roles = "Patient")]
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder(int orderId)
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

                var result = await _service.GetOrder(orderId, user.Patient.Id);

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
        // 👤 PATIENT — Mark Payment Done
        // POST /api/pharmacy-order/{orderId}/payment-done
        // =====================================================================
        [Authorize(Roles = "Patient")]
        [HttpPost("{orderId}/payment-done")]
        public async Task<IActionResult> MarkPaymentDone(int orderId)
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

                var result = await _service.MarkPaymentDone(
                    orderId, user.Patient.Id);

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
        // 🏥 EXTERNAL PHARMACY — View All Orders
        // GET /api/pharmacy-order/pharmacy/orders
        // =====================================================================
        [Authorize(Roles = "ExternalPharmacy")]
        [HttpGet("pharmacy/orders")]
        public async Task<IActionResult> GetPharmacyOrders()
        {
            try
            {
                var pharmacyId = int.Parse(
                    User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var result = await _service.GetPharmacyOrders(pharmacyId);

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
        // 🏥 EXTERNAL PHARMACY — Update Order Status
        // POST /api/pharmacy-order/status/update
        // =====================================================================
        [Authorize(Roles = "ExternalPharmacy")]
        [HttpPost("status/update")]
        public async Task<IActionResult> UpdateStatus(
            [FromBody] UpdateOrderStatusDto dto)
        {
            try
            {
                var pharmacyId = int.Parse(
                    User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var result = await _service.UpdateStatus(pharmacyId, dto);

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