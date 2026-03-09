using HospitalProject.Models;
using HospitalProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalProject.Controllers
{
    [ApiController]
    [Route("api/prescription-qr")]
    public class PrescriptionQrController : ControllerBase
    {
        private readonly PrescriptionQrService _qrService;

        public PrescriptionQrController(PrescriptionQrService qrService)
        {
            _qrService = qrService;
        }

        // =====================================================================
        // 👤 PATIENT — Own QR view
        // GET /api/prescription-qr/{prescriptionId}
        // =====================================================================
        [Authorize(Roles = "Patient")]
        [HttpGet("{prescriptionId}")]
        public async Task<IActionResult> GetMyQr(int prescriptionId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await _qrService.GetQrForPatient(prescriptionId, userId);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "QR Code fetched successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        // =====================================================================
        // 🏥 PHARMACY — QR Scan
        // POST /api/prescription-qr/scan
        // =====================================================================
        [Authorize(Roles = "InternalPharmacyStaff,ExternalPharmacy")]
        [HttpPost("scan")]
        public async Task<IActionResult> ScanQr([FromBody] QrScanRequestDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await _qrService.ScanQrCode(dto.QrPayload, userId, dto.PharmacyName);

                if (!result.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = result.InvalidReason ?? "QR validation failed",
                        Data = result
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = $"QR scanned. Refill #{result.RefillNumber}. Remaining: {result.RefillsRemaining}",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        // =====================================================================
        // 👨‍⚕️ DOCTOR / ADMIN — Scan audit log
        // GET /api/prescription-qr/{prescriptionId}/scan-logs
        // =====================================================================
        [Authorize(Roles = "Doctor,Admin,InternalPharmacyStaff")]
        [HttpGet("{prescriptionId}/scan-logs")]
        public async Task<IActionResult> GetScanLogs(int prescriptionId)
        {
            try
            {
                var logs = await _qrService.GetScanLogs(prescriptionId);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Scan logs fetched",
                    Data = logs
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }
    }
}