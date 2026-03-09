using HospitalProject.Models;
using HospitalProject.Services;
using HospitalProject.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HospitalProject.Controllers
{
    [ApiController]
    [Route("api/internal-pharmacy")]
    public class InternalPharmacyController : ControllerBase
    {
        private readonly InternalPharmacyService _service;
        private readonly IRepository<User> _user;

        public InternalPharmacyController(
            InternalPharmacyService service,
            IRepository<User> user)
        {
            _service = service;
            _user = user;
        }

        // =========================
        // ADMIN CREATE INTERNAL PHARMACY
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            InternalPharmacyCreateDto dto)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            var result = await _service.RegisterInternalPharmacy(
                userId, role, dto);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = result
            });
        }

        // =========================
        // GET INTERNAL PHARMACY
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Admin-ஓட HospitalId JWT-ல் இருந்து எடுக்கறோம்
            var hospitalId = int.Parse(
                User.FindFirst("HospitalId")!.Value);

            var result = await _service.GetPharmacy(hospitalId);
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Pharmacy details fetched successfully",
                Data = result
            });
        }

        // =========================
        // STAFF REGISTER (PUBLIC)
        // =========================
        [HttpPost("staff/register")]
        public async Task<IActionResult> RegisterStaff(
            InternalPharmacyStaffRegisterDto dto)
        {
            var result = await _service.RegisterStaff(dto);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = result
            });
        }

        // =========================
        // ADMIN APPROVE STAFF
        // =========================
        [Authorize(Roles = "Admin")]
        [HttpPost("staff/approve")]
        public async Task<IActionResult> ApproveStaff(
            ApprovePharmacyStaffDto dto)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            var result = await _service.ApproveStaff(
                userId, role, dto.RequestId);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = result
            });
        }

        // =========================
        // MEDICINE MASTER
        // =========================
        [Authorize(Roles = "Admin,InternalPharmacyStaff")]
        [HttpPost("medicine")]
        public async Task<IActionResult> AddMedicine(MedicineCreateDto dto)
        {
            await _service.AddMedicine(dto);
            return Ok(new ApiResponse 
            { 
                Success = true, 
                Message = "Medicine added to master list" 
            });
        }

        [HttpGet("medicine")]
        public async Task<IActionResult> GetMedicines()
        {
            return Ok(new ApiResponse 
            { 
                Success = true, 
                Data = await _service.GetAllMedicines() 
            });
        }

        // =========================
        // INVENTORY
        // =========================
        [Authorize(Roles = "InternalPharmacyStaff")]
        [HttpPost("inventory")]
        public async Task<IActionResult> UpdateInventory(InventoryUpdateDto dto)
        {
            await _service.UpdateInventory(dto);
            return Ok(new ApiResponse 
            { 
                Success = true, 
                Message = "Inventory updated" 
            });
        }

        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            return Ok(new ApiResponse 
            { 
                Success = true, 
                Data = await _service.GetInventory() 
            });
        }

        // =========================
        // PHARMACY QUEUE
        // =========================
        [Authorize(Roles = "InternalPharmacyStaff")]
        [HttpGet("queue")]
        public async Task<IActionResult> GetQueue()
        {
            return Ok(new ApiResponse 
            { 
              Success = true, 
              Data = await _service.GetPendingPrescriptions() 
            });
        }

        // =========================
        // DISPENSE
        // =========================
        [Authorize(Roles = "InternalPharmacyStaff")]
        [HttpPost("dispense")]
        public async Task<IActionResult> Dispense(DispenseRequestDto dto)
        {
            try 
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var dispenseId = await _service.Dispense(userId, dto);
                return Ok(new ApiResponse 
                { 
                    Success = true, 
                    Message = "Medication dispensed successfully", 
                    Data = new { DispenseId = dispenseId } 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse 
                { 
                    Success = false, 
                    Message = ex.Message });
            }
        }

        [Authorize(Roles = "InternalPharmacyStaff,Admin,Doctor")]
        [HttpGet("dispense/receipt/{dispenseId}")]
        public async Task<IActionResult> GetReceipt(int dispenseId)
        {
            var result = await _service.GetDispenseReceipt(dispenseId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Receipt not found" });
            return Ok(new ApiResponse 
            { 
                Success = true, 
                Data = result 
            });
        }

        [Authorize(Roles = "InternalPharmacyStaff")]
        [HttpGet("inventory/barcode/{barcode}")]
        public async Task<IActionResult> GetByBarcode(string barcode)
        {
            var result = await _service.GetInventoryByBarcode(barcode);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Barcode not found or expired" });
            return Ok(new ApiResponse 
            { 
                Success = true, 
                Data = result 
            });
        }

        [Authorize(Roles = "InternalPharmacyStaff")]
        [HttpGet("alerts/low-stock")]
        public async Task<IActionResult> GetLowStockAlerts()
        {
            return Ok(new ApiResponse 
            { 
               Success = true, 
               Data = await _service.GetLowStockAlerts() 
            });
        }
        
        [Authorize(Roles = "Admin,Doctor")]
        [HttpGet("patient-history/{patientId}")]
        public async Task<IActionResult> GetPatientHistory(int patientId)
        {
            var result = await _service.GetPatientDispenseHistory(patientId);
            return Ok(new ApiResponse
            {
                Success = true,
                Data = result
            });
        }

        [Authorize(Roles = "Patient")]
        [HttpGet("patient/notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            // Get Patient ID from User ID
            var user = await _user.Query()
                .Include(u => u.Patient)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user?.Patient == null) return BadRequest(new ApiResponse { Success = false, Message = "Patient profile not found" });

            return Ok(new ApiResponse 
            { 
                Success = true, 
                Data = await _service.GetPatientNotifications(user.Patient.Id) 
            });
        }

        [Authorize(Roles = "Patient")]
        [HttpPost("patient/notifications/read/{id}")]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _service.MarkNotificationAsRead(id);
            return Ok(new ApiResponse 
            { 
              Success = true, 
              Message = "Notification marked as read" 
            });
        }

        // =========================
        // DRUG INTERACTIONS (ADMIN/STAFF)
        // =========================
        [Authorize(Roles = "Admin,InternalPharmacyStaff")]
        [HttpPost("interactions/add")]
        public async Task<IActionResult> AddInteraction(DrugInteractionCreateDto dto)
        {
            await _service.AddDrugInteraction(dto);
            return Ok(new ApiResponse 
            { 
                Success = true, 
                Message = "Drug interaction rule added" 
            });
        }

        [Authorize(Roles = "Admin,InternalPharmacyStaff,Doctor")]
        [HttpGet("interactions/list")]
        public async Task<IActionResult> ListInteractions()
        {
            return Ok(new ApiResponse 
            { 
                Success = true, 
                Data = await _service.GetAllInteractions() 
            });
        }
    }
}
